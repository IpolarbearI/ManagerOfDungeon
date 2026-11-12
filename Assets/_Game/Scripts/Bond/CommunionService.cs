using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 교감 처리 (명세 6.1): 입력 검증 → 프롬프트 생성 → LLM → JSON 검증 → (대화 종료 시) 기억 저장 → EXP 정산.
    /// - LLM 호출은 비동기. 메인 스레드를 막지 않는다.
    /// - 실제 LLM 요청이 실패(시간 초과·형식 오류)하면 준비된 대사를 보여주되 EXP·기억·기회를 반영하지 않는다.
    /// - AI 대화 꺼짐/서버 사용 불가 상태에서는 준비된 대사가 '정상 응답'으로 처리된다(EXP 지급).
    ///   [결정 필요] 명세는 이 경우를 따로 정하지 않음. 저사양 PC 플레이어가 성장할 수 없게 되는 것을 막기 위한 해석.
    /// </summary>
    public sealed class CommunionService : ICommunionSession
    {
        readonly ILlmClient _llm;
        readonly IMonsterMemoryStore _memory;
        readonly CommunionPromptBuilder _prompts;
        readonly ICommunionLedger _ledger;
        readonly WorryChanceTracker _worryChance;

        CommunionRequest _pending;
        CommunionResult _pendingResult;
        int _pendingDay;
        bool _busy;

        public CommunionService()
            : this(new FakeLlmClient(), new InMemoryMonsterMemoryStore(), new CommunionPromptBuilder(), new InMemoryCommunionLedger())
        {
        }

        public CommunionService(ILlmClient llm, IMonsterMemoryStore memory, CommunionPromptBuilder prompts)
            : this(llm, memory, prompts, new InMemoryCommunionLedger())
        {
        }

        public CommunionService(ILlmClient llm, IMonsterMemoryStore memory, CommunionPromptBuilder prompts, ICommunionLedger ledger)
        {
            _llm = llm ?? new FakeLlmClient();
            _memory = memory ?? new InMemoryMonsterMemoryStore();
            _prompts = prompts ?? new CommunionPromptBuilder();
            _ledger = ledger ?? new InMemoryCommunionLedger();
            _worryChance = new WorryChanceTracker(_ledger);
        }

        /// <summary>본 게임용: 기록·기억을 모두 GameState에 남기고, LLM은 LlmRuntime(로컬 서버/준비된 대사)을 쓴다.</summary>
        public static CommunionService ForGame(GameState state, ILlmClient llm = null)
        {
            var store = new GameStateCommunion(state);
            return new CommunionService(llm ?? LlmRuntime.Client, store, new CommunionPromptBuilder(), store);
        }

        public IMonsterMemoryStore Memory => _memory;
        public bool HasPending => _pending != null;
        public CommunionRequest Pending => _pending;
        public CommunionResult PendingResult => _pendingResult;
        public bool IsBusy => _busy;

        public bool CanTalk(string actorId, int dayIndex)
        {
            if (string.IsNullOrEmpty(actorId))
                return false;

            int used = _ledger.GetTalksUsed(actorId, dayIndex);
            int bonus = _ledger.GetGiftBonus(actorId);
            return used < BondRules.TalksPerDay + bonus;
        }

        public bool CanWorry(string topicId, int otherMonsterCount)
        {
            return WorryCatalog.CanOccur(topicId, otherMonsterCount);
        }

        public Task<CommunionResult> StartTalkAsync(CommunionRequest request, CancellationToken ct = default)
        {
            if (request == null || request.Profile == null || request.Bond == null)
                return Done(CommunionResult.Fail("대화 대상이 없습니다."));
            if (!TalkCatalog.TryGet(request.TopicId, out var topic))
                return Done(CommunionResult.Fail("알 수 없는 대화 주제입니다."));
            if (!CanTalk(request.Profile.ActorId, request.DayIndex))
                return Done(CommunionResult.Fail("오늘은 이미 대화했습니다."));

            return BeginAsync(request, topic.Title, BondRules.TalkExp, ct);
        }

        public Task<CommunionResult> StartWorryAsync(CommunionRequest request, CancellationToken ct = default)
        {
            if (request == null || request.Profile == null || request.Bond == null)
                return Done(CommunionResult.Fail("대화 대상이 없습니다."));
            if (!WorryCatalog.TryGet(request.TopicId, out var topic))
                return Done(CommunionResult.Fail("알 수 없는 고민입니다."));
            if (!CanWorry(request.TopicId, request.OtherMonsterCount))
                return Done(CommunionResult.Fail("이 고민은 다른 몬스터가 있을 때만 발생합니다."));

            return BeginAsync(request, topic.Title, BondRules.WorryExp, ct);
        }

        /// <summary>선물. 추가 대화 기회(+1)는 대화가 정상 완료될 때(Complete) 지급한다.</summary>
        public Task<CommunionResult> GiveGiftAsync(CommunionRequest request, CancellationToken ct = default)
        {
            if (request == null || request.Profile == null || request.Bond == null)
                return Done(CommunionResult.Fail("선물 대상이 없습니다."));
            if (_ledger.GetGiftBonus(request.Profile.ActorId) >= BondRules.GiftBonusTalksMax)
                return Done(CommunionResult.Fail("추가 대화 기회는 최대 1회입니다."));

            var giftRequest = new CommunionRequest(
                request.Profile,
                request.Bond,
                CommunionKind.Gift,
                request.TopicId,
                request.PlayerChoice,
                request.DayIndex,
                request.OtherMonsterCount);
            return BeginAsync(giftRequest, "선물", BondRules.GiftExp, ct);
        }

        /// <summary>대화를 끝까지 봤을 때 1회 호출. EXP·기억·횟수를 한 번에 반영. 폴백이면 아무것도 반영하지 않고 false.</summary>
        public bool Complete()
        {
            if (_pending == null || _pendingResult == null || !_pendingResult.Ok)
                return false;

            if (_pendingResult.UsedFallback)
            {
                ClearPending();
                return false;
            }

            string actorId = _pending.Profile.ActorId;
            _pending.Bond.AddExp(_pendingResult.ExpGranted);
            _memory.Add(
                actorId,
                new MemoryEntry(_pendingDay, _pending.TopicId, _pending.PlayerChoice, _pendingResult.MemorySummary));

            switch (_pending.Kind)
            {
                case CommunionKind.Talk:
                {
                    int used = _ledger.GetTalksUsed(actorId, _pendingDay) + 1;
                    _ledger.SetTalksUsed(actorId, _pendingDay, used);
                    int bonus = _ledger.GetGiftBonus(actorId);
                    if (used > BondRules.TalksPerDay && bonus > 0)
                        _ledger.SetGiftBonus(actorId, bonus - 1);
                    break;
                }
                case CommunionKind.Worry:
                    _worryChance.OnResolved(actorId);
                    break;
                case CommunionKind.Gift:
                {
                    int bonus = _ledger.GetGiftBonus(actorId);
                    if (bonus < BondRules.GiftBonusTalksMax)
                        _ledger.SetGiftBonus(actorId, bonus + 1);
                    break;
                }
            }

            ClearPending();
            return true;
        }

        public void TickWorryHour(string actorId)
        {
            _worryChance.OnGameHour(actorId);
        }

        public int GetWorryChancePercent(string actorId)
        {
            return _worryChance.GetPercent(actorId);
        }

        public void ClearPending()
        {
            _pending = null;
            _pendingResult = null;
        }

        async Task<CommunionResult> BeginAsync(CommunionRequest request, string title, int exp, CancellationToken ct)
        {
            if (_busy)
                return CommunionResult.Fail("이미 대화를 준비하고 있습니다.");

            _busy = true;
            try
            {
                ClearPending();
                var memories = _memory.GetRecent(request.Profile.ActorId);
                var prompt = _prompts.Build(request, memories, title);
                string monsterName = request.Profile.Name;

                var response = await _llm.CompleteAsync(prompt, ct);
                if (ct.IsCancellationRequested)
                    return CommunionResult.Fail("대화가 취소되었습니다.");

                string failReason = null;
                ParsedLlmDialogue parsed = default;
                if (!response.Ok)
                    failReason = response.Error;
                else
                {
                    parsed = LlmDialogueParser.Parse(response.Text, monsterName);
                    if (!parsed.Ok)
                        failReason = "형식 오류(" + parsed.Error + ")";
                }

                LlmTelemetry.Record(response, failReason == null, failReason);

                CommunionResult result;
                if (failReason == null)
                {
                    result = new CommunionResult(true, string.Empty, parsed.Lines, parsed.MemorySummary, exp, false);
                }
                else
                {
                    Debug.LogWarning($"[LLM] 교감 폴백 ({monsterName}, {request.TopicId}): {failReason}");
                    result = new CommunionResult(
                        true,
                        string.Empty,
                        FallbackLines(request.TopicId, monsterName),
                        string.Empty,
                        0,
                        true,
                        failReason);
                }

                _pending = request;
                _pendingResult = result;
                _pendingDay = request.DayIndex;
                return result;
            }
            finally
            {
                _busy = false;
            }
        }

        static IReadOnlyList<DialogueLine> FallbackLines(string topicId, string monsterName)
        {
            var source = FakeLlmClient.LinesFor(topicId);
            var lines = new DialogueLine[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                bool player = source[i].Speaker == CommunionPromptBuilder.PlayerSpeaker;
                lines[i] = new DialogueLine(player ? CommunionPromptBuilder.PlayerSpeaker : monsterName, source[i].Text);
            }
            return lines;
        }

        static Task<CommunionResult> Done(CommunionResult result)
        {
            return Task.FromResult(result);
        }
    }
}
