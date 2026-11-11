namespace ManagerOfDungeon
{
    /// <summary>고철 검 (EQP-001): 근거리 유지, 공격력 +15%.</summary>
    public sealed class ScrapSword : Weapon
    {
        public ScrapSword()
            : base(
                GameDefs.ScrapSword,
                "고철 검",
                fixedValue: 10,
                merchantBuyPrice: 60,
                merchantSellPrice: 30,
                isRanged: false,
                attackRange: 0f,
                new EquipEffect("Attack Up", attackPowerPercent: 0.15f))
        {
        }
    }
}
