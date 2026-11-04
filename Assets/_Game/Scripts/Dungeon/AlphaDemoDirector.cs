using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ManagerOfDungeon
{
    public sealed class AlphaDemoDirector : MonoBehaviour
    {
        DemoSession _session;
        CombatDirector _combat;
        DungeonRoomScreen _hud;
        readonly IDialogueProvider _dialogue = new ScriptedDialogueProvider();
        CombatUnit _selected;
        bool _placingSpike;
        bool _dialogueOpen;
        int _dialogueIndex;
        string _status = string.Empty;

        public DemoSession Session => _session;
        public CombatDirector Combat => _combat;
        public CombatUnit Selected => _selected;
        public bool PlacingSpike => _placingSpike;
        public bool DialogueOpen => _dialogueOpen;
        public int DialogueIndex => _dialogueIndex;
        public IDialogueProvider Dialogue => _dialogue;
        public string Status => _status;

        public void Bind(DungeonRoomScreen hud, CombatDirector combat)
        {
            _hud = hud;
            _combat = combat;
            _session = DemoSession.Current;
            _selected = null;
            _placingSpike = false;
            _dialogueOpen = false;
            _dialogueIndex = 0;
            _status = StatusForPhase(_session.Phase);
            if (_combat != null)
            {
                _combat.BuildRoom();
                _combat.ApplySession(_session);
            }

            AdvanceIfReady();
            _hud?.RefreshDemoUi();
        }

        public void Select(CombatUnit unit)
        {
            if (_combat != null && _combat.InBattle)
                return;

            if (_selected != null)
                _selected.SetSelected(false);

            _selected = unit;
            if (_selected != null)
                _selected.SetSelected(true);

            if (_selected != null)
                _hud?.HideDesignMenus();

            if (_session.Phase == DemoPhase.Inspect)
                AdvanceIfReady();

            _hud?.RefreshDemoUi();
        }

        public bool TryEquipBow()
        {
            if (_combat != null && _combat.InBattle)
                return false;
            if (_selected == null || _selected.Team != CombatTeam.Defender)
            {
                _status = "고블린을 선택한 뒤 고목 활을 장착하세요.";
                _hud?.RefreshDemoUi();
                return false;
            }

            _selected.EquipMonsterWeapon(new OldWoodBow());
            _session.BowActorId = _selected.ActorId;
            _placingSpike = false;
            _status = "근거리 고블린이 원거리로 바뀌었습니다.";
            AdvanceIfReady();
            _hud?.RefreshDemoUi();
            return true;
        }

        public void BeginSpikePlacement()
        {
            if (_combat != null && _combat.InBattle)
                return;
            if (_session.Phase != DemoPhase.PlaceTrap && _session.Phase != DemoPhase.EquipBow)
            {
                if ((int)_session.Phase < (int)DemoPhase.PlaceTrap)
                {
                    _status = "먼저 고목 활을 장착하세요.";
                    _hud?.RefreshDemoUi();
                    return;
                }
            }

            _placingSpike = true;
            _combat?.SetTrapZoneHighlight(true);
            _status = "용사와 고블린 사이 빨간 구역에 가시 바닥을 설치하세요.";
            _hud?.RefreshDemoUi();
        }

        public void CancelSpikePlacement()
        {
            _placingSpike = false;
            _combat?.SetTrapZoneHighlight(false);
            _hud?.RefreshDemoUi();
        }

        public void SetStatus(string status)
        {
            _status = status ?? string.Empty;
            _hud?.RefreshDemoUi();
        }

        public bool TryStartBattle()
        {
            if (_combat == null || _combat.InBattle)
                return false;

            if (_session.Phase == DemoPhase.PlaceTrap || _session.Phase == DemoPhase.EquipBow)
            {
                if (!_combat.HasBowEquipped())
                {
                    _status = "고목 활을 장착해야 1차 전투를 시작할 수 있습니다.";
                    _hud?.RefreshDemoUi();
                    return false;
                }

                if (_combat.Spikes.Count < 1)
                {
                    _status = "가시 바닥을 하나 이상 설치하세요.";
                    _hud?.RefreshDemoUi();
                    return false;
                }

                _session.Phase = DemoPhase.Battle1;
            }
            else if (_session.Phase == DemoPhase.Strategy)
            {
                if (!_session.FocusFire)
                {
                    _status = "점사를 켠 뒤 2차 전투를 시작하세요.";
                    _hud?.RefreshDemoUi();
                    return false;
                }

                _session.Phase = DemoPhase.Battle2;
            }
            else
            {
                return false;
            }

            _placingSpike = false;
            _dialogueOpen = false;
            _status = _session.Phase == DemoPhase.Battle1
                ? "1차 전투: 기본 타겟팅입니다."
                : "2차 전투: 점사 타겟을 비교하세요.";
            _combat.RestoreUnits();
            _combat.StartBattle(_session.FocusFire);
            _hud?.RefreshDemoUi();
            return true;
        }

        public void OpenWorry()
        {
            if (!Application.isPlaying)
                return;

            if (_session.Phase != DemoPhase.Worry && !_session.WorryComplete)
            {
                _status = "아직 고민이 없습니다. 1차 전투 후에 고블린에게 고민이 생깁니다.";
                _hud?.RefreshDemoUi();
                return;
            }

            SceneManager.LoadScene("Monster");
        }

        public void SetFocusFire(bool enabled)
        {
            if (!_session.TrySetFocusFire(enabled))
                _status = _session.StrategyWarning;
            else
                _status = enabled ? "점사 ON. 근거리 고블린이 원거리 타겟을 따라갑니다." : "기본 공격 전략 ON.";

            AdvanceIfReady();
            _hud?.RefreshDemoUi();
        }

        public void SetDefaultStrategy(bool enabled)
        {
            if (!_session.TrySetDefaultStrategy(enabled))
                _status = _session.StrategyWarning;
            else
                _status = "기본 공격 전략 ON.";
            _hud?.RefreshDemoUi();
        }

        public string Hint()
        {
            switch (_session.Phase)
            {
                case DemoPhase.Inspect:
                    return "고블린을 터치하면 장비 창이 열립니다.";
                case DemoPhase.EquipBow:
                    return "고블린을 터치한 뒤 장비 창에서 고목 활을 장착하세요.";
                case DemoPhase.PlaceTrap:
                    return "설계 → 함정 → 가시 바닥으로 함정을 설치한 뒤 전투를 시작하세요.";
                case DemoPhase.Battle1:
                    return "자동전투를 관찰하세요. 용사를 직접 조작하지 않습니다.";
                case DemoPhase.Worry:
                    return $"왼쪽 위 고민을 눌러 {_session.WorryMonsterName}의 고민을 해결하세요.";
                case DemoPhase.Strategy:
                    return _session.FocusFire
                        ? "전투 시작으로 2차 전투를 보세요."
                        : "왼쪽 위 고민에서 점사를 해금하세요.";
                case DemoPhase.Battle2:
                    return "점사 ON 타겟 선을 1차와 비교하세요.";
                default:
                    return "시연 완료.";
            }
        }

        void Update()
        {
            if (!Application.isPlaying || _combat == null)
                return;

            if (_combat.ConsumeBattleEnd())
                HandleBattleEnd();

            if (_combat.InBattle || _dialogueOpen)
                return;

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            Vector3 screen = Mouse.current.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
            world.z = 0f;

            if (_placingSpike && ((int)_session.Phase >= (int)DemoPhase.PlaceTrap))
            {
                if (!_combat.TryPlaceSpike(world))
                {
                    _status = "함정은 용사와 고블린 사이 중앙에만 설치할 수 있습니다.";
                    _hud?.RefreshDemoUi();
                    return;
                }

                _session.SpikePositions.Add(world);
                _placingSpike = false;
                _status = "가시 바닥 설치. 전투 중 용사가 지나가며 밟습니다.";
                AdvanceIfReady();
                _hud?.RefreshDemoUi();
                return;
            }

            Select(_combat.PickUnit(world, 0.95f));
        }

        void HandleBattleEnd()
        {
            _combat.StopBattle();
            if (_session.Phase == DemoPhase.Battle1 && !_session.Battle1Finished)
            {
                _session.Battle1Finished = true;
                _combat.RestoreUnits();
                _session.AssignWorryTarget(_session.BowActorId);
                _session.Phase = DemoPhase.Worry;
                _status = $"{_session.WorryMonsterName}에게 고민이 생겼습니다.";
            }
            else if (_session.Phase == DemoPhase.Battle2 && !_session.Battle2Finished)
            {
                _session.Battle2Finished = true;
                _session.Phase = DemoPhase.Complete;
                _combat.RestoreUnits();
                _status = "시연이 끝났습니다. 메뉴로 돌아갑니다.";
                _hud?.RefreshDemoUi();
                StartCoroutine(ReturnToMenu());
                return;
            }

            _hud?.RefreshDemoUi();
        }

        IEnumerator ReturnToMenu()
        {
            yield return new WaitForSeconds(1.5f);
            DemoSession.ResetCurrent();
            SceneManager.LoadScene("Lobby");
        }

        void AdvanceIfReady()
        {
            if (_session.Phase == DemoPhase.Inspect && _selected != null)
                _session.Phase = DemoPhase.EquipBow;

            if (_session.Phase == DemoPhase.EquipBow && _combat != null && _combat.HasBowEquipped())
                _session.Phase = DemoPhase.PlaceTrap;

            if (_session.Phase == DemoPhase.Worry && _session.WorryComplete)
                _session.Phase = DemoPhase.Strategy;
        }

        string StatusForPhase(DemoPhase phase)
        {
            switch (phase)
            {
                case DemoPhase.Worry:
                    return $"{_session.WorryMonsterName}에게 고민이 있습니다.";
                case DemoPhase.Strategy:
                    return "점사 해금 완료. 전투 시작으로 2차 전투를 보세요.";
                case DemoPhase.Complete:
                    return "시연이 끝났습니다.";
                default:
                    return "유닛을 클릭해 고블린과 용사를 확인하세요.";
            }
        }
    }
}
