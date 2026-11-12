namespace ManagerOfDungeon
{
    public sealed class LlmPrompt
    {
        public string System { get; }
        public string User { get; }
        /// <summary>출력을 강제할 JSON 스키마(문자열). 비우면 자유 출력.</summary>
        public string JsonSchema { get; }
        /// <summary>0 이하면 설정값 사용.</summary>
        public int MaxTokens { get; }
        /// <summary>0 미만이면 설정값 사용.</summary>
        public float Temperature { get; }

        public LlmPrompt(string system, string user, string jsonSchema = null, int maxTokens = 0, float temperature = -1f)
        {
            System = system ?? string.Empty;
            User = user ?? string.Empty;
            JsonSchema = jsonSchema ?? string.Empty;
            MaxTokens = maxTokens;
            Temperature = temperature;
        }
    }
}
