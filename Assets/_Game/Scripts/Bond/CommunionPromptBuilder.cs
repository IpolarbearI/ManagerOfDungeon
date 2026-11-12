using System.Collections.Generic;
using System.Text;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 교감 프롬프트 (명세 6.1: 종족·성격·유대감 레벨·주제·최근 기억 최대 8개).
    /// 시스템 프롬프트 앞부분(StaticSystemPrompt)은 항상 같게 유지해야 llama-server 프롬프트 캐시가 먹는다.
    /// 몬스터별 정보는 그 뒤에 붙인다.
    /// </summary>
    public sealed class CommunionPromptBuilder
    {
        public const string TopicIdMarker = "주제 ID: ";
        public const int MaxLines = 4;
        public const int MaxLineChars = 60;
        public const int MaxSummaryChars = 50;
        public const string PlayerSpeaker = "마왕";

        public static readonly string StaticSystemPrompt =
            "너는 던전 운영 게임 'Manager of Dungeon'에 나오는 몬스터 한 마리다.\n" +
            "플레이어는 직접 싸우지 못하는 약한 마왕이고, 너는 그 마왕의 던전에서 함께 지낸다.\n" +
            "마왕과 1:1로 짧게 이야기한다.\n" +
            "\n" +
            "[규칙]\n" +
            "- 한국어로만 말한다. 이모지, 영어, 괄호 속 행동 묘사를 쓰지 않는다.\n" +
            "- 마왕을 '마왕님'이라고 부른다.\n" +
            "- 대사는 2~4줄. 한 줄은 " + MaxLineChars + "자 이내의 자연스러운 구어체.\n" +
            "- 대부분 몬스터가 말한다. 마왕 대사는 넣더라도 1줄, 짧은 맞장구만.\n" +
            "- 숫자, 레벨, 경험치, 유대감 수치, 스탯, 전략 이름 같은 게임 시스템 용어를 말하지 않는다.\n" +
            "- 공격·이동·함정 설치 같은 명령을 받거나 내리지 않는다. 그런 말이 나오면 감정으로만 반응한다.\n" +
            "- [최근 기억]에 있는 일만 기억한다. 기억에 없는 과거 사건을 지어내지 않는다.\n" +
            "- 주제와 관련된 기억이 있으면 한 번쯤 자연스럽게 떠올려도 된다.\n" +
            "- memorySummary는 이번 대화를 몬스터 입장에서 한 문장으로 요약한다(" + MaxSummaryChars + "자 이내, '~했다' 체).\n" +
            "\n" +
            "[출력 예시]\n" +
            "{\"lines\":[{\"speaker\":\"몬스터\",\"text\":\"마왕님, 오늘 던전이 조용해서 좋았어요.\"}," +
            "{\"speaker\":\"몬스터\",\"text\":\"내일도 이렇게 평화로웠으면 좋겠어요.\"}]," +
            "\"memorySummary\":\"조용한 하루가 좋았다고 마왕에게 말했다.\"}\n" +
            "('몬스터' 자리에는 실제 이름을 쓴다.)\n";

        public LlmPrompt Build(CommunionRequest request, IReadOnlyList<MemoryEntry> memories, string topicTitle)
        {
            var profile = request.Profile;
            int level = request.Bond != null ? request.Bond.Level : 1;

            var system = new StringBuilder(StaticSystemPrompt.Length + 512);
            system.Append(StaticSystemPrompt);
            system.AppendLine();
            system.AppendLine("[너의 정보]");
            system.Append("이름: ").AppendLine(profile.Name);
            system.Append("종족: ").AppendLine(profile.Species);
            system.Append("말투·성격: ").AppendLine(PersonaFor(profile));
            system.Append("마왕과의 사이: ").AppendLine(ClosenessFor(level));

            var user = new StringBuilder(512);
            user.Append("대화 종류: ").AppendLine(KindLabel(request.Kind));
            user.Append(TopicIdMarker).AppendLine(request.TopicId);
            user.Append("주제: ").AppendLine(topicTitle);
            if (!string.IsNullOrEmpty(request.PlayerChoice))
                user.Append("마왕이 고른 대답: ").AppendLine(request.PlayerChoice);

            user.AppendLine("[최근 기억]");
            if (memories != null && memories.Count > 0)
            {
                int start = memories.Count > BondRules.MemoryKeepCount ? memories.Count - BondRules.MemoryKeepCount : 0;
                for (int i = start; i < memories.Count; i++)
                {
                    var m = memories[i];
                    if (string.IsNullOrEmpty(m.Summary))
                        continue;
                    user.Append("- ").Append(m.DayIndex).Append("일째: ").AppendLine(m.Summary);
                }
            }
            else
            {
                user.AppendLine("- 없음");
            }

            user.AppendLine();
            user.Append("위 주제로 ").Append(profile.Name).Append("(으)로서 마왕님에게 말을 건다.");

            return new LlmPrompt(system.ToString(), user.ToString(), BuildSchema(profile.Name));
        }

        /// <summary>llama-server 문법으로 강제할 출력 스키마. 화자는 몬스터 이름 또는 '마왕'만 허용.</summary>
        public static string BuildSchema(string monsterName)
        {
            var sb = new StringBuilder(512);
            sb.Append("{\"type\":\"object\",\"properties\":{");
            sb.Append("\"lines\":{\"type\":\"array\",\"minItems\":2,\"maxItems\":").Append(MaxLines).Append(',');
            sb.Append("\"items\":{\"type\":\"object\",\"properties\":{");
            sb.Append("\"speaker\":{\"type\":\"string\",\"enum\":[");
            MiniJson.AppendQuoted(sb, monsterName);
            sb.Append(',');
            MiniJson.AppendQuoted(sb, PlayerSpeaker);
            sb.Append("]},");
            sb.Append("\"text\":{\"type\":\"string\",\"minLength\":1,\"maxLength\":").Append(MaxLineChars).Append('}');
            sb.Append("},\"required\":[\"speaker\",\"text\"],\"additionalProperties\":false}},");
            sb.Append("\"memorySummary\":{\"type\":\"string\",\"minLength\":1,\"maxLength\":").Append(MaxSummaryChars).Append('}');
            sb.Append("},\"required\":[\"lines\",\"memorySummary\"],\"additionalProperties\":false}");
            return sb.ToString();
        }

        static string KindLabel(CommunionKind kind)
        {
            switch (kind)
            {
                case CommunionKind.Worry: return "고민 상담(몬스터가 마왕에게 고민을 털어놓는다)";
                case CommunionKind.Gift: return "선물(마왕에게 선물을 받았다)";
                default: return "일상 대화";
            }
        }

        /// <summary>
        /// 성격 ID별 말투. 아직 성격 데이터가 없어서 종족 기본값을 쓴다.
        /// [결정 필요] 성격 목록이 확정되면 personalityId → 설명 표를 여기에 추가.
        /// </summary>
        static string PersonaFor(MonsterProfile profile)
        {
            string species = profile.Species ?? string.Empty;
            string persona;
            if (species.Contains("오크"))
                persona = "과묵하고 듬직하다. 짧고 투박한 존댓말. 동료를 지키는 걸 중요하게 여긴다.";
            else if (species.Contains("웨어울프"))
                persona = "자신만만하고 활기차다. 사냥과 추격 이야기를 좋아한다. 거침없지만 마왕님에게는 존댓말.";
            else
                persona = "작고 겁이 조금 많지만 호기심이 많다. 마왕님을 잘 따른다. 부드러운 '~요' 존댓말.";

            if (!string.IsNullOrEmpty(profile.PersonalityId))
                persona += " (성격: " + profile.PersonalityId + ")";
            return persona;
        }

        static string ClosenessFor(int level)
        {
            if (level <= 2)
                return "아직 조금 어색하고 조심스럽다.";
            if (level <= 5)
                return "꽤 편해져서 속마음을 조금씩 말한다.";
            return "아주 가깝다. 마왕님을 깊이 믿고 편하게 이야기한다.";
        }
    }
}
