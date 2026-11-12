using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagerOfDungeon
{
    /// <summary>
    /// llama.cpp의 llama-server(OpenAI 호환 /v1/chat/completions)에 요청한다.
    /// - 요청 제한 시간 = 서버 준비 대기 + 생성 시간 합계 (LlmSettings.requestTimeoutSeconds)
    /// - response_format json_schema로 출력 형식을 문법 단위로 강제
    /// - 예외를 밖으로 던지지 않는다(전부 LlmResponse.Failure)
    /// </summary>
    public sealed class LocalLlamaClient : ILlmClient
    {
        static readonly HttpClient Http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        readonly LlmRuntime _runtime;
        readonly LlmSettings _settings;

        public LocalLlamaClient(LlmRuntime runtime, LlmSettings settings)
        {
            _runtime = runtime;
            _settings = settings ?? new LlmSettings();
        }

        public async Task<LlmResponse> CompleteAsync(LlmPrompt prompt, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                cts.CancelAfter(TimeSpan.FromSeconds(_settings.requestTimeoutSeconds));
                try
                {
                    bool ready = await _runtime.WaitUntilReadyAsync(cts.Token).ConfigureAwait(false);
                    if (!ready)
                        return LlmResponse.Failure("LLM 서버가 준비되지 않았습니다.", Ms(sw));

                    string body = BuildBody(prompt);
                    if (_settings.logPrompts)
                        UnityEngine.Debug.Log("[LLM] 요청\n" + body);

                    using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
                    using (var resp = await Http.PostAsync(_runtime.BaseUrl + "/v1/chat/completions", content, cts.Token).ConfigureAwait(false))
                    {
                        string text = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (!resp.IsSuccessStatusCode)
                            return LlmResponse.Failure($"HTTP {(int)resp.StatusCode}: {Short(text)}", Ms(sw));

                        if (!TryExtractContent(text, out string message, out string finish))
                            return LlmResponse.Failure("응답에서 content를 찾지 못했습니다: " + Short(text), Ms(sw));

                        if (_settings.logPrompts)
                            UnityEngine.Debug.Log($"[LLM] 응답 ({Ms(sw):0}ms, finish={finish})\n{message}");

                        if (finish == "length")
                            return LlmResponse.Failure("최대 토큰에서 잘렸습니다.", Ms(sw));

                        return LlmResponse.Success(message, Ms(sw));
                    }
                }
                catch (OperationCanceledException)
                {
                    return LlmResponse.Failure(ct.IsCancellationRequested ? "취소됨" : "시간 초과", Ms(sw));
                }
                catch (Exception e)
                {
                    return LlmResponse.Failure(e.GetType().Name + ": " + e.Message, Ms(sw));
                }
            }
        }

        string BuildBody(LlmPrompt prompt)
        {
            int maxTokens = prompt.MaxTokens > 0 ? prompt.MaxTokens : _settings.maxTokens;
            float temperature = prompt.Temperature >= 0f ? prompt.Temperature : _settings.temperature;
            var inv = System.Globalization.CultureInfo.InvariantCulture;

            var sb = new StringBuilder(2048);
            sb.Append("{\"messages\":[{\"role\":\"system\",\"content\":");
            MiniJson.AppendQuoted(sb, prompt.System);
            sb.Append("},{\"role\":\"user\",\"content\":");
            MiniJson.AppendQuoted(sb, prompt.User);
            sb.Append("}]");
            sb.Append(",\"stream\":false");
            sb.Append(",\"cache_prompt\":true");
            sb.Append(",\"max_tokens\":").Append(maxTokens.ToString(inv));
            sb.Append(",\"temperature\":").Append(temperature.ToString("0.###", inv));
            sb.Append(",\"top_p\":").Append(_settings.topP.ToString("0.###", inv));
            if (!string.IsNullOrEmpty(prompt.JsonSchema))
            {
                sb.Append(",\"response_format\":{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"communion\",\"strict\":true,\"schema\":");
                sb.Append(prompt.JsonSchema);
                sb.Append("}}");
            }
            sb.Append('}');
            return sb.ToString();
        }

        static bool TryExtractContent(string responseJson, out string content, out string finishReason)
        {
            content = string.Empty;
            finishReason = string.Empty;
            if (!MiniJson.TryParse(responseJson, out object root))
                return false;

            var choices = MiniJson.AsArray(MiniJson.AsObject(root)?.GetValueOrNull("choices"));
            if (choices == null || choices.Count == 0)
                return false;

            var choice = MiniJson.AsObject(choices[0]);
            finishReason = MiniJson.GetString(choice, "finish_reason");
            var message = MiniJson.AsObject(choice?.GetValueOrNull("message"));
            content = MiniJson.GetString(message, "content");
            return !string.IsNullOrEmpty(content);
        }

        static float Ms(Stopwatch sw)
        {
            return (float)sw.Elapsed.TotalMilliseconds;
        }

        static string Short(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return s.Length > 200 ? s.Substring(0, 200) + "…" : s;
        }
    }

    static class DictionaryExtensions
    {
        public static object GetValueOrNull(this System.Collections.Generic.Dictionary<string, object> d, string key)
        {
            return d != null && d.TryGetValue(key, out var v) ? v : null;
        }
    }
}
