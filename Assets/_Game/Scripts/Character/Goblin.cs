namespace ManagerOfDungeon
{
    public sealed class Goblin : Monster
    {
        public Goblin(string name = "Goblin")
            : base(name, maxHealth: 420, baseAttackPower: 45, attackSpeed: 1.2f, moveSpeed: 10f, assetValue: 50)
        {
        }
    }
}
