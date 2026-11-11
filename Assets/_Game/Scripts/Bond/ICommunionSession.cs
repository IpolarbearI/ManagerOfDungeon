namespace ManagerOfDungeon
{
    public interface ICommunionSession
    {
        bool CanTalk(string actorId, int dayIndex);
        bool CanWorry(string topicId, int otherMonsterCount);
        CommunionResult StartTalk(CommunionRequest request);
        CommunionResult StartWorry(CommunionRequest request);
        CommunionResult GiveGift(CommunionRequest request);
        bool Complete();
        void TickWorryHour(string actorId);
        int GetWorryChancePercent(string actorId);
        void ClearPending();
    }
}
