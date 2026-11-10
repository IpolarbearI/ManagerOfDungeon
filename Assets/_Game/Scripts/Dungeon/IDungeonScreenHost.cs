using UnityEngine;

namespace ManagerOfDungeon
{
    /// <summary>상단 바에 그릴 값.</summary>
    public struct DungeonHudInfo
    {
        public int Day;
        public string Clock;
        public int Sol;
        public int Asset;
        public int Coins;
        public int CoinSlots;
        public string Room;
    }

    /// <summary>하단 유닛 카드에 그릴 값. null이면 카드를 숨긴다.</summary>
    public sealed class UnitCardView
    {
        public string Title = string.Empty;
        public bool ShowHp = true;
        public float Hp;
        public float MaxHp = 1f;
        public string PrimaryLabel = string.Empty;
        public bool PrimaryInteractable;
        public Sprite SlotSprite;
        public bool SlotFilled;
        public bool SlotInteractable;
        public bool ShowBond = true;
        public string Action = string.Empty;
    }

    /// <summary>
    /// 던전 화면(DungeonRoomScreen)을 움직이는 쪽. 화면 구성은 하나이고,
    /// A1 쇼케이스(AlphaDemoDirector)와 A2 본 게임(GameDirector)이 각각 이 인터페이스를 구현한다.
    /// </summary>
    public interface IDungeonScreenHost
    {
        CombatDirector Combat { get; }
        void Bind(DungeonRoomScreen screen, CombatDirector combat);

        bool InBattle { get; }
        string Hint();
        string Status { get; }
        void SetStatus(string status);

        // 상단 바
        DungeonHudInfo HudInfo { get; }
        bool WorryPending { get; }
        void OnAlertButton();
        void OnReportButton();
        void OnMenuButton();

        // 설계 메뉴
        bool CanOpenTrapPalette(out string reason);
        void BeginSpikePlacement();
        void CancelSpikePlacement();
        void ClearSelection();

        // 유닛 카드
        UnitCardView Card { get; }
        void OnCardPrimary();
        void OnCardSlot();
        void OnCardBond();

        // 하단 바
        bool StartBattleVisible { get; }
        bool StartBattleInteractable { get; }
        bool TryStartBattle();
    }
}
