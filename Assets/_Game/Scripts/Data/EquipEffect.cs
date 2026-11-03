namespace ManagerOfDungeon
{
    public sealed class EquipEffect : IEquipEffect
    {
        public string Name { get; }
        public float AttackPowerPercent { get; }

        public EquipEffect(string name, float attackPowerPercent = 0f)
        {
            Name = name ?? string.Empty;
            AttackPowerPercent = attackPowerPercent;
        }
    }
}
