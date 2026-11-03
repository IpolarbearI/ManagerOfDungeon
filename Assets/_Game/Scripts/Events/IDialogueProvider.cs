using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public readonly struct DialogueLine
    {
        public DialogueLine(string speaker, string text)
        {
            Speaker = speaker ?? string.Empty;
            Text = text ?? string.Empty;
        }

        public string Speaker { get; }
        public string Text { get; }
    }

    public interface IDialogueProvider
    {
        IReadOnlyList<DialogueLine> GetLines(string topicId);
    }
}
