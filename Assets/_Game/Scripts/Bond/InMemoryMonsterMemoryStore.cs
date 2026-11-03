using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public sealed class InMemoryMonsterMemoryStore : IMonsterMemoryStore
    {
        readonly Dictionary<string, List<MemoryEntry>> _byActor = new Dictionary<string, List<MemoryEntry>>();
        readonly int _keep;

        public InMemoryMonsterMemoryStore(int keep = BondRules.MemoryKeepCount)
        {
            _keep = keep < 1 ? BondRules.MemoryKeepCount : keep;
        }

        public IReadOnlyList<MemoryEntry> GetRecent(string actorId)
        {
            if (string.IsNullOrEmpty(actorId) || !_byActor.TryGetValue(actorId, out var list))
                return System.Array.Empty<MemoryEntry>();
            return list;
        }

        public void Add(string actorId, MemoryEntry entry)
        {
            if (string.IsNullOrEmpty(actorId) || entry == null)
                return;

            if (!_byActor.TryGetValue(actorId, out var list))
            {
                list = new List<MemoryEntry>();
                _byActor[actorId] = list;
            }

            list.Add(entry);
            while (list.Count > _keep)
                list.RemoveAt(0);
        }

        public void Clear(string actorId)
        {
            if (!string.IsNullOrEmpty(actorId))
                _byActor.Remove(actorId);
        }
    }
}
