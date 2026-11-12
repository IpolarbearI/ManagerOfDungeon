using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 준비된 대사 클라이언트. 쓰는 곳:
    /// - AI 대화 꺼짐/LLM 서버 사용 불가 상태(정상 교감으로 처리, EXP 지급)
    /// - 실제 LLM 요청 실패 시 표시할 폴백 대사(이때는 CommunionService가 EXP를 주지 않음)
    /// - 테스트(SimulatedDelayMs로 지연 흉내)
    /// </summary>
    public sealed class FakeLlmClient : ILlmClient
    {
        public int SimulatedDelayMs { get; set; }

        public async Task<LlmResponse> CompleteAsync(LlmPrompt prompt, CancellationToken ct)
        {
            if (SimulatedDelayMs > 0)
            {
                try
                {
                    await Task.Delay(SimulatedDelayMs, ct);
                }
                catch (TaskCanceledException)
                {
                    return LlmResponse.Failure("취소됨");
                }
            }

            string topicId = ReadTopicId(prompt != null ? prompt.User : string.Empty);
            return LlmResponse.Success(BuildJson(topicId), SimulatedDelayMs, scripted: true);
        }

        static string ReadTopicId(string user)
        {
            int i = user.IndexOf(CommunionPromptBuilder.TopicIdMarker, System.StringComparison.Ordinal);
            if (i < 0)
                return string.Empty;

            int start = i + CommunionPromptBuilder.TopicIdMarker.Length;
            int end = user.IndexOf('\n', start);
            if (end < 0)
                end = user.Length;
            return user.Substring(start, end - start).Trim();
        }

        static string BuildJson(string topicId)
        {
            IReadOnlyList<DialogueLine> lines = LinesFor(topicId);
            var sb = new StringBuilder();
            sb.Append("{\"lines\":[");
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append("{\"speaker\":");
                MiniJson.AppendQuoted(sb, lines[i].Speaker);
                sb.Append(",\"text\":");
                MiniJson.AppendQuoted(sb, lines[i].Text);
                sb.Append('}');
            }

            sb.Append("],\"memorySummary\":");
            MiniJson.AppendQuoted(sb, SummaryFor(topicId));
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>주제별 준비 대사. 화자 '고블린'은 파서/서비스에서 실제 몬스터 이름으로 바뀐다.</summary>
        public static IReadOnlyList<DialogueLine> LinesFor(string topicId)
        {
            switch (topicId)
            {
                case TalkCatalog.Talk01:
                    return new[]
                    {
                        new DialogueLine("고블린", "오늘 진짜 재밌게 놀았어요."),
                        new DialogueLine("고블린", "마왕님도 같이 있었으면 더 좋았을 텐데.")
                    };
                case TalkCatalog.Talk02:
                    return new[]
                    {
                        new DialogueLine("고블린", "용사는 왜 자꾸 쳐들어와요?"),
                        new DialogueLine("고블린", "우리가 먼저 뭘 한 건 아닌데…")
                    };
                case TalkCatalog.Talk03:
                    return new[]
                    {
                        new DialogueLine("고블린", "마계는 어떤 곳이에요?"),
                        new DialogueLine("고블린", "거기도 이런 던전이 있나요?")
                    };
                case TalkCatalog.Talk04:
                    return new[]
                    {
                        new DialogueLine("고블린", "싸울 때 이렇게 움직이는 게 더 마음이 편해요."),
                        new DialogueLine("고블린", "마왕님이 알려준 대로요.")
                    };
                case TalkCatalog.Talk05:
                    return new[]
                    {
                        new DialogueLine("고블린", "여기에 와서 너무 기뻐요."),
                        new DialogueLine("고블린", "혼자 있는 것보다 나아요.")
                    };
                case WorryCatalog.Worry01:
                    return new[]
                    {
                        new DialogueLine("고블린", "선물을 받고 싶어요."),
                        new DialogueLine("고블린", "작은 거라도 좋아요.")
                    };
                case WorryCatalog.Worry02:
                    return new[]
                    {
                        new DialogueLine("고블린", "다른 녀석이랑 친해지고 싶어요."),
                        new DialogueLine("고블린", "어떻게 말을 걸면 되죠?")
                    };
                case WorryCatalog.Worry03:
                    return new[]
                    {
                        new DialogueLine("고블린", "다른 몬스터랑 다퉜어요."),
                        new DialogueLine("고블린", "아직 마음이 안 풀려요.")
                    };
                case WorryCatalog.Worry04:
                    return new[]
                    {
                        new DialogueLine("고블린", "전투 중에 맞는 건 아파요."),
                        new DialogueLine("고블린", "그래도 던전은 지킬게요.")
                    };
                case WorryCatalog.Worry05:
                default:
                    return new[]
                    {
                        new DialogueLine("고블린", "이유 없이 화가 나요."),
                        new DialogueLine("마왕", "갑자기? 무슨 일 있었어?"),
                        new DialogueLine("고블린", "몰라요. 그냥 답답하고 짜증이 나요."),
                        new DialogueLine("마왕", "그래도 괜찮아. 같이 있어 줄게.")
                    };
            }
        }

        public static string SummaryFor(string topicId)
        {
            if (TalkCatalog.TryGet(topicId, out var talk))
                return talk.Title + "에 대해 이야기했다.";
            if (WorryCatalog.TryGet(topicId, out var worry))
                return "고민을 말했다: " + worry.Title;
            return "마왕과 짧게 이야기했다.";
        }
    }
}
