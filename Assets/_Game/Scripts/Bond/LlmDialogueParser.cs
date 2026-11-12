using System.Collections.Generic;
using System.Text;

namespace ManagerOfDungeon
{
    public readonly struct ParsedLlmDialogue
    {
        public ParsedLlmDialogue(string speaker, IReadOnlyList<DialogueLine> lines, string memorySummary, bool ok, string error = null)
        {
            Speaker = speaker ?? string.Empty;
            Lines = lines ?? System.Array.Empty<DialogueLine>();
            MemorySummary = memorySummary ?? string.Empty;
            Ok = ok;
            Error = error ?? string.Empty;
        }

        public string Speaker { get; }
        public IReadOnlyList<DialogueLine> Lines { get; }
        public string MemorySummary { get; }
        public bool Ok { get; }
        public string Error { get; }
    }

    /// <summary>
    /// LLM 응답 JSON 검증 (명세 6.1 JSON 검증 단계).
    /// 형식: {"lines":[{"speaker":"이름|마왕","text":"..."}], "memorySummary":"..."}
    /// 문자열 배열 lines도 허용(구형). 하나라도 규칙을 어기면 Ok=false → 폴백.
    /// </summary>
    public static class LlmDialogueParser
    {
        const int HardMaxLineChars = 90;
        const int HardMaxSummaryChars = 80;
        const int HardMaxLines = 5;

        static readonly string[] Forbidden = { "<|", "|>", "```", "{", "}", "assistant", "system" };

        public static ParsedLlmDialogue Parse(string raw, string monsterName)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Fail(monsterName, "빈 응답");

            string json = ExtractObject(raw);
            if (json == null)
                return Fail(monsterName, "JSON 객체 없음");

            if (!MiniJson.TryParse(json, out object rootValue) || !(rootValue is Dictionary<string, object> root))
                return Fail(monsterName, "JSON 파싱 실패");

            if (!root.TryGetValue("lines", out object linesValue) || !(linesValue is List<object> rawLines))
                return Fail(monsterName, "lines 없음");

            var lines = new List<DialogueLine>(rawLines.Count);
            bool monsterSpoke = false;
            for (int i = 0; i < rawLines.Count && lines.Count < HardMaxLines; i++)
            {
                string speaker;
                string text;
                if (rawLines[i] is string s)
                {
                    speaker = monsterName;
                    text = s;
                }
                else if (rawLines[i] is Dictionary<string, object> obj)
                {
                    speaker = MiniJson.GetString(obj, "speaker");
                    text = MiniJson.GetString(obj, "text");
                    if (string.IsNullOrEmpty(text))
                        text = MiniJson.GetString(obj, "line");
                }
                else
                {
                    continue;
                }

                text = Clean(text, HardMaxLineChars);
                if (text.Length == 0)
                    continue;
                if (ContainsForbidden(text))
                    return Fail(monsterName, "금지 문자열: " + text);

                bool isPlayer = speaker == CommunionPromptBuilder.PlayerSpeaker;
                if (!isPlayer)
                    monsterSpoke = true;
                lines.Add(new DialogueLine(isPlayer ? CommunionPromptBuilder.PlayerSpeaker : monsterName, text));
            }

            if (lines.Count == 0)
                return Fail(monsterName, "대사 없음");
            if (!monsterSpoke)
                return Fail(monsterName, "몬스터 대사 없음");

            string summary = Clean(MiniJson.GetString(root, "memorySummary"), HardMaxSummaryChars);
            if (ContainsForbidden(summary))
                summary = string.Empty;
            if (summary.Length == 0)
                summary = Clean(FirstMonsterLine(lines), 40);

            return new ParsedLlmDialogue(monsterName, lines, summary, true);
        }

        static ParsedLlmDialogue Fail(string monsterName, string error)
        {
            return new ParsedLlmDialogue(monsterName, System.Array.Empty<DialogueLine>(), string.Empty, false, error);
        }

        /// <summary>앞뒤 설명문·코드펜스·&lt;think&gt; 블록이 붙어도 첫 '{' ~ 마지막 '}'만 꺼낸다.</summary>
        static string ExtractObject(string raw)
        {
            int think = raw.IndexOf("</think>", System.StringComparison.Ordinal);
            if (think >= 0)
                raw = raw.Substring(think + "</think>".Length);

            int first = raw.IndexOf('{');
            int last = raw.LastIndexOf('}');
            if (first < 0 || last <= first)
                return null;
            return raw.Substring(first, last - first + 1);
        }

        static string Clean(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder(text.Length);
            bool space = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsWhiteSpace(c))
                {
                    space = sb.Length > 0;
                    continue;
                }
                if (char.IsControl(c))
                    continue;
                if (space)
                {
                    sb.Append(' ');
                    space = false;
                }
                sb.Append(c);
            }

            string result = sb.ToString().Trim('"', '\'', ' ');
            if (result.Length > maxChars)
                result = result.Substring(0, maxChars - 1) + "…";
            return result;
        }

        static bool ContainsForbidden(string text)
        {
            for (int i = 0; i < Forbidden.Length; i++)
            {
                if (text.IndexOf(Forbidden[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        static string FirstMonsterLine(List<DialogueLine> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Speaker != CommunionPromptBuilder.PlayerSpeaker)
                    return lines[i].Text;
            }
            return string.Empty;
        }
    }
}
