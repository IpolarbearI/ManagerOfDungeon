using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ManagerOfDungeon
{
    /// <summary>GameFlow 상태 (설계 문서의 상태 전이도).</summary>
    public enum GamePhase
    {
        Daily,
        Battle,
        BattleResult,
        Ending,
        GameOver
    }

    public enum PlacementMode
    {
        None,
        Trap
    }

    /// <summary>
    /// A2 본 게임 진행자. Dungeon 씬에서 GameSession.IsGame일 때 DungeonMap이 붙인다.
    /// 화면은 기존 DungeonRoomScreen(상단 바·설계 메뉴·유닛 카드·하단 바)을 그대로 쓰고,
    /// 교감은 기존 Monster 씬을 쓴다. 이 클래스는 IDungeonScreenHost로 그 화면에 값을 공급한다.
    /// 하는 일: 시계 진행 → 정시 처리(고민) → 침입 → 전투 → 정산 → 22:00 → 다음 날/엔딩.
    /// </summary>
    public sealed class GameDirector : MonoBehaviour, IDungeonScreenHost
    {
        GameState _state;
        CombatDirector _combat;
        DungeonRoomScreen _screen;
        GameHud _hud;
        CommunionService _communion;
        bool _bound;

        GamePhase _phase = GamePhase.Daily;
        PlacementMode _placement = PlacementMode.None;
        string _selectedMonsterId = string.Empty;
        string _selectedTrapId = string.Empty;
        bool _paused;
        bool _popupOpen;
        readonly List<KeyValuePair<StrategyInfo, string>> _unlockQueue = new List<KeyValuePair<StrategyInfo, string>>();
        string _status = string.Empty;
        SettlementReport _lastReport;

        public GameState State => _state;
        public GamePhase Phase => _phase;
        public CombatDirector Combat => _combat;
        public string Status => _status;
        public SettlementReport LastReport => _lastReport;
        public MonsterState SelectedMonster => _state != null ? _state.FindMonster(_selectedMonsterId) : null;
        public TrapState SelectedTrap => _state != null ? _state.FindTrap(_selectedTrapId) : null;
        public bool CanEdit => _bound && _phase == GamePhase.Daily && !_paused && !_popupOpen;
        public bool CanSave => _phase == GamePhase.Daily;

        // ───────────── 시작 ─────────────

        /// <summary>DungeonRoomScreen이 준비되면 호출한다(카메라·UI가 먼저 있어야 방 좌표가 맞는다).</summary>
        public void Bind(DungeonRoomScreen screen, CombatDirector combat)
        {
            _screen = screen;
            _combat = combat;
            if (_bound)
            {
                Refresh();
                return;
            }

            _state = GameSession.Current;
            if (_state == null)
            {
                GameSession.StartNewGame(System.Environment.TickCount);
                _state = GameSession.Current;
            }

            _communion = CommunionService.ForGame(_state);
            _hud = GetComponent<GameHud>();
            if (_hud == null)
                _hud = gameObject.AddComponent<GameHud>();
            _hud.Bind(this);
            _bound = true;

            if (!_state.dayStarted)
                BeginDay();

            if (!string.IsNullOrEmpty(GameSession.PendingStatus))
            {
                _status = GameSession.PendingStatus;
                GameSession.PendingStatus = string.Empty;
            }

            _phase = _state.gameOver ? GamePhase.GameOver : GamePhase.Daily;
            RebuildRoom();
            GameMusic.PlayDungeon();
            ApplyTimeScale();
            if (_phase == GamePhase.GameOver)
            {
                _hud.ShowGameOver();
                return;
            }

            // 교감 화면에서 오른 레벨 → 머리 위 표시 + 전략 해금 팝업
            ShowLevelUps(GameSession.PendingLevelUps);
            GameSession.PendingLevelUps.Clear();
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (!_bound || _state == null)
                return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePause();

            if (_paused || _popupOpen)
                return;

            float dt = Time.deltaTime;
            switch (_phase)
            {
                case GamePhase.Daily:
                    TickDaily(dt);
                    if (_phase == GamePhase.Daily)
                        HandleClick();
                    break;
                case GamePhase.Battle:
                    TickBattle(dt);
                    break;
            }
        }

        // ───────────── 하루 진행 ─────────────

        void BeginDay()
        {
            DayStartPipeline.Run(_state);
            bool saved = SaveService.Save(_state, SaveService.AutoSlot);
            SetStatus($"Day {_state.day} 시작" + (saved ? " · 자동저장 완료" : string.Empty));
        }

        void TickDaily(float dt)
        {
            var tick = GameClock.Advance(_state, dt);
            for (int i = 0; i < tick.HoursPassed; i++)
                OnGameHour();

            if (InvasionScheduler.IsDue(_state))
            {
                StartInvasion();
                return;
            }

            if (tick.ReachedDayEnd)
                EndDay();
        }

        void TickBattle(float dt)
        {
            // 전투 중에도 시계는 흐르지만, 22:00이 지나도 전투가 끝날 때까지 날짜를 넘기지 않는다(04-1).
            var tick = GameClock.Advance(_state, dt);
            for (int i = 0; i < tick.HoursPassed; i++)
                OnGameHour();

            if (_combat.ConsumeBattleEnd())
                FinishBattle();
        }

        void OnGameHour()
        {
            for (int i = 0; i < _state.monsters.Count; i++)
            {
                var m = _state.monsters[i];
                if (m.hasWorry)
                    continue;

                _communion.TickWorryHour(m.id);
                string topic = BalanceConfig.AlphaWorryTopic;
                if (!_communion.CanWorry(topic, _state.monsters.Count - 1))
                    continue;
                if (_state.rng.Percent(m.worryChance))
                {
                    m.hasWorry = true;
                    m.worryTopicId = topic;
                    _state.AddLog($"{m.name}에게 고민이 생겼다.");
                    _hud.Banner($"{m.name}에게 고민이 생겼습니다");
                    var unit = _phase == GamePhase.Daily ? _combat.FindUnit(m.id) : null;
                    if (unit != null)
                        unit.SetWorryIcon(true);
                }
            }

            Refresh();
        }

        void EndDay()
        {
            CancelSpikePlacement();
            if (_state.day >= BalanceConfig.TargetDays && !_state.endless)
            {
                _phase = GamePhase.Ending;
                ApplyTimeScale();
                _hud.ShowEnding();
                return;
            }

            NextDay();
        }

        void NextDay()
        {
            GameClock.StartNewDay(_state);
            BeginDay();
            _phase = GamePhase.Daily;
            ApplyTimeScale();
            RebuildRoom();
        }

        // ───────────── 침입·전투 ─────────────

        void StartInvasion()
        {
            CancelSpikePlacement();
            ClearSelection();
            _screen?.HideDesignMenus();
            _state.invasion.todayTriggered = true;

            var party = _state.invasion.party;
            var names = new List<string>();
            for (int i = 0; i < party.heroes.Count; i++)
            {
                var h = party.heroes[i];
                var def = GameDefs.GetHero(h.jobId);
                names.Add((def != null ? def.DisplayName : h.jobId)
                    + $"(AI{h.aiLevel} · HP{h.hpLevel}/공{h.atkLevel}/속{h.asLevel})");
            }

            _state.AddLog($"용사 침입! {string.Join(", ", names)} · 파티 가치 {party.totalValue} (목표 {party.targetValue})");
            _hud.Banner("용사 파티가 침입했습니다!");

            _combat.BuildFromState(_state, party);
            _combat.StartBattle(false);
            _phase = GamePhase.Battle;
            SetStatus($"침입: {string.Join(", ", names)}");
        }

        void FinishBattle()
        {
            _combat.StopBattle();
            var result = _combat.CollectResult();
            _lastReport = BattleSettlement.Apply(_state, result);
            _phase = GamePhase.BattleResult;
            ApplyTimeScale();
            GameMusic.PlayDungeon();
            _hud.ShowResult(_lastReport);
            Refresh();
        }

        public void ConfirmResult()
        {
            if (_phase != GamePhase.BattleResult)
                return;

            _hud.HideModal();
            if (_state.gameOver)
            {
                _phase = GamePhase.GameOver;
                ApplyTimeScale();
                _hud.ShowGameOver();
                return;
            }

            _phase = GamePhase.Daily;
            ApplyTimeScale();
            RebuildRoom();
            if (_state.clockMinutes >= BalanceConfig.DayEndMinute)
                EndDay();

            if (_lastReport != null)
            {
                if (_lastReport.TrapsReinstallFailed > 0)
                    _hud.Banner($"자동 재설치 실패 {_lastReport.TrapsReinstallFailed}개 · 솔이 부족합니다");
                if (_phase == GamePhase.Daily)
                    ShowLevelUps(_lastReport.LevelUps);
            }
        }

        // ───────────── 유대감 레벨업 표시 (11-10 개정) ─────────────

        /// <summary>일반 레벨업은 머리 위 '♥ Lv.N ↑'만, 전략이 열리면 이름·설명 팝업.</summary>
        void ShowLevelUps(List<BondLevelUp> levelUps)
        {
            if (levelUps == null || levelUps.Count == 0)
                return;

            for (int i = 0; i < levelUps.Count; i++)
            {
                var up = levelUps[i];
                var unit = _combat.FindUnit(up.MonsterId);
                if (unit != null)
                    unit.ShowFloatingText($"♥ Lv.{up.ToLevel} ↑", new Color(1f, 0.45f, 0.55f, 1f));

                var unlocked = StrategyCatalog.UnlockedBetween(up.FromLevel, up.ToLevel);
                for (int k = 0; k < unlocked.Count; k++)
                    _unlockQueue.Add(new KeyValuePair<StrategyInfo, string>(unlocked[k], up.MonsterName));
            }

            if (!_popupOpen)
                ShowNextUnlock();
        }

        void ShowNextUnlock()
        {
            if (_unlockQueue.Count == 0)
            {
                _popupOpen = false;
                ApplyTimeScale();
                return;
            }

            var next = _unlockQueue[0];
            _unlockQueue.RemoveAt(0);
            _popupOpen = true;
            ApplyTimeScale();
            _hud.ShowStrategyUnlock(next.Key, next.Value, ShowNextUnlock);
        }

        // ───────────── 엔딩 (03-6 C안) ─────────────

        public void ChooseReturnToDemonRealm()
        {
            _state.AddLog("마계로 돌아갔다.");
            SaveService.Save(_state, SaveService.ManualSlot);
            ReturnToMenu();
        }

        public void ChooseStay()
        {
            _state.endless = true;
            _state.AddLog("이 던전의 마왕으로 남았다. (엔드리스)");
            _hud.HideModal();
            NextDay();
        }

        // ───────────── 입력 ─────────────

        void HandleClick()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            bool left = mouse.leftButton.wasPressedThisFrame;
            bool right = mouse.rightButton.wasPressedThisFrame;
            if (!left && !right)
                return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
            world.z = 0f;

            // 우클릭: 선택한 몬스터의 전투 시작 위치 지정
            if (right)
            {
                MoveSelectedMonsterTo(world);
                return;
            }

            if (_placement == PlacementMode.Trap)
            {
                PlaceTrapAt(world);
                return;
            }

            var unit = _combat.PickUnit(world, 0.95f);
            if (unit != null && unit.Team == CombatTeam.Defender)
            {
                SelectMonster(unit.ActorId);
                return;
            }

            var spike = _combat.PickSpike(world, 0.8f);
            if (spike != null && !string.IsNullOrEmpty(spike.TrapId))
            {
                SelectTrap(spike.TrapId);
                return;
            }

            ClearSelection();
        }

        void PlaceTrapAt(Vector3 world)
        {
            if (!_combat.IsInTrapZone(world))
            {
                SetStatus("함정은 용사와 고블린 사이 중앙에만 설치할 수 있습니다.");
                return;
            }

            var v = _combat.WorldToViewport(world);
            TrapService.TryBuy(_state, GameDefs.SpikeFloor, v.x, v.y, out var message);
            CancelSpikePlacement();
            RebuildRoom();
            SetStatus(message);
        }

        void MoveSelectedMonsterTo(Vector3 world)
        {
            var m = SelectedMonster;
            if (m == null)
                return;

            if (!_combat.IsInDefenderZone(world))
            {
                SetStatus("전투 시작 위치는 방 오른쪽(코어 쪽)에만 둘 수 있습니다.");
                return;
            }

            var v = _combat.WorldToViewport(world);
            m.posX = v.x;
            m.posY = v.y;
            var unit = _combat.FindUnit(m.id);
            if (unit != null)
                unit.SetHomePosition(world);
            SetStatus($"{m.name}의 전투 시작 위치를 저장했습니다.");
        }

        // ───────────── 선택 ─────────────

        void SelectMonster(string monsterId)
        {
            ClearSelection();
            _selectedMonsterId = monsterId ?? string.Empty;
            var unit = _combat.FindUnit(_selectedMonsterId);
            if (unit != null)
                unit.SetSelected(true);
            _screen?.HideDesignMenus();
            Refresh();
        }

        void SelectTrap(string trapId)
        {
            ClearSelection();
            _selectedTrapId = trapId ?? string.Empty;
            _screen?.HideDesignMenus();
            Refresh();
        }

        public void ClearSelection()
        {
            var unit = _combat != null ? _combat.FindUnit(_selectedMonsterId) : null;
            if (unit != null)
                unit.SetSelected(false);
            _selectedMonsterId = string.Empty;
            _selectedTrapId = string.Empty;
            Refresh();
        }

        // ───────────── IDungeonScreenHost: 화면 값 ─────────────

        public bool InBattle => _phase == GamePhase.Battle;

        public string Hint()
        {
            if (_phase == GamePhase.Battle)
                return "자동전투 중입니다. 용사를 직접 조작하지 않습니다.";
            if (_placement == PlacementMode.Trap)
                return "빨간 구역을 클릭해 가시 바닥을 설치하세요. (20솔)";
            if (SelectedMonster != null)
                return "우클릭으로 전투 시작 위치를 정합니다. ♥로 교감 화면에 들어갑니다.";
            if (_state != null && _state.day >= BalanceConfig.TargetDays && !_state.endless)
                return $"마지막 날입니다. 22:00까지 버티세요.";
            int defense = _state != null ? AssetCalculator.DefensePower(_state) : 0;
            return $"Day {(_state != null ? _state.day : 1)} / {BalanceConfig.TargetDays} · 방어 전투력 {defense} · 용사는 언제 올지 모릅니다.";
        }

        public DungeonHudInfo HudInfo
        {
            get
            {
                var info = new DungeonHudInfo();
                if (_state == null)
                    return info;
                info.Day = _state.day;
                info.Clock = GameClock.Format(_state.clockMinutes);
                info.Sol = _state.sol;
                info.Asset = AssetCalculator.Total(_state);
                info.Coins = _state.coins;
                info.CoinSlots = 3;
                info.Room = "방 1/1";
                return info;
            }
        }

        public bool WorryPending
        {
            get
            {
                if (_state == null)
                    return false;
                for (int i = 0; i < _state.monsters.Count; i++)
                {
                    if (_state.monsters[i].hasWorry)
                        return true;
                }

                return false;
            }
        }

        public UnitCardView Card
        {
            get
            {
                var m = SelectedMonster;
                if (m != null)
                    return MonsterCard(m);
                var t = SelectedTrap;
                return t != null ? TrapCard(t) : null;
            }
        }

        UnitCardView MonsterCard(MonsterState m)
        {
            var def = GameDefs.GetMonster(m.speciesId);
            var weapon = GameDefs.GetEquipment(m.weaponId);
            var unit = _combat != null ? _combat.FindUnit(m.id) : null;
            int need = BondRules.ExpToNextLevel(m.bondLevel);

            var card = new UnitCardView();
            card.Title = $"{m.name} - {(def != null ? def.DisplayName : m.speciesId)}";
            card.ShowHp = true;
            card.MaxHp = unit != null ? unit.MaxHealth : (def != null ? def.MaxHealth : 1f);
            card.Hp = unit != null ? unit.Health : card.MaxHp;
            card.PrimaryLabel = weapon != null ? $"{weapon.DisplayName} 착용 중" : "맨손 · 장비 착용";
            card.PrimaryInteractable = CanEdit;
            card.SlotSprite = WeaponIcon(m.weaponId);
            card.SlotFilled = weapon != null;
            card.SlotInteractable = CanEdit;
            card.ShowBond = true;
            card.Action = $"행동 : {(m.focusFire ? "점사 공격" : "기본 공격")} · 유대 Lv.{m.bondLevel}"
                + (need > 0 ? $" ({m.bondExp}/{need})" : string.Empty)
                + (m.hasWorry ? " · 고민 있음" : string.Empty);
            return card;
        }

        UnitCardView TrapCard(TrapState t)
        {
            var def = GameDefs.GetTrap(t.trapId);
            int price = def != null ? def.BuyPrice : 0;

            var card = new UnitCardView();
            card.Title = $"{(def != null ? def.DisplayName : t.trapId)} - 함정";
            card.ShowHp = false;
            int reinstall = def != null ? def.ReinstallPrice : 0;
            card.PrimaryLabel = t.used ? $"재설치 ({reinstall}솔)" : $"철거 (+{price}솔)";
            card.PrimaryInteractable = CanEdit;
            card.SlotSprite = PackAssets.LoadSprite(PackAssets.SpikeIdle);
            card.SlotFilled = t.autoReinstall;
            card.SlotInteractable = CanEdit;
            card.ShowBond = false;
            card.Action = $"상태 : {(t.used ? "사용됨" : "작동 가능")} · 자동 재설치 {(t.autoReinstall ? "ON" : "OFF")} (칸 클릭)";
            return card;
        }

        static Sprite WeaponIcon(string weaponId)
        {
            if (weaponId == GameDefs.OldWoodBow)
                return PackAssets.LoadSprite(PackAssets.OldWoodBow);
            if (weaponId == GameDefs.ScrapSword)
                return PackAssets.LoadSprite(PackAssets.ScrapSwordIcon);
            return null;
        }

        public bool StartBattleVisible => false;
        public bool StartBattleInteractable => false;

        public bool TryStartBattle()
        {
            return false; // 본 게임에서 침입은 확률로 온다(04-1).
        }

        // ───────────── IDungeonScreenHost: 버튼 ─────────────

        public void OnAlertButton()
        {
            if (_state != null)
                _hud.ShowLog(_state.log);
        }

        public void OnReportButton()
        {
            if (_state == null)
                return;
            for (int i = 0; i < _state.monsters.Count; i++)
            {
                if (_state.monsters[i].hasWorry)
                {
                    OpenMonsterScreen(_state.monsters[i].id);
                    return;
                }
            }

            SetStatus("지금 고민이 있는 몬스터가 없습니다.");
        }

        public void OnMenuButton()
        {
            TogglePause();
        }

        public bool CanOpenTrapPalette(out string reason)
        {
            reason = CanEdit ? string.Empty : "지금은 설계할 수 없습니다.";
            return CanEdit;
        }

        public void BeginSpikePlacement()
        {
            if (!CanEdit)
                return;
            var def = GameDefs.GetTrap(GameDefs.SpikeFloor);
            if (def != null && _state.sol < def.BuyPrice)
            {
                SetStatus($"솔이 부족합니다. (가시 바닥 {def.BuyPrice}솔)");
                return;
            }

            _placement = PlacementMode.Trap;
            _combat.SetTrapZoneHighlight(true);
            SetStatus("용사와 고블린 사이 빨간 구역에 가시 바닥을 설치하세요.");
        }

        public void CancelSpikePlacement()
        {
            _placement = PlacementMode.None;
            if (_combat != null)
                _combat.SetTrapZoneHighlight(false);
        }

        public void OnCardPrimary()
        {
            if (!CanEdit)
                return;

            if (SelectedMonster != null)
            {
                CycleWeapon();
                return;
            }

            var t = SelectedTrap;
            if (t == null)
                return;

            string message;
            if (t.used)
            {
                TrapService.TryReinstall(_state, t.id, out message);
            }
            else
            {
                TrapService.TryRemove(_state, t.id, out message);
                _selectedTrapId = string.Empty;
            }

            RebuildRoom();
            SetStatus(message);
        }

        public void OnCardSlot()
        {
            if (!CanEdit)
                return;

            if (SelectedMonster != null)
            {
                CycleWeapon();
                return;
            }

            var t = SelectedTrap;
            if (t == null)
                return;
            TrapService.ToggleAuto(_state, t.id);
            SetStatus(t.autoReinstall ? "자동 재설치 ON" : "자동 재설치 OFF");
        }

        public void OnCardBond()
        {
            var m = SelectedMonster;
            if (m != null)
                OpenMonsterScreen(m.id);
        }

        void CycleWeapon()
        {
            var m = SelectedMonster;
            if (m == null)
                return;
            string message = EquipmentService.CycleWeapon(_state, m.id);
            RebuildRoom();
            SetStatus(message);
        }

        /// <summary>기존 교감 화면(Monster 씬)으로 이동. 그동안 게임 시간은 멈춰 있다.</summary>
        void OpenMonsterScreen(string monsterId)
        {
            if (!CanEdit)
                return;

            GameSession.InteractionMonsterId = monsterId;
            Time.timeScale = 1f;
            SceneManager.LoadScene("Monster");
        }

        // ───────────── 시스템 ─────────────

        public void TogglePause()
        {
            if (!_bound || _popupOpen || _phase == GamePhase.BattleResult || _phase == GamePhase.Ending || _phase == GamePhase.GameOver)
                return;

            _paused = !_paused;
            ApplyTimeScale();
            if (_paused)
                _hud.ShowPause();
            else
                _hud.HideModal();
        }

        public void SaveManual()
        {
            if (!CanSave)
            {
                SetStatus("전투 중에는 저장할 수 없습니다.");
                return;
            }

            bool ok = SaveService.Save(_state, SaveService.ManualSlot);
            SetStatus(ok ? "저장했습니다." : "저장에 실패했습니다.");
        }

        public void LoadManual()
        {
            if (!GameSession.LoadSlot(SaveService.ManualSlot))
            {
                SetStatus("수동 저장 파일이 없습니다.");
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            GameSession.Clear();
            SceneManager.LoadScene("Lobby");
        }

        public void SetStatus(string status)
        {
            _status = status ?? string.Empty;
            Refresh();
        }

        // ───────────── 내부 ─────────────

        void RebuildRoom()
        {
            string keep = _selectedMonsterId;
            _combat.BuildFromState(_state, null);
            if (!string.IsNullOrEmpty(keep))
            {
                var unit = _combat.FindUnit(keep);
                if (unit != null)
                    unit.SetSelected(true);
            }

            Refresh();
        }

        void Refresh()
        {
            if (_screen != null)
                _screen.RefreshDemoUi();
        }

        void ApplyTimeScale()
        {
            bool frozen = _paused
                || _popupOpen
                || _phase == GamePhase.BattleResult
                || _phase == GamePhase.Ending
                || _phase == GamePhase.GameOver;
            Time.timeScale = frozen ? 0f : 1f;
        }
    }
}
