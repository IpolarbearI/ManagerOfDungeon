using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public class Weapon
    {
        readonly List<IEquipEffect> _effects = new List<IEquipEffect>();

        public string Id { get; }
        public string Name { get; }
        public int FixedValue { get; }
        public int MerchantBuyPrice { get; }
        public int MerchantSellPrice { get; }
        public bool IsRanged { get; }
        public float AttackRange { get; }
        public IReadOnlyList<IEquipEffect> Effects => _effects;

        public float AttackPowerPercent
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < _effects.Count; i++)
                    total += _effects[i].AttackPowerPercent;
                return total;
            }
        }

        public Weapon(
            string id,
            string name,
            int fixedValue,
            int merchantBuyPrice,
            int merchantSellPrice,
            params IEquipEffect[] effects)
            : this(id, name, fixedValue, merchantBuyPrice, merchantSellPrice, false, 0f, effects)
        {
        }

        public Weapon(
            string id,
            string name,
            int fixedValue,
            int merchantBuyPrice,
            int merchantSellPrice,
            bool isRanged,
            float attackRange,
            params IEquipEffect[] effects)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            FixedValue = fixedValue < 0 ? 0 : fixedValue;
            MerchantBuyPrice = merchantBuyPrice < 0 ? 0 : merchantBuyPrice;
            MerchantSellPrice = merchantSellPrice < 0 ? 0 : merchantSellPrice;
            IsRanged = isRanged;
            AttackRange = attackRange < 0f ? 0f : attackRange;

            if (effects == null)
                return;

            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] != null)
                    _effects.Add(effects[i]);
            }
        }
    }
}
