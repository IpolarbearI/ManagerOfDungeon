namespace ManagerOfDungeon
{
    public sealed class OldWoodBow : Weapon
    {
        public OldWoodBow()
            : base(
                "old_wood_bow",
                "고목활",
                fixedValue: 50,
                merchantBuyPrice: 60,
                merchantSellPrice: 30,
                isRanged: true,
                attackRange: CombatRanges.Ranged,
                new EquipEffect("Attack Up", attackPowerPercent: 0.10f))
        {
        }
    }
}
