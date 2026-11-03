namespace ManagerOfDungeon
{
    public sealed class MemoryEntry
    {
        public int DayIndex { get; }
        public string TopicId { get; }
        public string PlayerChoice { get; }
        public string Summary { get; }

        public MemoryEntry(int dayIndex, string topicId, string playerChoice, string summary)
        {
            DayIndex = dayIndex;
            TopicId = topicId ?? string.Empty;
            PlayerChoice = playerChoice ?? string.Empty;
            Summary = summary ?? string.Empty;
        }
    }
}
