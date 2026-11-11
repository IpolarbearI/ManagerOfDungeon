namespace ManagerOfDungeon
{
    public sealed class LlmPrompt
    {
        public string System { get; }
        public string User { get; }

        public LlmPrompt(string system, string user)
        {
            System = system ?? string.Empty;
            User = user ?? string.Empty;
        }
    }
}
