namespace ManagerOfDungeon
{
    public sealed class Buff : IBuff
    {
        public string Name { get; }
        public int RemainingTurns { get; private set; }
        public int HealthBonus { get; }
        public int AttackBonus { get; }
        public int SkillAttackBonus { get; }
        public bool IsExpired => RemainingTurns == 0;

        public Buff(
            string name,
            int turns,
            int healthBonus = 0,
            int attackBonus = 0,
            int skillAttackBonus = 0)
        {
            Name = name ?? string.Empty;
            RemainingTurns = turns < 0 ? -1 : turns;
            HealthBonus = healthBonus;
            AttackBonus = attackBonus;
            SkillAttackBonus = skillAttackBonus;
        }

        public void OnApply(ICharacter target) { }

        public void OnRemove(ICharacter target) { }

        public void Tick()
        {
            if (RemainingTurns > 0)
                RemainingTurns--;
        }
    }
}
