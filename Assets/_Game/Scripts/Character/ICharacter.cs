using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public interface ICharacter
    {
        string Name { get; }
        int Health { get; }
        int MaxHealth { get; }
        int AttackPower { get; }
        int SkillAttackPower { get; }
        bool IsAlive { get; }
        Weapon Weapon { get; }
        Armor Armor { get; }
        Skill Skill { get; }
        IReadOnlyList<IBuff> Buffs { get; }

        void EquipWeapon(Weapon weapon);
        void UnequipWeapon();
        void EquipArmor(Armor armor);
        void UnequipArmor();
        int BasicAttack(ICharacter target);
        int CastSkill(ICharacter target);
        int TakeDamage(int amount);
        void AddBuff(IBuff buff);
        void RemoveBuff(IBuff buff);
        void TickBuffs();
    }
}
