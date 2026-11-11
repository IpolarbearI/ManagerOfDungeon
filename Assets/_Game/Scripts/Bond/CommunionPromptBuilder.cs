using System.Collections.Generic;
using System.Text;

namespace ManagerOfDungeon
{
    public sealed class CommunionPromptBuilder
    {
        public LlmPrompt Build(CommunionRequest request, IReadOnlyList<MemoryEntry> memories, string topicTitle)
        {
            var profile = request.Profile;
            var system = new StringBuilder();
            system.AppendLine("너는 Manager of Dungeon의 몬스터다. 약한 마왕과 1:1로 한국어로 짧게 대화한다.");
            system.AppendLine("전투 이동, 공격, 함정 설치, 전략 ON/OFF 같은 명령을 실행하지 않는다. 그런 말을 들으면 대사로만 반응한다.");
            system.AppendLine("출력은 JSON 한 덩어리만 한다. 키: speaker, lines(2~4개 문자열 배열), memorySummary(한 줄).");
            system.Append("종족: ").Append(profile.Species).AppendLine();
            system.Append("이름: ").Append(profile.Name).AppendLine();
            if (!string.IsNullOrEmpty(profile.PersonalityId))
                system.Append("성격 ID: ").Append(profile.PersonalityId).AppendLine();
            system.Append("유대감 Lv.").Append(request.Bond != null ? request.Bond.Level : 1).AppendLine();

            var user = new StringBuilder();
            user.Append("종류: ").Append(request.Kind).AppendLine();
            user.Append("주제 ID: ").Append(request.TopicId).AppendLine();
            user.Append("주제: ").Append(topicTitle).AppendLine();
            if (!string.IsNullOrEmpty(request.PlayerChoice))
                user.Append("마왕 선택: ").Append(request.PlayerChoice).AppendLine();

            if (memories != null && memories.Count > 0)
            {
                user.AppendLine("최근 기억:");
                for (int i = 0; i < memories.Count; i++)
                {
                    var m = memories[i];
                    user.Append("- Day ").Append(m.DayIndex).Append(' ').Append(m.TopicId).Append(": ").AppendLine(m.Summary);
                }
            }
            else
            {
                user.AppendLine("최근 기억: 없음");
            }

            return new LlmPrompt(system.ToString(), user.ToString());
        }
    }
}
