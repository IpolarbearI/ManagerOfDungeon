using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagerOfDungeon
{
    public sealed class FakeLlmClient : ILlmClient
    {
        public Task<string> Complete(LlmPrompt prompt)
        {
            string topicId = ReadTopicId(prompt != null ? prompt.User : string.Empty);
            string json = BuildJson(topicId);
            return Task.FromResult(json);
        }

        static string ReadTopicId(string user)
        {
            const string marker = "주제 ID: ";
            int i = user.IndexOf(marker, System.StringComparison.Ordinal);
            if (i < 0)
                return string.Empty;

            int start = i + marker.Length;
            int end = user.IndexOf('\n', start);
            if (end < 0)
                end = user.Length;
            return user.Substring(start, end - start).Trim();
        }

        static string BuildJson(string topicId)
        {
            IReadOnlyList<DialogueLine> lines = LinesFor(topicId);
            string summary = SummaryFor(topicId);
            var sb = new System.Text.StringBuilder();
            sb.Append("{\"speaker\":\"").Append(Escape(lines[0].Speaker)).Append("\",\"lines\":[");
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append("{\"speaker\":\"").Append(Escape(lines[i].Speaker))
                    .Append("\",\"text\":\"").Append(Escape(lines[i].Text)).Append("\"}");
            }

            sb.Append("],\"memorySummary\":\"").Append(Escape(summary)).Append("\"}");
            return sb.ToString();
        }

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

        static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
