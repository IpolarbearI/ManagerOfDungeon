namespace ManagerOfDungeon
{
    public enum SessionMode
    {
        None,
        /// <summary>A1.01 고정 시연(AlphaDemoDirector). 발표용으로 보존.</summary>
        Showcase,
        /// <summary>A2 본 게임(GameDirector).</summary>
        Game
    }

    /// <summary>
    /// 씬 사이에 살아남는 현재 판. 로비에서 모드를 정하고 Dungeon 씬이 이를 읽는다.
    /// </summary>
    public static class GameSession
    {
        public static SessionMode Mode { get; private set; } = SessionMode.None;
        public static GameState Current { get; private set; }

        public static bool IsGame => Mode == SessionMode.Game && Current != null;

        /// <summary>던전 → 교감(Monster) 씬으로 넘어갈 때 대상 몬스터 id.</summary>
        public static string InteractionMonsterId { get; set; } = string.Empty;

        /// <summary>씬을 오간 뒤 던전 화면 하단에 띄울 메시지.</summary>
        public static string PendingStatus { get; set; } = string.Empty;

        /// <summary>교감 화면에서 오른 레벨. 던전에 돌아오면 머리 위 '♥ Lv.N ↑'와 전략 해금 팝업으로 보여준다.</summary>
        public static readonly System.Collections.Generic.List<BondLevelUp> PendingLevelUps =
            new System.Collections.Generic.List<BondLevelUp>();

        public static void StartShowcase()
        {
            Mode = SessionMode.Showcase;
            Current = null;
            DemoSession.ResetCurrent();
        }

        public static void StartNewGame(int seed)
        {
            Mode = SessionMode.Game;
            Current = CreateNewState(seed);
        }

        public static bool Continue()
        {
            var state = SaveService.LoadLatest();
            if (state == null)
                return false;
            Mode = SessionMode.Game;
            Current = state;
            return true;
        }

        public static bool LoadSlot(int slot)
        {
            var state = SaveService.Load(slot);
            if (state == null)
                return false;
            Mode = SessionMode.Game;
            Current = state;
            return true;
        }

        public static void Clear()
        {
            PendingLevelUps.Clear();
            Mode = SessionMode.None;
            Current = null;
        }

        /// <summary>
        /// A2 시작 상태 (06-1): 20솔, 코인 1, 코어·방 1개, 고블린 2(고철 검 / 고목 활).
        /// 시작 자산 420.
        /// </summary>
        public static GameState CreateNewState(int seed)
        {
            var state = new GameState();
            state.rng = new GameRng(seed);
            state.day = 1;
            state.clockMinutes = BalanceConfig.DayStartMinute;
            state.dayStarted = false;
            state.sol = BalanceConfig.StartSol;
            state.coins = BalanceConfig.StartCoins;

            var a = new MonsterState();
            a.id = state.NewId("mon");
            a.speciesId = GameDefs.Goblin;
            a.name = "고블린 A";
            a.weaponId = GameDefs.ScrapSword;
            a.posX = 0.78f;
            a.posY = 0.56f;
            state.monsters.Add(a);

            var b = new MonsterState();
            b.id = state.NewId("mon");
            b.speciesId = GameDefs.Goblin;
            b.name = "고블린 B";
            b.weaponId = GameDefs.OldWoodBow;
            b.posX = 0.80f;
            b.posY = 0.38f;
            state.monsters.Add(b);

            state.AddLog("새 게임 시작 · 7일을 버티세요.");
            return state;
        }
    }
}
