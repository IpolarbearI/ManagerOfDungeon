using System.Collections.Generic;

namespace ManagerOfDungeon
{
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

        /// <summary>본 게임용: 기록·기억을 모두 GameState에 남긴다.</summary>
        public static CommunionService ForGame(GameState state, ILlmClient llm = null)
        {
            var store = new GameStateCommunion(state);
            return new CommunionService(llm ?? new FakeLlmClient(), store, new CommunionPromptBuilder(), store);
        }

        public IMonsterMemoryStore Memory => _memory;
        public bool HasPending => _pending != null;
        public CommunionRequest Pending => _pending;

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

        public CommunionResult StartTalk(CommunionRequest request)
        {
            if (request == null || request.Profile == null || request.Bond == null)
                return CommunionResult.Fail("대화 대상이 없습니다.");
            if (!TalkCatalog.TryGet(request.TopicId, out var topic))
                return CommunionResult.Fail("알 수 없는 대화 주제입니다.");
            if (!CanTalk(request.Profile.ActorId, request.DayIndex))
                return CommunionResult.Fail("오늘은 이미 대화했습니다.");

            return Begin(request, topic.Title, BondRules.TalkExp);
        }

        public CommunionResult StartWorry(CommunionRequest request)
        {
            if (request == null || request.Profile == null || request.Bond == null)
                return CommunionResult.Fail("대화 대상이 없습니다.");
            if (!WorryCatalog.TryGet(request.TopicId, out var topic))
                return CommunionResult.Fail("알 수 없는 고민입니다.");
            if (!CanWorry(request.TopicId, request.OtherMonsterCount))
                return CommunionResult.Fail("이 고민은 다른 몬스터가 있을 때만 발생합니다.");

            return Begin(request, topic.Title, BondRules.WorryExp);
        }

        public CommunionResult GiveGift(CommunionRequest request)
        {
            if (request == null || request.Profile == null || request.Bond == null)
                return CommunionResult.Fail("선물 대상이 없습니다.");

            string actorId = request.Profile.ActorId;
            int bonus = _ledger.GetGiftBonus(actorId);
            if (bonus >= BondRules.GiftBonusTalksMax)
                return CommunionResult.Fail("추가 대화 기회는 최대 1회입니다.");

            _ledger.SetGiftBonus(actorId, bonus + 1);
            var giftRequest = new CommunionRequest(
                request.Profile,
                request.Bond,
                CommunionKind.Gift,
                request.TopicId,
                request.PlayerChoice,
                request.DayIndex,
                request.OtherMonsterCount);
            return Begin(giftRequest, "선물", BondRules.GiftExp);
        }

        public bool Complete()
        {
            if (_pending == null || _pendingResult == null || !_pendingResult.Ok)
                return false;

            string actorId = _pending.Profile.ActorId;
            _pending.Bond.AddExp(_pendingResult.ExpGranted);
            _memory.Add(
                actorId,
                new MemoryEntry(_pendingDay, _pending.TopicId, _pending.PlayerChoice, _pendingResult.MemorySummary));

            if (_pending.Kind == CommunionKind.Talk)
            {
                int used = _ledger.GetTalksUsed(actorId, _pendingDay) + 1;
                _ledger.SetTalksUsed(actorId, _pendingDay, used);
                int bonus = _ledger.GetGiftBonus(actorId);
                if (used > BondRules.TalksPerDay && bonus > 0)
                    _ledger.SetGiftBonus(actorId, bonus - 1);
            }
            else if (_pending.Kind == CommunionKind.Worry)
            {
                _worryChance.OnResolved(actorId);
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

        CommunionResult Begin(CommunionRequest request, string title, int exp)
        {
            var memories = _memory.GetRecent(request.Profile.ActorId);
            var prompt = _prompts.Build(request, memories, title);
            // TODO(LLM): 실제 LLM을 붙이면 메인 스레드가 멈춘다. 코루틴/async로 바꾸고 실패 시 아래 대체 대사를 쓴다.
            string json = _llm.Complete(prompt).GetAwaiter().GetResult();
            var parsed = LlmDialogueParser.Parse(json, request.Profile.Name);
            bool fallback = !parsed.Ok;
            IReadOnlyList<DialogueLine> lines = parsed.Lines;
            string summary = parsed.MemorySummary;

            if (fallback)
            {
                lines = FakeLlmClient.LinesFor(request.TopicId);
                summary = FakeLlmClient.SummaryFor(request.TopicId);
            }

            var result = new CommunionResult(true, string.Empty, lines, summary, exp, fallback);
            _pending = request;
            _pendingResult = result;
            _pendingDay = request.DayIndex;
            return result;
        }
    }
}
