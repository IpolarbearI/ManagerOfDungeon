using System.Collections.Generic;
using System.Text;

namespace ManagerOfDungeon
{
    public readonly struct ParsedLlmDialogue
    {
        public ParsedLlmDialogue(string speaker, IReadOnlyList<DialogueLine> lines, string memorySummary, bool ok)
        {
            Speaker = speaker ?? string.Empty;
            Lines = lines ?? System.Array.Empty<DialogueLine>();
            MemorySummary = memorySummary ?? string.Empty;
            Ok = ok;
        }

        public string Speaker { get; }
        public IReadOnlyList<DialogueLine> Lines { get; }
        public string MemorySummary { get; }
        public bool Ok { get; }
    }

    public static class LlmDialogueParser
    {
        public static ParsedLlmDialogue Parse(string json, string fallbackSpeaker)
        {
            if (string.IsNullOrEmpty(json))
                return new ParsedLlmDialogue(fallbackSpeaker, System.Array.Empty<DialogueLine>(), string.Empty, false);

            string speaker = ReadString(json, "speaker");
            if (string.IsNullOrEmpty(speaker))
                speaker = fallbackSpeaker;

            string summary = ReadString(json, "memorySummary");
            var lines = ReadLines(json, speaker);
            if (lines.Count == 0)
                return new ParsedLlmDialogue(speaker, System.Array.Empty<DialogueLine>(), summary, false);

            return new ParsedLlmDialogue(speaker, lines, summary, true);
        }

        static string ReadString(string json, string key)
        {
            string token = "\"" + key + "\"";
            int keyIndex = json.IndexOf(token, System.StringComparison.Ordinal);
            if (keyIndex < 0)
                return string.Empty;

            int colon = json.IndexOf(':', keyIndex + token.Length);
            if (colon < 0)
                return string.Empty;

            int first = json.IndexOf('"', colon + 1);
            if (first < 0)
                return string.Empty;

            return Unescape(ReadQuoted(json, first));
        }

        static List<DialogueLine> ReadLines(string json, string fallbackSpeaker)
        {
            var result = new List<DialogueLine>();
            const string token = "\"lines\"";
            int keyIndex = json.IndexOf(token, System.StringComparison.Ordinal);
            if (keyIndex < 0)
                return result;

            int bracket = json.IndexOf('[', keyIndex + token.Length);
            if (bracket < 0)
                return result;

            int depth = 0;
            int end = -1;
            for (int i = bracket; i < json.Length; i++)
            {
                if (json[i] == '[')
                    depth++;
                else if (json[i] == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        end = i;
                        break;
                    }
                }
            }

            if (end < 0)
                return result;

            int cursor = bracket + 1;
            while (cursor < end)
            {
                while (cursor < end && (json[cursor] == ',' || json[cursor] == ' ' || json[cursor] == '\n'))
                    cursor++;
                if (cursor >= end)
                    break;

                if (json[cursor] == '{')
                {
                    int close = json.IndexOf('}', cursor);
                    if (close < 0 || close > end)
                        break;
                    string obj = json.Substring(cursor, close - cursor + 1);
                    string lineSpeaker = ReadString(obj, "speaker");
                    string text = ReadString(obj, "text");
                    if (string.IsNullOrEmpty(text))
                        text = ReadString(obj, "line");
                    if (!string.IsNullOrEmpty(text))
                        result.Add(new DialogueLine(string.IsNullOrEmpty(lineSpeaker) ? fallbackSpeaker : lineSpeaker, text));
                    cursor = close + 1;
                    continue;
                }

                if (json[cursor] == '"')
                {
                    string text = Unescape(ReadQuoted(json, cursor));
                    if (!string.IsNullOrEmpty(text))
                        result.Add(new DialogueLine(fallbackSpeaker, text));
                    cursor += text.Length + 2;
                    continue;
                }

                cursor++;
            }

            return result;
        }

        static string ReadQuoted(string json, int firstQuote)
        {
            var sb = new StringBuilder();
            for (int i = firstQuote + 1; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '\\' && i + 1 < json.Length)
                {
                    sb.Append(json[i + 1]);
                    i++;
                    continue;
                }

                if (c == '"')
                    break;
                sb.Append(c);
            }

            return sb.ToString();
        }

        static string Unescape(string value)
        {
            return value.Replace("\\n", "\n");
        }
    }
}
