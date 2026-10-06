namespace ManagerOfDungeon
{
    public sealed class Skill
    {
        public string Name { get; }

        public Skill(string name)
        {
            Name = name ?? string.Empty;
        }

        public int Cast(ICharacter caster, ICharacter target)
        {
            if (caster == null || target == null || !caster.IsAlive || !target.IsAlive)
                return 0;

            return target.TakeDamage(caster.SkillAttackPower);
        }
    }
}
