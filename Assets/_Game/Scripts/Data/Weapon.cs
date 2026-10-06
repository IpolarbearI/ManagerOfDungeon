namespace ManagerOfDungeon
{
    public sealed class Weapon
    {
        public string Name { get; }
        public int AttackBonus { get; }

        public Weapon(string name, int attackBonus)
        {
            Name = name ?? string.Empty;
            AttackBonus = attackBonus;
        }
    }
}
