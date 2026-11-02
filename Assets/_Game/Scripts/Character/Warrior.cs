namespace ManagerOfDungeon
{
    public sealed class Warrior : Character
    {
        public Warrior(string name = "전사")
            : base(name, maxHealth: 150, attackPower: 18, skillAttackPower: 10, skill: new Skill("강타"))
        {
            EquipWeapon(new Weapon("검", 6));
            EquipArmor(new Armor("판금 갑옷", 30));
            RestoreHealth();
        }
    }
}
