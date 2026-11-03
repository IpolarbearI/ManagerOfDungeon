namespace ManagerOfDungeon
{
    public sealed class Mage : Character
    {
        public Mage(string name = "마법사")
            : base(name, maxHealth: 80, attackPower: 6, skillAttackPower: 28, skill: new Skill("파이어볼"))
        {
            EquipWeapon(new Weapon("staff_01", "Staff", 50, 70, 25));
            EquipArmor(new Armor("로브", 8));
            RestoreHealth();
        }
    }
}
