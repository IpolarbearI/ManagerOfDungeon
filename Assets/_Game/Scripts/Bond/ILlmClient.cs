using System.Threading;
using System.Threading.Tasks;

namespace ManagerOfDungeon
{
    /// <summary>
    /// LLM 호출 경계. 구현: LocalLlamaClient(로컬 llama-server), FakeLlmClient(준비된 대사), RuntimeLlmClient(상황별 선택).
    /// 구현체는 예외를 던지지 않고 실패를 LlmResponse로 돌려준다. 게임 값(EXP·HP 등)은 절대 바꾸지 않는다.
    /// </summary>
    public interface ILlmClient
    {
        Task<LlmResponse> CompleteAsync(LlmPrompt prompt, CancellationToken ct);
    }

    public sealed class LlmResponse
    {
        public bool Ok { get; }
        public string Text { get; }
        public string Error { get; }
        public float ElapsedMs { get; }
        /// <summary>true면 실제 모델이 아니라 준비된 대사(Fake)가 답한 것.</summary>
        public bool Scripted { get; }

        LlmResponse(bool ok, string text, string error, float elapsedMs, bool scripted)
        {
            Ok = ok;
            Text = text ?? string.Empty;
            Error = error ?? string.Empty;
            ElapsedMs = elapsedMs;
            Scripted = scripted;
        }

        public static LlmResponse Success(string text, float elapsedMs, bool scripted = false)
        {
            return new LlmResponse(true, text, string.Empty, elapsedMs, scripted);
        }

        public static LlmResponse Failure(string error, float elapsedMs = 0f)
        {
            return new LlmResponse(false, string.Empty, error, elapsedMs, false);
        }
    }
}
