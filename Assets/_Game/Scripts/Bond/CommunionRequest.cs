namespace ManagerOfDungeon
{
    public enum CommunionKind
    {
        Talk,
        Worry,
        Gift
    }

    public sealed class CommunionRequest
    {
        public MonsterProfile Profile { get; }
        public BondProgress Bond { get; }
        public CommunionKind Kind { get; }
        public string TopicId { get; }
        public string PlayerChoice { get; }
        public int DayIndex { get; }
        public int OtherMonsterCount { get; }

        public CommunionRequest(
            MonsterProfile profile,
            BondProgress bond,
            CommunionKind kind,
            string topicId,
            string playerChoice,
            int dayIndex,
            int otherMonsterCount)
        {
            Profile = profile;
            Bond = bond;
            Kind = kind;
            TopicId = topicId ?? string.Empty;
            PlayerChoice = playerChoice ?? string.Empty;
            DayIndex = dayIndex;
            OtherMonsterCount = otherMonsterCount;
        }
    }
}
