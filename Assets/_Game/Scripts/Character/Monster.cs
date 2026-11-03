namespace ManagerOfDungeon
{
    public class Monster
    {
        readonly int _baseMaxHealth;
        readonly int _baseAttackPower;

        public string Name { get; }
        public int Health { get; private set; }
        public float AttackSpeed { get; }
        public float MoveSpeed { get; }
        public float BaseAttackRange { get; }
        public int AssetValue { get; }
        public Weapon Weapon { get; private set; }
        public Armor Armor { get; private set; }
        public bool IsRanged => Weapon != null && Weapon.IsRanged;
        public float AttackRange => IsRanged ? Weapon.AttackRange : BaseAttackRange;

        public int MaxHealth => _baseMaxHealth + (Armor != null ? Armor.HealthBonus : 0);
        public int AttackPower
        {
            get
            {
                float percent = Weapon != null ? Weapon.AttackPowerPercent : 0f;
                return (int)(_baseAttackPower * (1f + percent));
            }
        }
        public bool IsAlive => Health > 0;

        public Monster(
            string name,
            int maxHealth,
            int baseAttackPower,
            float attackSpeed,
            float moveSpeed,
            int assetValue,
            float attackRange = CombatRanges.Melee)
        {
            Name = name ?? string.Empty;
            _baseMaxHealth = maxHealth < 1 ? 1 : maxHealth;
            _baseAttackPower = baseAttackPower < 0 ? 0 : baseAttackPower;
            AttackSpeed = attackSpeed < 0f ? 0f : attackSpeed;
            MoveSpeed = moveSpeed < 0f ? 0f : moveSpeed;
            BaseAttackRange = attackRange < 0.1f ? CombatRanges.Melee : attackRange;
            AssetValue = assetValue < 0 ? 0 : assetValue;
            Health = MaxHealth;
        }

        public void RestoreHealth()
        {
            Health = MaxHealth;
        }

        public void EquipWeapon(Weapon weapon)
        {
            Weapon = weapon;
        }

        public void UnequipWeapon()
        {
            Weapon = null;
        }

        public void EquipArmor(Armor armor)
        {
            Armor = armor;
            ClampHealth();
        }

        public void UnequipArmor()
        {
            Armor = null;
            ClampHealth();
        }

        public int TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0)
                return 0;

            int dealt = amount > Health ? Health : amount;
            Health -= dealt;
            return dealt;
        }

        void ClampHealth()
        {
            if (Health > MaxHealth)
                Health = MaxHealth;
        }
    }
}
