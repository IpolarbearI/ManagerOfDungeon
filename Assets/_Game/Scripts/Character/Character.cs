using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public class Character : ICharacter
    {
        readonly int _baseMaxHealth;
        readonly int _baseAttackPower;
        readonly int _baseSkillAttackPower;
        readonly List<IBuff> _buffs = new List<IBuff>();

        public string Name { get; }
        public int Health { get; private set; }
        public Weapon Weapon { get; private set; }
        public Armor Armor { get; private set; }
        public Skill Skill { get; }
        public IReadOnlyList<IBuff> Buffs => _buffs;

        public int MaxHealth => _baseMaxHealth + ArmorBonusHealth() + BuffBonus(buff => buff.HealthBonus);
        public int AttackPower => _baseAttackPower + (Weapon != null ? Weapon.AttackBonus : 0) + BuffBonus(buff => buff.AttackBonus);
        public int SkillAttackPower => _baseSkillAttackPower + BuffBonus(buff => buff.SkillAttackBonus);
        public bool IsAlive => Health > 0;

        public Character(string name, int maxHealth, int attackPower, int skillAttackPower, Skill skill)
        {
            Name = name ?? string.Empty;
            _baseMaxHealth = maxHealth < 1 ? 1 : maxHealth;
            _baseAttackPower = attackPower < 0 ? 0 : attackPower;
            _baseSkillAttackPower = skillAttackPower < 0 ? 0 : skillAttackPower;
            Skill = skill;
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

        public int BasicAttack(ICharacter target)
        {
            if (target == null || !IsAlive || !target.IsAlive)
                return 0;

            return target.TakeDamage(AttackPower);
        }

        public int CastSkill(ICharacter target)
        {
            if (Skill == null)
                return 0;

            return Skill.Cast(this, target);
        }

        public int TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0)
                return 0;

            int dealt = amount > Health ? Health : amount;
            Health -= dealt;
            return dealt;
        }

        public void RestoreHealth()
        {
            Health = MaxHealth;
        }

        public void AddBuff(IBuff buff)
        {
            if (buff == null || buff.IsExpired)
                return;

            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                if (_buffs[i].Name == buff.Name)
                    RemoveBuff(_buffs[i]);
            }

            _buffs.Add(buff);
            buff.OnApply(this);
            ClampHealth();
        }

        public void RemoveBuff(IBuff buff)
        {
            if (buff == null || !_buffs.Remove(buff))
                return;

            buff.OnRemove(this);
            ClampHealth();
        }

        public void TickBuffs()
        {
            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                var buff = _buffs[i];
                buff.Tick();
                if (buff.IsExpired)
                    RemoveBuff(buff);
            }
        }

        int ArmorBonusHealth() => Armor != null ? Armor.HealthBonus : 0;

        int BuffBonus(System.Func<IBuff, int> selector)
        {
            int total = 0;
            for (int i = 0; i < _buffs.Count; i++)
                total += selector(_buffs[i]);
            return total;
        }

        void ClampHealth()
        {
            if (Health > MaxHealth)
                Health = MaxHealth;
        }
    }
}
