namespace ManagerOfDungeon
{
    /// <summary>
    /// 04-6 전략 해금. 전투 AI가 읽고, LLM은 건드리지 않는다.
    /// Lv.1 근거리: 원거리 보호 또는 전투 우선 (상호 배타)
    /// Lv.2 근거리: 점사
    /// Lv.3 근거리: 함정 유도 + 장비 활용 패시브 (Beta)
    /// Lv.4 근거리: 전략적 후퇴 (Beta)
    /// 원거리 Lv.1 전투/생존 우선, Lv.2 후퇴 1, Lv.3 후퇴 2+장비, Lv.4 지형전 (Beta)
    /// </summary>
    public static class StrategyUnlockTable
    {
        public static bool MeleeFocusFireUnlocked(int bondLevel)
        {
            return bondLevel >= BondRules.MeleeFocusFireLevel;
        }
    }
}
