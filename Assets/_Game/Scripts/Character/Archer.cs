namespace ManagerOfDungeon
{
    public sealed class Archer : Character
    {
        public Archer(string name = "궁수")
            : base(
                name,
                maxHealth: 360,
                attackPower: 23,
                skillAttackPower: 23,
                skill: new Skill("민첩함"),
                attackSpeed: 1.0f,
                moveSpeed: 6.5f,
                assetValue: 40,
                merchantBuyPrice: 300,
                merchantSellPrice: 210,
                attackRange: CombatRanges.Ranged)
        {
            EquipWeapon(new Weapon("bow_01", "Bow", 70, 90, 35, true, CombatRanges.Ranged));
            EquipArmor(new Armor("가죽 갑옷", 0));
            RestoreHealth();
        }
    }
}
