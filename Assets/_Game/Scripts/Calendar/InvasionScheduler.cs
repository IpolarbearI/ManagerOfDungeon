namespace ManagerOfDungeon
{
    /// <summary>
    /// 침입 판정 (04-1). 하루 최대 1회.
    /// 마지막 침입 다음 날 33%, 2일 후 66%, 3일 후 99%, 4일 후 100%. 오전 30% / 오후 70%.
    /// Day 7도 같은 규칙(강제 침입 없음).
    /// </summary>
    public static class InvasionScheduler
    {
        public static int ChanceForToday(GameState state)
        {
            int daysSince = state.day - state.invasion.lastInvasionDay;
            if (daysSince < 1)
                return 0;

            var table = BalanceConfig.InvasionChanceByDaysSince;
            int index = daysSince - 1;
            if (index >= table.Length)
                index = table.Length - 1;
            return table[index];
        }

        public static void RollToday(GameState state)
        {
            var inv = state.invasion;
            inv.todayTriggered = false;
            inv.todayHasInvasion = state.rng.Percent(ChanceForToday(state));
            inv.todayMinute = 0f;
            if (!inv.todayHasInvasion)
                return;

            bool morning = state.rng.Percent(BalanceConfig.MorningInvasionPercent);
            inv.todayMinute = morning
                ? state.rng.Range(BalanceConfig.MorningWindowStart, BalanceConfig.NoonMinute)
                : state.rng.Range(BalanceConfig.NoonMinute, BalanceConfig.AfternoonWindowEnd);
        }

        public static bool IsDue(GameState state)
        {
            var inv = state.invasion;
            return inv.todayHasInvasion && !inv.todayTriggered && state.clockMinutes >= inv.todayMinute;
        }
    }
}
