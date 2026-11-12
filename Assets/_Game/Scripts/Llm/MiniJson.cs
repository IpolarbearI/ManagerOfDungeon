using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 작은 JSON 파서/이스케이프 도우미. (JsonUtility는 임의 구조·배열 루트를 못 읽어서 LLM 응답용으로 따로 둔다.)
    /// 결과 타입: Dictionary&lt;string, object&gt; / List&lt;object&gt; / string / double / bool / null
    /// </summary>
    public static class MiniJson
    {
        public static bool TryParse(string json, out object value)
        {
            value = null;
            if (string.IsNullOrEmpty(json))
                return false;

            var p = new Parser(json);
            try
            {
                p.SkipWs();
                value = p.ReadValue();
                p.SkipWs();
                return p.AtEnd;
            }
            catch (System.FormatException)
            {
                value = null;
                return false;
            }
        }

        /// <summary>문자열을 JSON 문자열 리터럴(따옴표 포함)로 만든다.</summary>
        public static string Quote(string s)
        {
            var sb = new StringBuilder((s?.Length ?? 0) + 2);
            AppendQuoted(sb, s);
            return sb.ToString();
        }

        public static void AppendQuoted(StringBuilder sb, string s)
        {
            sb.Append('"');
            if (!string.IsNullOrEmpty(s))
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        default:
                            if (c < 0x20)
                                sb.Append("\\u").Append(((int)c).ToString("x4"));
                            else
                                sb.Append(c);
                            break;
                    }
                }
            }
            sb.Append('"');
        }

        public static Dictionary<string, object> AsObject(object value)
        {
            return value as Dictionary<string, object>;
        }

        public static List<object> AsArray(object value)
        {
            return value as List<object>;
        }

        public static string GetString(Dictionary<string, object> obj, string key)
        {
            if (obj == null || !obj.TryGetValue(key, out var v) || v == null)
                return string.Empty;
            return v as string ?? System.Convert.ToString(v, CultureInfo.InvariantCulture);
        }

        sealed class Parser
        {
            readonly string _s;
            int _i;

            public Parser(string s)
            {
                _s = s;
            }

            public bool AtEnd => _i >= _s.Length;

            public void SkipWs()
            {
                while (_i < _s.Length && char.IsWhiteSpace(_s[_i]))
                    _i++;
            }

            char Peek()
            {
                if (_i >= _s.Length)
                    throw new System.FormatException("unexpected end");
                return _s[_i];
            }

            public object ReadValue()
            {
                SkipWs();
                char c = Peek();
                switch (c)
                {
                    case '{': return ReadObject();
                    case '[': return ReadArray();
                    case '"': return ReadString();
                    case 't': Expect("true"); return true;
                    case 'f': Expect("false"); return false;
                    case 'n': Expect("null"); return null;
                    default:
                        if (c == '-' || (c >= '0' && c <= '9'))
                            return ReadNumber();
                        throw new System.FormatException("unexpected char " + c);
                }
            }

            void Expect(string word)
            {
                if (string.CompareOrdinal(_s, _i, word, 0, word.Length) != 0)
                    throw new System.FormatException("expected " + word);
                _i += word.Length;
            }

            Dictionary<string, object> ReadObject()
            {
                var obj = new Dictionary<string, object>();
                _i++; // {
                SkipWs();
                if (Peek() == '}')
                {
                    _i++;
                    return obj;
                }

                while (true)
                {
                    SkipWs();
                    if (Peek() != '"')
                        throw new System.FormatException("expected key");
                    string key = ReadString();
                    SkipWs();
                    if (Peek() != ':')
                        throw new System.FormatException("expected :");
                    _i++;
                    obj[key] = ReadValue();
                    SkipWs();
                    char c = Peek();
                    _i++;
                    if (c == ',')
                        continue;
                    if (c == '}')
                        return obj;
                    throw new System.FormatException("expected , or }");
                }
            }

            List<object> ReadArray()
            {
                var list = new List<object>();
                _i++; // [
                SkipWs();
                if (Peek() == ']')
                {
                    _i++;
                    return list;
                }

                while (true)
                {
                    list.Add(ReadValue());
                    SkipWs();
                    char c = Peek();
                    _i++;
                    if (c == ',')
                        continue;
                    if (c == ']')
                        return list;
                    throw new System.FormatException("expected , or ]");
                }
            }

            string ReadString()
            {
                var sb = new StringBuilder();
                _i++; // "
                while (true)
                {
                    char c = Peek();
                    _i++;
                    if (c == '"')
                        return sb.ToString();
                    if (c != '\\')
                    {
                        sb.Append(c);
                        continue;
                    }

                    char e = Peek();
                    _i++;
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length)
                                throw new System.FormatException("bad \\u");
                            sb.Append((char)int.Parse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            _i += 4;
                            break;
                        default:
                            throw new System.FormatException("bad escape");
                    }
                }
            }

            double ReadNumber()
            {
                int start = _i;
                while (_i < _s.Length && "+-0123456789.eE".IndexOf(_s[_i]) >= 0)
                    _i++;
                if (!double.TryParse(_s.Substring(start, _i - start), NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    throw new System.FormatException("bad number");
                return d;
            }
        }
    }
}
