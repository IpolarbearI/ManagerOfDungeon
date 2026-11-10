namespace ManagerOfDungeon
{
    /// <summary>
    /// 유대감 규칙 (2026-11-10 개정).
    /// - 최대 레벨 제한 없음. 필요 EXP 70 / 80 / 100 / 160 / 210 / 280 / 370 / 480 / 610 …
    /// - 레벨이 오를 때마다 HP·공격력 보너스 (BalanceConfig.BondHpBonusPerLevel / BondAtkBonusPerLevel)
    /// - 전투 사망 −10 (레벨 하락 없음, 현재 레벨 EXP 최소 0)
    /// - 방어 성공 시 전투 참여 몬스터 각각 +30
    /// - 교감하지 않은 날 감소 없음
    /// </summary>
    public static class BondRules
    {
        public const int MinLevel = 1;
        /// <summary>사실상 무제한(안전 상한).</summary>
        public const int MaxLevel = 999;
        public const int TalkExp = 5;
        public const int WorryExp = 20;
        public const int GiftExp = 10;
        public const int DeathExpPenalty = 10;
        public const int DefenseWinExp = 30;
        public const int GiftBonusTalksMax = 1;
        public const int TalksPerDay = 1;
        public const int WorryChanceStepPercent = 10;
        public const int MemoryKeepCount = 8;
        public const int MeleeFocusFireLevel = 2;

        /// <summary>Lv.1→2부터 Lv.10→11까지 확정값.</summary>
        static readonly int[] ExpTable = { 70, 80, 100, 160, 210, 280, 370, 480, 610 };

        /// <summary>
        /// level → level+1 에 필요한 EXP.
        /// [결정 필요] 610 이후는 기획서에 '…'만 있어, 증가폭이 20씩 커지는 기존 흐름으로 이어 붙였다
        /// (610 → 760 → 930 → 1120 …).
        /// </summary>
        public static int ExpToNextLevel(int level)
        {
            if (level < MinLevel || level >= MaxLevel)
                return 0;

            int index = level - MinLevel;
            if (index < ExpTable.Length)
                return ExpTable[index];

            int value = ExpTable[ExpTable.Length - 1];
            int step = ExpTable[ExpTable.Length - 1] - ExpTable[ExpTable.Length - 2];
            for (int i = ExpTable.Length; i <= index; i++)
            {
                step += 20;
                value += step;
            }

            return value;
        }
    }
}
