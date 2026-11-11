namespace ManagerOfDungeon
{
    public sealed class MonsterProfile
    {
        public string ActorId { get; }
        public string Species { get; }
        public string Name { get; }
        public string PersonalityId { get; }

        public MonsterProfile(string actorId, string species, string name, string personalityId)
        {
            ActorId = actorId ?? string.Empty;
            Species = species ?? string.Empty;
            Name = string.IsNullOrEmpty(name) ? Species : name;
            PersonalityId = personalityId ?? string.Empty;
        }
    }
}
