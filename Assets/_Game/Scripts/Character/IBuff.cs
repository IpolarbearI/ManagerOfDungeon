namespace ManagerOfDungeon
{
    public interface IBuff
    {
        string Name { get; }
        int RemainingTurns { get; }
        bool IsExpired { get; }
        int HealthBonus { get; }
        int AttackBonus { get; }
        int SkillAttackBonus { get; }

        void OnApply(ICharacter target);
        void OnRemove(ICharacter target);
        void Tick();
    }
}
