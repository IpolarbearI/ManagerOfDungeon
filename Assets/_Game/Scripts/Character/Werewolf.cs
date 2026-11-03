namespace ManagerOfDungeon
{
    public sealed class Werewolf : Monster
    {
        public Werewolf(string name = "Werewolf")
            : base(name, maxHealth: 580, baseAttackPower: 85, attackSpeed: 1.0f, moveSpeed: 6f, assetValue: 200)
        {
        }
    }
}
