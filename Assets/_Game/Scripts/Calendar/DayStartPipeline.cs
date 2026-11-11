namespace ManagerOfDungeon
{
    /// <summary>
    /// 매일 06:00 처리 순서 (04-1):
    /// 1) 총자산·방어 전투력 스냅샷 → 2) 침입 판정 → 3) 용사 파티 생성(방어 전투력 기준)
    /// 4) (대화 횟수는 날짜 키로 자동 초기화). 자동저장은 GameDirector가 이 함수 직후에 한다.
    /// </summary>
    public static class DayStartPipeline
    {
        public static void Run(GameState state)
        {
            if (state == null || state.dayStarted)
                return;

            state.clockMinutes = BalanceConfig.DayStartMinute;
            state.assetSnapshot = AssetCalculator.Total(state);
            state.defenseSnapshot = AssetCalculator.DefensePower(state);

            InvasionScheduler.RollToday(state);
            if (state.invasion.todayHasInvasion)
                state.invasion.party = PartyGenerator.Generate(state.defenseSnapshot, state.rng);
            else
                state.invasion.party = new HeroPartyState();

            state.dayStarted = true;
            state.AddLog($"Day {state.day} 시작 · 총자산 {state.assetSnapshot} · 방어 전투력 {state.defenseSnapshot}");
        }
    }
}
