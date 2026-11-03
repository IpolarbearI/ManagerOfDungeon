using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public sealed class CommunionService : ICommunionSession
    {
        readonly ILlmClient _llm;
        readonly IMonsterMemoryStore _memory;
        readonly CommunionPromptBuilder _prompts;
        readonly WorryChanceTracker _worryChance = new WorryChanceTracker();
        readonly Dictionary<string, int> _talksToday = new Dictionary<string, int>();
        readonly Dictionary<string, int> _giftTalkBonus = new Dictionary<string, int>();

        CommunionRequest _pending;
        CommunionResult _pendingResult;
        int _pendingDay;

        public CommunionService()
            : this(new FakeLlmClient(), new InMemoryMonsterMemoryStore(), new CommunionPromptBuilder())
        {
        }

        public CommunionService(ILlmClient llm, IMonsterMemoryStore memory, CommunionPromptBuilder prompts)
        {
            _llm = llm ?? new FakeLlmClient();
            _memory = memory ?? new InMemoryMonsterMemoryStore();
            _prompts = prompts ?? new CommunionPromptBuilder();
        }

        public IMonsterMemoryStore Memory => _memory;

        public bool CanTalk(string actorId, int dayIndex)
        {
            if (string.IsNullOrEmpty(actorId))
                return false;

            int used = TalksUsed(actorId, dayIndex);
            int bonus = BonusTalks(actorId);
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
            int bonus = BonusTalks(actorId);
            if (bonus >= BondRules.GiftBonusTalksMax)
                return CommunionResult.Fail("추가 대화 기회는 최대 1회입니다.");

            _giftTalkBonus[actorId] = bonus + 1;
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
                string key = DayKey(actorId, _pendingDay);
                _talksToday[key] = TalksUsed(actorId, _pendingDay) + 1;
                int bonus = BonusTalks(actorId);
                if (TalksUsed(actorId, _pendingDay) > BondRules.TalksPerDay && bonus > 0)
                    _giftTalkBonus[actorId] = bonus - 1;
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

        int TalksUsed(string actorId, int dayIndex)
        {
            string key = DayKey(actorId, dayIndex);
            if (_talksToday.TryGetValue(key, out int used))
                return used;
            return 0;
        }

        int BonusTalks(string actorId)
        {
            if (_giftTalkBonus.TryGetValue(actorId, out int bonus))
                return bonus;
            return 0;
        }

        static string DayKey(string actorId, int dayIndex)
        {
            return actorId + "#" + dayIndex;
        }
    }
}
