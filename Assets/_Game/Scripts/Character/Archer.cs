namespace ManagerOfDungeon
{
    public sealed class Archer : Character
    {
        public Archer(string name = "궁수")
            : base(name, maxHealth: 100, attackPower: 14, skillAttackPower: 16, skill: new Skill("조준 사격"))
        {
            EquipWeapon(new Weapon("활", 5));
            EquipArmor(new Armor("가죽 갑옷", 12));
            RestoreHealth();
        }
    }
}
