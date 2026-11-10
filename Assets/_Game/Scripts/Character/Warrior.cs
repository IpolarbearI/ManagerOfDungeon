namespace ManagerOfDungeon
{
    public sealed class Warrior : Character
    {
        public Warrior(string name = "전사")
            : base(
                name,
                maxHealth: 470,
                attackPower: 45,
                skillAttackPower: 45,
                skill: new Skill("강타"),
                attackSpeed: 1.0f,
                moveSpeed: 8f,
                assetValue: 50,
                merchantBuyPrice: 350,
                merchantSellPrice: 250)
        {
            EquipWeapon(new Weapon("sword_01", "Sword", 80, 100, 40));
            RestoreHealth();
        }
    }
}
