using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public sealed class ScriptedDialogueProvider : IDialogueProvider
    {
        static readonly DialogueLine[] WorryFive =
        {
            new DialogueLine("고블린", "이유 없이 화가 나요."),
            new DialogueLine("마왕", "갑자기? 무슨 일 있었어?"),
            new DialogueLine("고블린", "몰라요. 그냥 답답하고 짜증이 나요."),
            new DialogueLine("마왕", "그래도 괜찮아. 같이 있어 줄게.")
        };

        public IReadOnlyList<DialogueLine> GetLines(string topicId)
        {
            if (topicId == ResourceIds.WorryFive)
                return WorryFive;

            return System.Array.Empty<DialogueLine>();
        }
    }
}
