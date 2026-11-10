using System.Collections.Generic;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 교감 횟수·선물 보너스·고민 확률을 어디에 기록할지 추상화.
    /// 시연은 메모리(InMemoryCommunionLedger), 본 게임은 GameState(GameStateCommunion)에 기록해 저장된다.
    /// </summary>
    public interface ICommunionLedger
    {
        int GetTalksUsed(string actorId, int dayIndex);
        void SetTalksUsed(string actorId, int dayIndex, int value);
        int GetGiftBonus(string actorId);
        void SetGiftBonus(string actorId, int value);
        int GetWorryChance(string actorId);
        void SetWorryChance(string actorId, int value);
    }

    public sealed class InMemoryCommunionLedger : ICommunionLedger
    {
        readonly Dictionary<string, int> _talks = new Dictionary<string, int>();
        readonly Dictionary<string, int> _gift = new Dictionary<string, int>();
        readonly Dictionary<string, int> _worry = new Dictionary<string, int>();

        public int GetTalksUsed(string actorId, int dayIndex)
        {
            return _talks.TryGetValue(actorId + "#" + dayIndex, out int v) ? v : 0;
        }

        public void SetTalksUsed(string actorId, int dayIndex, int value)
        {
            _talks[actorId + "#" + dayIndex] = value;
        }

        public int GetGiftBonus(string actorId)
        {
            return _gift.TryGetValue(actorId, out int v) ? v : 0;
        }

        public void SetGiftBonus(string actorId, int value)
        {
            _gift[actorId] = value;
        }

        public int GetWorryChance(string actorId)
        {
            return _worry.TryGetValue(actorId, out int v) ? v : 0;
        }

        public void SetWorryChance(string actorId, int value)
        {
            _worry[actorId] = value;
        }
    }

    /// <summary>
    /// GameState(MonsterState)를 직접 읽고 쓰는 교감 장부 + 기억 저장소. 저장/불러오기에 그대로 포함된다.
    /// </summary>
    public sealed class GameStateCommunion : ICommunionLedger, IMonsterMemoryStore
    {
        readonly GameState _state;

        public GameStateCommunion(GameState state)
        {
            _state = state;
        }

        MonsterState M(string actorId)
        {
            return _state != null ? _state.FindMonster(actorId) : null;
        }

        public int GetTalksUsed(string actorId, int dayIndex)
        {
            var m = M(actorId);
            return m != null && m.talkDay == dayIndex ? m.talkCount : 0;
        }

        public void SetTalksUsed(string actorId, int dayIndex, int value)
        {
            var m = M(actorId);
            if (m == null)
                return;
            m.talkDay = dayIndex;
            m.talkCount = value;
        }

        public int GetGiftBonus(string actorId)
        {
            var m = M(actorId);
            return m != null ? m.giftBonus : 0;
        }

        public void SetGiftBonus(string actorId, int value)
        {
            var m = M(actorId);
            if (m != null)
                m.giftBonus = value;
        }

        public int GetWorryChance(string actorId)
        {
            var m = M(actorId);
            return m != null ? m.worryChance : 0;
        }

        public void SetWorryChance(string actorId, int value)
        {
            var m = M(actorId);
            if (m != null)
                m.worryChance = value;
        }

        public IReadOnlyList<MemoryEntry> GetRecent(string actorId)
        {
            var m = M(actorId);
            if (m == null || m.memories == null)
                return System.Array.Empty<MemoryEntry>();

            var list = new List<MemoryEntry>(m.memories.Count);
            for (int i = 0; i < m.memories.Count; i++)
            {
                var s = m.memories[i];
                list.Add(new MemoryEntry(s.day, s.topicId, s.playerChoice, s.summary));
            }

            return list;
        }

        public void Add(string actorId, MemoryEntry entry)
        {
            var m = M(actorId);
            if (m == null || entry == null)
                return;

            var s = new MemoryState();
            s.day = entry.DayIndex;
            s.topicId = entry.TopicId;
            s.playerChoice = entry.PlayerChoice;
            s.summary = entry.Summary;
            m.memories.Add(s);
            while (m.memories.Count > BondRules.MemoryKeepCount)
                m.memories.RemoveAt(0);
        }

        public void Clear(string actorId)
        {
            var m = M(actorId);
            if (m != null)
                m.memories.Clear();
        }
    }
}
