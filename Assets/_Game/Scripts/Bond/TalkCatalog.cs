using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public readonly struct TalkTopic
    {
        public TalkTopic(string id, string title)
        {
            Id = id ?? string.Empty;
            Title = title ?? string.Empty;
        }

        public string Id { get; }
        public string Title { get; }
    }

    public static class TalkCatalog
    {
        public const string Talk01 = "TALK_01";
        public const string Talk02 = "TALK_02";
        public const string Talk03 = "TALK_03";
        public const string Talk04 = "TALK_04";
        public const string Talk05 = "TALK_05";

        static readonly TalkTopic[] Topics =
        {
            new TalkTopic(Talk01, "오늘 재밌게 놀았다"),
            new TalkTopic(Talk02, "용사가 왜 쳐들어오는 걸까"),
            new TalkTopic(Talk03, "마계는 어때?"),
            new TalkTopic(Talk04, "싸울 때 이렇게 싸우는 게 더 기분이 좋아"),
            new TalkTopic(Talk05, "여기에 와서 너무 기뻐")
        };

        public static IReadOnlyList<TalkTopic> All => Topics;

        public static bool TryGet(string id, out TalkTopic topic)
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
    }
}
