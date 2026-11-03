namespace ManagerOfDungeon
{
    public sealed class BondProgress
    {
        public int Level { get; private set; } = BondRules.MinLevel;
        public int Exp { get; private set; }

        public bool CanUnlockMeleeFocusFire => Level >= BondRules.MeleeFocusFireLevel;

        public int AddExp(int amount)
        {
            if (amount <= 0)
                return 0;

            int gained = amount;
            Exp += amount;
            while (Level < BondRules.MaxLevel)
            {
                int need = BondRules.ExpToNextLevel(Level);
                if (need <= 0 || Exp < need)
                    break;

                Exp -= need;
                Level++;
            }

            if (Level >= BondRules.MaxLevel)
                Exp = 0;

            return gained;
        }

        public int ApplyDeathPenalty()
        {
            int before = Exp;
            Exp -= BondRules.DeathExpPenalty;
            if (Exp < 0)
                Exp = 0;
            return before - Exp;
        }
    }
}
