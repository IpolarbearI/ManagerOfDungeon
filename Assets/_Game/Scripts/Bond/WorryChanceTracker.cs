namespace ManagerOfDungeon
{
    /// <summary>
    /// 고민 발생 확률 (04-5): 해결 시 0%, 이후 1게임시간마다 +10%p.
    /// 값은 ICommunionLedger에 기록되어 본 게임에서는 저장된다.
    /// </summary>
    public sealed class WorryChanceTracker
    {
        readonly ICommunionLedger _ledger;

        public WorryChanceTracker()
            : this(null)
        {
        }

        public WorryChanceTracker(ICommunionLedger ledger)
        {
            _ledger = ledger ?? new InMemoryCommunionLedger();
        }

        public int GetPercent(string actorId)
        {
            if (string.IsNullOrEmpty(actorId))
                return 0;
            return _ledger.GetWorryChance(actorId);
        }

        public void OnResolved(string actorId)
        {
            if (!string.IsNullOrEmpty(actorId))
                _ledger.SetWorryChance(actorId, 0);
        }

        public void OnGameHour(string actorId)
        {
            if (string.IsNullOrEmpty(actorId))
                return;

            int value = GetPercent(actorId) + BondRules.WorryChanceStepPercent;
            if (value > 100)
                value = 100;
            _ledger.SetWorryChance(actorId, value);
        }
    }
}
