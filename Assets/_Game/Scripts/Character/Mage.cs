namespace ManagerOfDungeon
{
    public sealed class Mage : Character
    {
        public Mage(string name = "마법사")
            : base(name, maxHealth: 315, attackPower: 27, skillAttackPower: 27, skill: new Skill("파이어볼"), attackSpeed: 0.7f, moveSpeed: 5f, assetValue: 45)
        {
            EquipWeapon(new Weapon("staff_01", "Staff", 50, 70, 25));
            EquipArmor(new Armor("로브", 8));
            RestoreHealth();
        }
    }
}
