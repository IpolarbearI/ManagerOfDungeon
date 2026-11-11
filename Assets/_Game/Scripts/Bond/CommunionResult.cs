using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public sealed class CommunionResult
    {
        public bool Ok { get; }
        public string Error { get; }
        public IReadOnlyList<DialogueLine> Lines { get; }
        public string MemorySummary { get; }
        public int ExpGranted { get; }
        public bool UsedFallback { get; }

        public CommunionResult(
            bool ok,
            string error,
            IReadOnlyList<DialogueLine> lines,
            string memorySummary,
            int expGranted,
            bool usedFallback)
        {
            Ok = ok;
            Error = error ?? string.Empty;
            Lines = lines ?? System.Array.Empty<DialogueLine>();
            MemorySummary = memorySummary ?? string.Empty;
            ExpGranted = expGranted;
            UsedFallback = usedFallback;
        }

        public static CommunionResult Fail(string error)
        {
            return new CommunionResult(false, error, System.Array.Empty<DialogueLine>(), string.Empty, 0, false);
        }
    }
}
