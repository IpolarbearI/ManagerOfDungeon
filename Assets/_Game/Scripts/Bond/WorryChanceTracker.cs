using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public sealed class WorryChanceTracker
    {
        readonly Dictionary<string, int> _chance = new Dictionary<string, int>();

        public int GetPercent(string actorId)
        {
            if (string.IsNullOrEmpty(actorId) || !_chance.TryGetValue(actorId, out int value))
                return 0;
            return value;
        }

        public void OnResolved(string actorId)
        {
            if (!string.IsNullOrEmpty(actorId))
                _chance[actorId] = 0;
        }

        public void OnGameHour(string actorId)
        {
            if (string.IsNullOrEmpty(actorId))
                return;

            int value = GetPercent(actorId) + BondRules.WorryChanceStepPercent;
            if (value > 100)
                value = 100;
            _chance[actorId] = value;
        }
    }
}
