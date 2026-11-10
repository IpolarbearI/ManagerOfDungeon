namespace ManagerOfDungeon
{
    /// <summary>
    /// 밸런스 초기 기준값(기획서 v1.1). QA 후 조정할 값은 모두 여기에 모은다.
    /// </summary>
    public static class BalanceConfig
    {
        // 기간·실패 (06-1)
        public const int TargetDays = 7;
        public const int StartCoins = 1;
        public const int StartSol = 20;

        // 시간 (04-1): 06:00~22:00 = 실제 약 5분
        public const float DayStartMinute = 6 * 60f;
        public const float DayEndMinute = 22 * 60f;
        public const float NoonMinute = 12 * 60f;
        public const float RealSecondsPerDay = 300f;
        public static float GameMinutesPerRealSecond => (DayEndMinute - DayStartMinute) / RealSecondsPerDay;

        // 침입 (04-1)
        public static readonly int[] InvasionChanceByDaysSince = { 33, 66, 99, 100 };
        public const int MorningInvasionPercent = 30;
        public const float MorningWindowStart = 6 * 60f + 30f;
        public const float AfternoonWindowEnd = 21 * 60f;
        // [결정 필요] 게임 시작을 '마지막 침입 = Day 0'으로 본다 → Day 1 침입 확률 33%
        public const int InitialLastInvasionDay = 0;

        // 자산 (11-10 개정)
        // 총자산가치 = 코어 + 방 + 방어 전투력 + 보유 솔 (+ 미장착 장비)
        // 방어 전투력 가치 = 몬스터 + 장착 장비 + 설치 함정 → 침입 강도는 이 값으로 정한다.
        // 시작: 총자산 350 / 방어 전투력 130 (고블린 50×2 + 고철 검 10 + 고목 활 20)
        // 유대감 레벨당 자산 +50은 폐지.
        public const int CoreValue = 100;
        public const int RoomValue = 100;
        // [결정 필요] 미장착 인벤토리 장비를 총자산에 포함할지 (방어 전투력에는 넣지 않음)
        public const bool InventoryCountsAsAsset = true;

        // 유대감 레벨 보너스 (11-10 개정: 모든 레벨 상승 시 HP·공격력 보너스)
        // [결정 필요] 수치가 아직 없어 레벨당 +5%로 임시 설정
        // (전투 테스트 씬에서 실시간으로 바꿀 수 있도록 static 필드)
        public static float BondHpBonusPerLevel = 0.05f;
        public static float BondAtkBonusPerLevel = 0.05f;

        // 용사 파티 (04-9 + 11-10 개정)
        // AI도 HP·공격력·공속과 같은 강화 후보(4종). 낮은 단계부터 무작위, AI 후순위 규칙 폐지.
        // [결정 필요] 용사 기본가치·강화 1단계 가치·허용 오차는 새 스탯 기준값이 없어 임시값.
        public const int PartyValueTolerance = 50;
        public const int HeroStatStepValue = 15;
        public const int HeroUpgradeStatCount = 4;
        public const int MaxPartySize = 4;
        public const int MaxHeroStatLevel = 4;
        // A2에서 실제로 구현된 용사 AI: 0(기본공격) · 1(가까운 적) · 2(처치 가능 → 낮은 HP)
        public const int MaxHeroAiLevelAlpha2 = 2;

        // 함정 (11-10 개정): 소모형 함정 재설치 = 최초 구매가의 10%
        public const float TrapReinstallRate = 0.10f;

        // 보상 (04-3)
        public const float RewardConsumedTrapRate = 0.10f;
        public const float RewardDeployedRate = 0.15f;

        // 교감 (04-5)
        public const string AlphaWorryTopic = "WORRY_05";
        public const int MaxLogLines = 40;
    }
}
