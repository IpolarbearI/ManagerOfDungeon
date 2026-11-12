using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public sealed class CommunionResult
    {
        /// <summary>false면 대화 자체를 시작할 수 없음(횟수 초과 등). Error 표시.</summary>
        public bool Ok { get; }
        public string Error { get; }
        public IReadOnlyList<DialogueLine> Lines { get; }
        public string MemorySummary { get; }
        /// <summary>대화를 끝까지 보면 지급될 EXP. 폴백이면 0.</summary>
        public int ExpGranted { get; }
        /// <summary>
        /// true = LLM 요청이 실패해서 준비된 대사를 보여주는 중.
        /// 명세 6.1: EXP·기억 미지급, 일일/추가 기회 미소진(Complete()가 아무것도 반영하지 않음).
        /// </summary>
        public bool UsedFallback { get; }
        public string FallbackReason { get; }

        public CommunionResult(
            bool ok,
            string error,
            IReadOnlyList<DialogueLine> lines,
            string memorySummary,
            int expGranted,
            bool usedFallback,
            string fallbackReason = null)
        {
            Ok = ok;
            Error = error ?? string.Empty;
            Lines = lines ?? System.Array.Empty<DialogueLine>();
            MemorySummary = memorySummary ?? string.Empty;
            ExpGranted = expGranted;
            UsedFallback = usedFallback;
            FallbackReason = fallbackReason ?? string.Empty;
        }

        public static CommunionResult Fail(string error)
        {
            return new CommunionResult(false, error, System.Array.Empty<DialogueLine>(), string.Empty, 0, false);
        }
    }
}
