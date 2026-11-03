namespace ManagerOfDungeon
{
    public sealed class Orc : Monster
    {
        public Orc(string name = "Orc")
            : base(name, maxHealth: 750, baseAttackPower: 65, attackSpeed: 0.65f, moveSpeed: 4f, assetValue: 100)
        {
        }
    }
}
