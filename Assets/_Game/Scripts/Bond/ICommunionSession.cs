using System.Threading;
using System.Threading.Tasks;

namespace ManagerOfDungeon
{
    public interface ICommunionSession
    {
        bool IsBusy { get; }
        bool CanTalk(string actorId, int dayIndex);
        bool CanWorry(string topicId, int otherMonsterCount);
        Task<CommunionResult> StartTalkAsync(CommunionRequest request, CancellationToken ct = default);
        Task<CommunionResult> StartWorryAsync(CommunionRequest request, CancellationToken ct = default);
        Task<CommunionResult> GiveGiftAsync(CommunionRequest request, CancellationToken ct = default);
        /// <summary>대화를 끝까지 봤을 때 호출. 실제로 EXP·기억을 반영했으면 true(폴백이면 false).</summary>
        bool Complete();
        void TickWorryHour(string actorId);
        int GetWorryChancePercent(string actorId);
        void ClearPending();
    }
}
