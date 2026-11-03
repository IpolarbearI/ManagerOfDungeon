namespace ManagerOfDungeon
{
    public sealed class Warrior : Character
    {
        public Warrior(string name = "전사")
            : base(
                name,
                maxHealth: 1500,
                attackPower: 70,
                skillAttackPower: 70,
                skill: new Skill("강타"),
                attackSpeed: 1.0f,
                moveSpeed: 8f,
                assetValue: 300,
                merchantBuyPrice: 350,
                merchantSellPrice: 250)
        {
            EquipWeapon(new Weapon("sword_01", "Sword", 80, 100, 40));
            RestoreHealth();
        }
    }
}
