namespace ManagerOfDungeon
{
    public static class BondRules
    {
        public const int MinLevel = 1;
        public const int MaxLevel = 4;
        public const int TalkExp = 5;
        public const int WorryExp = 20;
        public const int GiftExp = 10;
        public const int DeathExpPenalty = 50;
        public const int GiftBonusTalksMax = 1;
        public const int TalksPerDay = 1;
        public const int WorryChanceStepPercent = 10;
        public const int MemoryKeepCount = 8;
        public const int MeleeFocusFireLevel = 2;

        public static int ExpToNextLevel(int level)
        {
            switch (level)
            {
                case 1:
                    return 100;
                case 2:
                    return 200;
                case 3:
                    return 400;
                default:
                    return 0;
            }
        }
    }
}
