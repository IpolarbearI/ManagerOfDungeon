using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public interface IMonsterMemoryStore
    {
        IReadOnlyList<MemoryEntry> GetRecent(string actorId);
        void Add(string actorId, MemoryEntry entry);
        void Clear(string actorId);
    }
}
