using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public readonly struct WorryTopic
    {
        public WorryTopic(string id, string title, bool needsOtherMonster)
        {
            Id = id ?? string.Empty;
            Title = title ?? string.Empty;
            NeedsOtherMonster = needsOtherMonster;
        }

        public string Id { get; }
        public string Title { get; }
        public bool NeedsOtherMonster { get; }
    }

    public static class WorryCatalog
    {
        public const string Worry01 = "WORRY_01";
        public const string Worry02 = "WORRY_02";
        public const string Worry03 = "WORRY_03";
        public const string Worry04 = "WORRY_04";
        public const string Worry05 = "WORRY_05";

        static readonly WorryTopic[] Topics =
        {
            new WorryTopic(Worry01, "선물을 받고 싶다", false),
            new WorryTopic(Worry02, "다른 몬스터와 친해지고 싶다", true),
            new WorryTopic(Worry03, "다른 몬스터와 다퉜다", true),
            new WorryTopic(Worry04, "전투 중 다치는 것은 아프다", false),
            new WorryTopic(Worry05, "이유 없이 화가 난다", false)
        };

        public static IReadOnlyList<WorryTopic> All => Topics;

        public static bool TryGet(string id, out WorryTopic topic)
        {
            for (int i = 0; i < Topics.Length; i++)
            {
                if (Topics[i].Id == id)
                {
                    topic = Topics[i];
                    return true;
                }
            }

            topic = default;
            return false;
        }

        public static bool CanOccur(string id, int otherMonsterCount)
        {
            if (!TryGet(id, out var topic))
                return false;
            if (topic.NeedsOtherMonster && otherMonsterCount < 1)
                return false;
            return true;
        }
    }
}
