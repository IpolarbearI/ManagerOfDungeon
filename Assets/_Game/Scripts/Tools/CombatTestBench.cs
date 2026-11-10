using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 전투 테스트 벤치 (개발용). 메뉴 "Manager of Dungeon/Combat Test Scene"으로 씬을 만들고 Play.
    /// - 왼쪽: 몬스터·용사·함정 구성 → 전투 시작 / N회 반복(배속) → 승률·평균 시간 집계
    /// - 오른쪽: 종족·직업 기본 스탯, 유대 보너스, 용사 강화표를 실시간 수정(다음 전투부터 적용)
    /// - '수치 복사'로 현재 값을 클립보드에 복사 → GameDefs / BalanceConfig에 옮겨 적는다.
    /// Play를 멈추면 수정한 수치는 원래대로 돌아간다(코드 값이 기준).
    /// 전투 로직은 본 게임과 같은 CombatDirector.BuildFromState를 쓴다.
    /// </summary>
    public sealed class CombatTestBench : MonoBehaviour
    {
        const float RefWidth = 1920f;
        const float RefHeight = 1080f;
        const int MaxDefenders = 6;
        const int MaxHeroes = 4;
        const int MaxResultLines = 14;

        static readonly string[] SpeciesIds = { GameDefs.Goblin, GameDefs.Orc, GameDefs.Werewolf };
        static readonly string[] HeroJobIds = { GameDefs.Warrior, GameDefs.Archer, GameDefs.Mage };
        static readonly string[] WeaponIds = { string.Empty, GameDefs.ScrapSword, GameDefs.OldWoodBow };
        static readonly float[] Speeds = { 1f, 2f, 4f, 8f };

        CombatDirector _combat;
        Font _font;
        GUIStyle _box;
        GUIStyle _title;

        readonly List<MonsterState> _defenders = new List<MonsterState>();
        readonly List<HeroState> _heroes = new List<HeroState>();
        int _spikeCount;
        int _speedIndex;
        int _repeatTarget = 20;
        float _timeLimit = 120f;
        int _seed = 1;

        bool _running;
        int _remaining;
        int _battleNo;
        int _wins;
        int _losses;
        int _draws;
        float _totalTime;
        readonly List<string> _results = new List<string>();

        Vector2 _leftScroll;
        Vector2 _rightScroll;
        readonly Dictionary<string, string> _buffers = new Dictionary<string, string>();
        string _defaultsText = string.Empty;
        string _message = string.Empty;

        // ───────────── 준비 ─────────────

        void Start()
        {
            EnsureCamera();
            EnsureMapArt();
            _combat = GetComponent<CombatDirector>();
            if (_combat == null)
                _combat = gameObject.AddComponent<CombatDirector>();
            _font = PackAssets.KoreanFont();
            _defaultsText = ExportText();

            // 기본 구성: A2 시작과 같은 고블린 2 (고철 검 / 고목 활) vs 전사 + 궁수
            AddDefender(GameDefs.Goblin, GameDefs.ScrapSword);
            AddDefender(GameDefs.Goblin, GameDefs.OldWoodBow);
            AddHero(GameDefs.Warrior);
            AddHero(GameDefs.Archer);
            BuildPreview();
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void AddDefender(string species, string weapon)
        {
            if (_defenders.Count >= MaxDefenders)
                return;
            var m = new MonsterState();
            m.speciesId = species;
            m.weaponId = weapon;
            m.bondLevel = 1;
            _defenders.Add(m);
        }

        void AddHero(string job)
        {
            if (_heroes.Count >= MaxHeroes)
                return;
            var h = new HeroState();
            h.jobId = job;
            _heroes.Add(h);
        }

        GameState BuildState()
        {
            var state = new GameState();
            state.rng = new GameRng(_seed);
            int n = _defenders.Count;
            for (int i = 0; i < n; i++)
            {
                var src = _defenders[i];
                var def = GameDefs.GetMonster(src.speciesId);
                var m = new MonsterState();
                m.id = "mon_" + i;
                m.speciesId = src.speciesId;
                m.name = (def != null ? def.DisplayName : src.speciesId) + " " + (i + 1);
                m.weaponId = src.weaponId;
                m.bondLevel = src.bondLevel;
                m.focusFire = src.focusFire;
                m.posX = i % 2 == 0 ? 0.78f : 0.84f;
                m.posY = 0.22f + 0.56f * (i + 0.5f) / Mathf.Max(1, n);
                state.monsters.Add(m);
            }

            for (int i = 0; i < _spikeCount; i++)
            {
                var trap = new TrapState();
                trap.id = "trap_" + i;
                trap.trapId = GameDefs.SpikeFloor;
                trap.posX = 0.50f + (i % 2) * 0.08f;
                trap.posY = 0.35f + 0.30f * (i + 0.5f) / Mathf.Max(1, _spikeCount);
                state.traps.Add(trap);
            }

            return state;
        }

        HeroPartyState BuildParty()
        {
            var party = new HeroPartyState();
            for (int i = 0; i < _heroes.Count; i++)
            {
                var src = _heroes[i];
                var h = new HeroState();
                h.jobId = src.jobId;
                h.hpLevel = src.hpLevel;
                h.atkLevel = src.atkLevel;
                h.asLevel = src.asLevel;
                h.aiLevel = src.aiLevel;
                party.heroes.Add(h);
                party.totalValue += h.Value();
            }

            return party;
        }

        // ───────────── 전투 진행 ─────────────

        void BuildPreview()
        {
            StopRun();
            _combat.BuildFromState(BuildState(), BuildParty());
        }

        void StartRun(int count)
        {
            _remaining = Mathf.Max(1, count);
            _running = true;
            StartOneBattle();
        }

        void StopRun()
        {
            _running = false;
            _remaining = 0;
            Time.timeScale = 1f;
            if (_combat != null)
                _combat.StopBattle();
        }

        void StartOneBattle()
        {
            _remaining--;
            _battleNo++;
            _combat.BuildFromState(BuildState(), BuildParty());
            _combat.StartBattle(false);
            Time.timeScale = Speeds[_speedIndex];
        }

        void Update()
        {
            if (!_running || _combat == null)
                return;

            bool ended = _combat.ConsumeBattleEnd();
            bool timeout = !ended && _combat.InBattle && _combat.BattleTime >= _timeLimit;
            if (!ended && !timeout)
                return;

            _combat.StopBattle();
            Record(timeout);

            if (_remaining > 0)
                StartOneBattle();
            else
                StopRun();
        }

        void Record(bool timeout)
        {
            var result = _combat.CollectResult();
            float time = _combat.BattleTime;
            string outcome;
            if (timeout)
            {
                _draws++;
                outcome = "무승부(시간 초과)";
            }
            else if (result.DefendersWon)
            {
                _wins++;
                outcome = "몬스터 승";
            }
            else
            {
                _losses++;
                outcome = "용사 승";
            }

            _totalTime += time;
            var sb = new StringBuilder();
            sb.Append('#').Append(_battleNo).Append(' ').Append(outcome).Append(" · ").Append(time.ToString("0.0")).Append("초 · ");
            var units = _combat.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (u == null || !u.IsAlive)
                    continue;
                sb.Append(u.UnitName).Append(' ').Append(u.DisplayHealth).Append('/').Append(u.DisplayMaxHealth).Append("  ");
            }

            _results.Insert(0, sb.ToString());
            while (_results.Count > MaxResultLines)
                _results.RemoveAt(_results.Count - 1);
        }

        void ClearResults()
        {
            _battleNo = 0;
            _wins = 0;
            _losses = 0;
            _draws = 0;
            _totalTime = 0f;
            _results.Clear();
        }

        // ───────────── 화면 ─────────────

        void OnGUI()
        {
            if (_font != null)
                GUI.skin.font = _font;
            EnsureStyles();

            float scale = Mathf.Min(Screen.width / RefWidth, Screen.height / RefHeight);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            GUILayout.BeginArea(new Rect(10f, 10f, 560f, height - 20f), _box);
            _leftScroll = GUILayout.BeginScrollView(_leftScroll);
            DrawSetup();
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(width - 610f, 10f, 600f, height - 20f), _box);
            _rightScroll = GUILayout.BeginScrollView(_rightScroll);
            DrawStatEditor();
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(580f, height - 330f, width - 1200f, 320f), _box);
            DrawResults();
            GUILayout.EndArea();
        }

        void EnsureStyles()
        {
            if (_box != null)
                return;
            _box = new GUIStyle(GUI.skin.box);
            _box.alignment = TextAnchor.UpperLeft;
            _box.padding = new RectOffset(10, 10, 10, 10);
            _title = new GUIStyle(GUI.skin.label);
            _title.fontStyle = FontStyle.Bold;
            _title.fontSize = 18;
        }

        void DrawSetup()
        {
            GUILayout.Label("전투 테스트", _title);

            // 실행
            GUILayout.BeginHorizontal();
            GUI.enabled = !_running;
            if (GUILayout.Button("전투 1회", GUILayout.Height(32f)))
                StartRun(1);
            if (GUILayout.Button($"{_repeatTarget}회 반복", GUILayout.Height(32f)))
                StartRun(_repeatTarget);
            GUI.enabled = true;
            if (GUILayout.Button(_running ? "정지" : "배치 보기", GUILayout.Height(32f)))
                BuildPreview();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("배속", GUILayout.Width(40f));
            for (int i = 0; i < Speeds.Length; i++)
            {
                bool on = GUILayout.Toggle(_speedIndex == i, $"x{Speeds[i]:0}", "Button", GUILayout.Width(48f));
                if (on && _speedIndex != i)
                {
                    _speedIndex = i;
                    if (_running)
                        Time.timeScale = Speeds[i];
                }
            }

            GUILayout.Label("반복", GUILayout.Width(36f));
            _repeatTarget = Mathf.Clamp(IntField("repeat", _repeatTarget, 50f), 1, 1000);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("시간 제한(초)", GUILayout.Width(96f));
            _timeLimit = Mathf.Max(5f, FloatField("timelimit", _timeLimit, 60f));
            GUILayout.Label("시드", GUILayout.Width(36f));
            _seed = IntField("seed", _seed, 70f);
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            // 몬스터
            var probe = BuildState();
            GUILayout.Label($"몬스터 (방어 전투력 {AssetCalculator.DefensePower(probe)})", _title);
            for (int i = 0; i < _defenders.Count; i++)
            {
                var m = _defenders[i];
                GUILayout.BeginHorizontal();
                var def = GameDefs.GetMonster(m.speciesId);
                if (GUILayout.Button(def != null ? def.DisplayName : m.speciesId, GUILayout.Width(80f)))
                    m.speciesId = Cycle(SpeciesIds, m.speciesId);
                var weapon = GameDefs.GetEquipment(m.weaponId);
                if (GUILayout.Button(weapon != null ? weapon.DisplayName : "맨손", GUILayout.Width(80f)))
                    m.weaponId = Cycle(WeaponIds, m.weaponId ?? string.Empty);
                GUILayout.Label($"유대 Lv.{m.bondLevel}", GUILayout.Width(70f));
                if (GUILayout.Button("-", GUILayout.Width(26f)))
                    m.bondLevel = Mathf.Max(1, m.bondLevel - 1);
                if (GUILayout.Button("+", GUILayout.Width(26f)))
                    m.bondLevel++;
                m.focusFire = GUILayout.Toggle(m.focusFire, "점사", GUILayout.Width(56f));
                if (GUILayout.Button("X", GUILayout.Width(28f)))
                {
                    _defenders.RemoveAt(i);
                    GUILayout.EndHorizontal();
                    break;
                }

                GUILayout.EndHorizontal();
            }

            if (_defenders.Count < MaxDefenders && GUILayout.Button("+ 몬스터 추가"))
                AddDefender(GameDefs.Goblin, string.Empty);

            GUILayout.Space(8f);

            // 용사
            var party = BuildParty();
            GUILayout.Label($"용사 (파티 가치 {party.totalValue})", _title);
            GUILayout.Label("강화 단계: HP / 공격 / 공속 / AI (0~4, AI 3·4는 현재 2와 동일하게 동작)");
            for (int i = 0; i < _heroes.Count; i++)
            {
                var h = _heroes[i];
                GUILayout.BeginHorizontal();
                var def = GameDefs.GetHero(h.jobId);
                if (GUILayout.Button(def != null ? def.DisplayName : h.jobId, GUILayout.Width(70f)))
                    h.jobId = Cycle(HeroJobIds, h.jobId);
                h.hpLevel = Stepper("HP", h.hpLevel, BalanceConfig.MaxHeroStatLevel);
                h.atkLevel = Stepper("공", h.atkLevel, BalanceConfig.MaxHeroStatLevel);
                h.asLevel = Stepper("속", h.asLevel, BalanceConfig.MaxHeroStatLevel);
                h.aiLevel = Stepper("AI", h.aiLevel, 4);
                if (GUILayout.Button("X", GUILayout.Width(28f)))
                {
                    _heroes.RemoveAt(i);
                    GUILayout.EndHorizontal();
                    break;
                }

                GUILayout.EndHorizontal();
            }

            if (_heroes.Count < MaxHeroes && GUILayout.Button("+ 용사 추가"))
                AddHero(GameDefs.Warrior);

            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"가시 바닥 {_spikeCount}개", GUILayout.Width(110f));
            if (GUILayout.Button("-", GUILayout.Width(26f)))
                _spikeCount = Mathf.Max(0, _spikeCount - 1);
            if (GUILayout.Button("+", GUILayout.Width(26f)))
                _spikeCount = Mathf.Min(6, _spikeCount + 1);
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);
            GUILayout.Label("※ 구성·수치는 다음 전투(또는 '배치 보기')부터 적용됩니다.");
        }

        int Stepper(string label, int value, int max)
        {
            GUILayout.Label($"{label}{value}", GUILayout.Width(44f));
            if (GUILayout.Button("-", GUILayout.Width(22f)))
                value = Mathf.Max(0, value - 1);
            if (GUILayout.Button("+", GUILayout.Width(22f)))
                value = Mathf.Min(max, value + 1);
            return value;
        }

        void DrawStatEditor()
        {
            GUILayout.Label("기본 스탯 (실시간 수정)", _title);

            GUILayout.Label("몬스터 종족: HP / 공격력 / 공격속도 / 이동속도 / 자산가치");
            for (int i = 0; i < SpeciesIds.Length; i++)
            {
                var def = GameDefs.GetMonster(SpeciesIds[i]);
                if (def == null)
                    continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label(def.DisplayName, GUILayout.Width(80f));
                def.MaxHealth = IntField("m_hp_" + def.Id, def.MaxHealth, 70f);
                def.AttackPower = IntField("m_atk_" + def.Id, def.AttackPower, 60f);
                def.AttackSpeed = FloatField("m_as_" + def.Id, def.AttackSpeed, 60f);
                def.MoveSpeed = FloatField("m_ms_" + def.Id, def.MoveSpeed, 60f);
                def.AssetValue = IntField("m_val_" + def.Id, def.AssetValue, 60f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8f);
            GUILayout.Label("용사 AI 0 기본: HP / 공격력 / 공격속도 / 이동속도 / 파티 가치");
            for (int i = 0; i < HeroJobIds.Length; i++)
            {
                var def = GameDefs.GetHero(HeroJobIds[i]);
                if (def == null)
                    continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label(def.DisplayName, GUILayout.Width(80f));
                def.MaxHealth = IntField("h_hp_" + def.Id, def.MaxHealth, 70f);
                def.AttackPower = IntField("h_atk_" + def.Id, def.AttackPower, 60f);
                def.AttackSpeed = FloatField("h_as_" + def.Id, def.AttackSpeed, 60f);
                def.MoveSpeed = FloatField("h_ms_" + def.Id, def.MoveSpeed, 60f);
                def.BaseValue = IntField("h_val_" + def.Id, def.BaseValue, 60f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8f);
            GUILayout.Label("용사 강화 누적치 (Lv.0~4, 쉼표로 구분)");
            ArrayField("HP", "up_hp", GameDefs.HeroHpBonus);
            ArrayField("공격력", "up_atk", GameDefs.HeroAtkBonus);
            ArrayField("공격속도", "up_as", GameDefs.HeroAsBonus);

            GUILayout.Space(8f);
            GUILayout.Label("유대감 레벨당 보너스 (0.05 = 5%)");
            GUILayout.BeginHorizontal();
            GUILayout.Label("HP", GUILayout.Width(40f));
            BalanceConfig.BondHpBonusPerLevel = FloatField("bond_hp", BalanceConfig.BondHpBonusPerLevel, 70f);
            GUILayout.Label("공격력", GUILayout.Width(56f));
            BalanceConfig.BondAtkBonusPerLevel = FloatField("bond_atk", BalanceConfig.BondAtkBonusPerLevel, 70f);
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("수치 복사 (클립보드)", GUILayout.Height(30f)))
            {
                GUIUtility.systemCopyBuffer = ExportText();
                _message = "현재 수치를 클립보드에 복사했습니다.";
            }

            if (GUILayout.Button("처음 값 보기", GUILayout.Height(30f)))
            {
                GUIUtility.systemCopyBuffer = _defaultsText;
                _message = "시작 시점 수치를 클립보드에 복사했습니다. (Play를 다시 시작하면 원래 값으로 돌아갑니다)";
            }

            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(_message))
                GUILayout.Label(_message);
        }

        void DrawResults()
        {
            int total = _wins + _losses + _draws;
            float winRate = total > 0 ? _wins * 100f / total : 0f;
            float avg = total > 0 ? _totalTime / total : 0f;
            GUILayout.BeginHorizontal();
            GUILayout.Label(
                $"결과 {total}판 · 몬스터 승 {_wins} / 용사 승 {_losses} / 무 {_draws} · 몬스터 승률 {winRate:0.0}% · 평균 {avg:0.0}초"
                + (_running ? $" · 진행 중 (남은 {_remaining}판)" : string.Empty),
                _title);
            if (GUILayout.Button("결과 초기화", GUILayout.Width(110f)))
                ClearResults();
            GUILayout.EndHorizontal();

            for (int i = 0; i < _results.Count; i++)
                GUILayout.Label(_results[i]);
        }

        // ───────────── 입력 칸 ─────────────

        int IntField(string key, int value, float width)
        {
            string shown = _buffers.TryGetValue(key, out var buffered) ? buffered : value.ToString(CultureInfo.InvariantCulture);
            string typed = GUILayout.TextField(shown, GUILayout.Width(width));
            _buffers[key] = typed;
            return int.TryParse(typed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : value;
        }

        float FloatField(string key, float value, float width)
        {
            string shown = _buffers.TryGetValue(key, out var buffered) ? buffered : value.ToString("0.###", CultureInfo.InvariantCulture);
            string typed = GUILayout.TextField(shown, GUILayout.Width(width));
            _buffers[key] = typed;
            return float.TryParse(typed, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : value;
        }

        void ArrayField(string label, string key, int[] values)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(80f));
            string shown = _buffers.TryGetValue(key, out var buffered) ? buffered : string.Join(",", values);
            string typed = GUILayout.TextField(shown, GUILayout.Width(300f));
            _buffers[key] = typed;
            var parts = typed.Split(',');
            if (parts.Length == values.Length)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (int.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                        values[i] = v;
                }
            }

            GUILayout.EndHorizontal();
        }

        void ArrayField(string label, string key, float[] values)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(80f));
            string shown;
            if (!_buffers.TryGetValue(key, out shown))
            {
                var items = new string[values.Length];
                for (int i = 0; i < values.Length; i++)
                    items[i] = values[i].ToString("0.###", CultureInfo.InvariantCulture);
                shown = string.Join(",", items);
            }

            string typed = GUILayout.TextField(shown, GUILayout.Width(300f));
            _buffers[key] = typed;
            var parts = typed.Split(',');
            if (parts.Length == values.Length)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (float.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                        values[i] = v;
                }
            }

            GUILayout.EndHorizontal();
        }

        static string Cycle(string[] options, string current)
        {
            int index = System.Array.IndexOf(options, current);
            return options[(index + 1) % options.Length];
        }

        // ───────────── 내보내기 ─────────────

        static string ExportText()
        {
            var ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.AppendLine("// Manager of Dungeon 전투 테스트 수치");
            sb.AppendLine("// 몬스터: HP / 공격력 / 공속 / 이동 / 자산가치");
            for (int i = 0; i < SpeciesIds.Length; i++)
            {
                var d = GameDefs.GetMonster(SpeciesIds[i]);
                if (d != null)
                    sb.AppendLine(string.Format(ci, "{0}: {1} / {2} / {3} / {4} / {5}", d.DisplayName, d.MaxHealth, d.AttackPower, d.AttackSpeed, d.MoveSpeed, d.AssetValue));
            }

            sb.AppendLine("// 용사 AI 0: HP / 공격력 / 공속 / 이동 / 파티 가치");
            for (int i = 0; i < HeroJobIds.Length; i++)
            {
                var d = GameDefs.GetHero(HeroJobIds[i]);
                if (d != null)
                    sb.AppendLine(string.Format(ci, "{0}: {1} / {2} / {3} / {4} / {5}", d.DisplayName, d.MaxHealth, d.AttackPower, d.AttackSpeed, d.MoveSpeed, d.BaseValue));
            }

            sb.AppendLine("// 용사 강화 누적치");
            sb.AppendLine("HP: " + string.Join(", ", GameDefs.HeroHpBonus));
            sb.AppendLine("공격력: " + string.Join(", ", GameDefs.HeroAtkBonus));
            var asItems = new string[GameDefs.HeroAsBonus.Length];
            for (int i = 0; i < asItems.Length; i++)
                asItems[i] = GameDefs.HeroAsBonus[i].ToString("0.###", ci);
            sb.AppendLine("공격속도: " + string.Join(", ", asItems));
            sb.AppendLine(string.Format(ci, "유대 보너스/레벨: HP {0} · 공격력 {1}", BalanceConfig.BondHpBonusPerLevel, BalanceConfig.BondAtkBonusPerLevel));
            return sb.ToString();
        }

        // ───────────── 카메라·배경 ─────────────

        static void EnsureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                go.transform.position = new Vector3(0f, 0f, -10f);
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
                cam.orthographicSize = 5.625f; // Dungeon 씬과 같은 크기
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.10f, 0.12f, 1f);
            cam.orthographic = true;
        }

        void EnsureMapArt()
        {
            var sprite = PackAssets.DungeonMap();
            var cam = Camera.main;
            if (sprite == null || cam == null)
                return;

            var go = new GameObject("DungeonMapArt");
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -20;
            PackAssets.ApplySpriteMaterial(renderer);

            float worldHeight = cam.orthographicSize * 2f;
            float worldWidth = worldHeight * cam.aspect;
            float spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
            float spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;
            if (spriteWidth < 0.01f || spriteHeight < 0.01f)
                return;
            var camPos = cam.transform.position;
            go.transform.position = new Vector3(camPos.x, camPos.y, 1f);
            go.transform.localScale = new Vector3(worldWidth / spriteWidth, worldHeight / spriteHeight, 1f);
        }
    }
}
