using System.Collections.Generic;
using UnityEngine;

namespace ManagerOfDungeon
{
    public sealed class CombatDirector : MonoBehaviour
    {
        readonly List<CombatUnit> _units = new List<CombatUnit>();
        readonly List<SpikeFloor> _spikes = new List<SpikeFloor>();

        Transform _unitRoot;
        Transform _trapRoot;
        SpriteRenderer _trapZone;
        Sprite _squareSprite;
        Vector2 _trapMin;
        Vector2 _trapMax;
        bool _inBattle;
        bool _battleEnded;

        public IReadOnlyList<CombatUnit> Units => _units;
        public IReadOnlyList<SpikeFloor> Spikes => _spikes;
        public bool InBattle => _inBattle;
        public bool BattleEnded => _battleEnded;
        public bool UseFocusFire { get; set; }
        public bool ShowTargetDebug { get; set; } = true;
        public CombatTeam LastWinner { get; private set; }

        public void BuildRoom()
        {
            EnsureRoots();
            ClearChildren(_unitRoot);
            ClearChildren(_trapRoot);
            _units.Clear();
            _spikes.Clear();
            _inBattle = false;
            _battleEnded = false;

            var cam = Camera.main;
            RefreshTrapZone(cam);
            Vector3 warriorHome = Viewport(cam, 0.21f, 0.56f);
            Vector3 archerHome = Viewport(cam, 0.21f, 0.36f);
            Vector3 goblinAHome = Viewport(cam, 0.78f, 0.56f);
            Vector3 goblinBHome = Viewport(cam, 0.80f, 0.38f);

            var warrior = CreateUnit("hero_warrior");
            warrior.SetupFromCharacter(
                "hero_warrior",
                new Warrior(),
                CombatTeam.Attacker,
                new Color(0.35f, 0.55f, 1f),
                warriorHome);

            var archer = CreateUnit("hero_archer");
            archer.SetupFromCharacter(
                "hero_archer",
                new Archer(),
                CombatTeam.Attacker,
                new Color(0.35f, 0.85f, 0.45f),
                archerHome);

            var goblinA = CreateUnit("goblin_a");
            goblinA.SetupFromMonster(
                "goblin_a",
                new Goblin("고블린 A"),
                CombatTeam.Defender,
                new Color(0.35f, 0.7f, 0.25f),
                goblinAHome);

            var goblinB = CreateUnit("goblin_b");
            goblinB.SetupFromMonster(
                "goblin_b",
                new Goblin("고블린 B"),
                CombatTeam.Defender,
                new Color(0.28f, 0.62f, 0.22f),
                goblinBHome);

            _units.Add(warrior);
            _units.Add(archer);
            _units.Add(goblinA);
            _units.Add(goblinB);
        }

        public void StartBattle(bool focusFire)
        {
            UseFocusFire = focusFire;
            ShowTargetDebug = true;
            _inBattle = true;
            _battleEnded = false;
            GameMusic.PlayBattle();
        }

        public void StopBattle()
        {
            _inBattle = false;
        }

        public bool ConsumeBattleEnd()
        {
            if (!_battleEnded)
                return false;

            _battleEnded = false;
            return true;
        }

        public void RestoreUnits()
        {
            StopBattle();
            _battleEnded = false;
            ClearProjectiles();
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null)
                    _units[i].RestoreForNextBattle();
            }
        }

        public CombatUnit FindUnit(string actorId)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null && _units[i].ActorId == actorId)
                    return _units[i];
            }

            return null;
        }

        public CombatUnit PickUnit(Vector3 worldPoint, float radius)
        {
            CombatUnit best = null;
            float bestDist = radius * radius;
            for (int i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                if (unit == null)
                    continue;

                float dist = (unit.transform.position - worldPoint).sqrMagnitude;
                if (dist <= bestDist)
                {
                    bestDist = dist;
                    best = unit;
                }
            }

            return best;
        }

        public bool HasBowEquipped()
        {
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null && _units[i].HasOldWoodBow())
                    return true;
            }

            return false;
        }

        public bool IsInTrapZone(Vector3 worldPoint)
        {
            return worldPoint.x >= _trapMin.x
                && worldPoint.x <= _trapMax.x
                && worldPoint.y >= _trapMin.y
                && worldPoint.y <= _trapMax.y;
        }

        public void SetTrapZoneHighlight(bool placing)
        {
            if (_trapZone == null)
                return;

            _trapZone.color = placing
                ? new Color(0.75f, 0.18f, 0.16f, 0.35f)
                : new Color(0.55f, 0.12f, 0.12f, 0.16f);
        }

        public bool TryPlaceSpike(Vector3 worldPoint)
        {
            if (!IsInTrapZone(worldPoint))
                return false;

            PlaceSpike(worldPoint);
            SetTrapZoneHighlight(false);
            return true;
        }

        public void ApplySession(DemoSession session)
        {
            if (session == null)
                return;

            if (!string.IsNullOrEmpty(session.BowActorId))
            {
                var unit = FindUnit(session.BowActorId);
                unit?.EquipMonsterWeapon(new OldWoodBow());
            }

            for (int i = 0; i < session.SpikePositions.Count; i++)
                PlaceSpike(session.SpikePositions[i]);
        }

        void PlaceSpike(Vector3 worldPoint)
        {
            EnsureRoots();
            var go = new GameObject(ResourceIds.ObjSpikeFloor);
            go.transform.SetParent(_trapRoot, false);
            var spike = go.AddComponent<SpikeFloor>();
            spike.Setup(worldPoint);
            _spikes.Add(spike);
        }

        public void SpawnProjectile(CombatUnit owner, CombatUnit target, float damage)
        {
            if (owner == null || target == null)
                return;

            var go = new GameObject(ResourceIds.ObjArrow);
            go.transform.SetParent(_unitRoot, false);
            go.transform.position = owner.transform.position;
            var bow = owner.HasOldWoodBow() ? PackAssets.LoadSprite(PackAssets.OldWoodBow) : null;
            var body = go.AddComponent<SpriteRenderer>();
            if (bow != null)
            {
                go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
                body.sprite = bow;
                body.color = Color.white;
            }
            else
            {
                go.transform.localScale = new Vector3(0.28f, 0.12f, 1f);
                body.sprite = SquareSprite();
                body.color = new Color(0.92f, 0.78f, 0.28f, 1f);
            }

            PackAssets.ApplySpriteMaterial(body);
            body.sortingOrder = 8;
            var projectile = go.AddComponent<Projectile>();
            projectile.Launch(this, owner, target, damage, CombatRanges.ProjectileSpeed);
        }

        public CombatUnit FindNearestEnemy(CombatUnit self)
        {
            CombatUnit nearest = null;
            float best = float.MaxValue;
            for (int i = 0; i < _units.Count; i++)
            {
                var other = _units[i];
                if (other == null || other == self || !other.IsAlive || other.Team == self.Team)
                    continue;

                float dist = (other.transform.position - self.transform.position).sqrMagnitude;
                if (dist < best)
                {
                    best = dist;
                    nearest = other;
                }
            }

            return nearest;
        }

        public CombatUnit FindRangedAllyTarget(CombatUnit self)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                var ally = _units[i];
                if (ally == null || ally == self || !ally.IsAlive || ally.Team != self.Team || !ally.IsRanged)
                    continue;
                if (ally.CurrentTarget != null && ally.CurrentTarget.IsAlive)
                    return ally.CurrentTarget;
            }

            return null;
        }

        void Update()
        {
            if (!Application.isPlaying || !_inBattle)
                return;

            float dt = Time.deltaTime;
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null)
                    _units[i].PrepareTarget(this, false);
            }

            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null)
                    _units[i].PrepareTarget(this, true);
            }

            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null)
                    _units[i].Tick(this, dt);
            }

            for (int i = 0; i < _spikes.Count; i++)
            {
                if (_spikes[i] != null)
                    _spikes[i].Tick(_units, dt);
            }

            if (TeamWiped(CombatTeam.Attacker))
            {
                LastWinner = CombatTeam.Defender;
                EndBattle();
            }
            else if (TeamWiped(CombatTeam.Defender))
            {
                LastWinner = CombatTeam.Attacker;
                EndBattle();
            }
        }

        void EndBattle()
        {
            _inBattle = false;
            _battleEnded = true;
            ClearProjectiles();
        }

        bool TeamWiped(CombatTeam team)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null && _units[i].Team == team && _units[i].IsAlive)
                    return false;
            }

            return true;
        }

        CombatUnit CreateUnit(string objectName)
        {
            EnsureRoots();
            var go = new GameObject(objectName);
            go.transform.SetParent(_unitRoot, false);
            return go.AddComponent<CombatUnit>();
        }

        void EnsureRoots()
        {
            if (_unitRoot == null)
            {
                var units = transform.Find("CombatUnits");
                if (units == null)
                {
                    var go = new GameObject("CombatUnits");
                    go.transform.SetParent(transform, false);
                    units = go.transform;
                }

                _unitRoot = units;
            }

            if (_trapRoot == null)
            {
                var traps = transform.Find("Traps");
                if (traps == null)
                {
                    var go = new GameObject("Traps");
                    go.transform.SetParent(transform, false);
                    traps = go.transform;
                }

                _trapRoot = traps;
            }
        }

        void ClearProjectiles()
        {
            if (_unitRoot == null)
                return;

            var projectiles = _unitRoot.GetComponentsInChildren<Projectile>(true);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i] != null)
                    Destroy(projectiles[i].gameObject);
            }
        }

        static void ClearChildren(Transform root)
        {
            if (root == null)
                return;

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        Sprite SquareSprite()
        {
            if (_squareSprite == null)
            {
                var tex = Texture2D.whiteTexture;
                _squareSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            }

            return _squareSprite;
        }

        void RefreshTrapZone(Camera cam)
        {
            EnsureRoots();
            Vector3 min = Viewport(cam, 0.40f, 0.32f);
            Vector3 max = Viewport(cam, 0.66f, 0.66f);
            _trapMin = new Vector2(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y));
            _trapMax = new Vector2(Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));

            if (_trapZone == null)
            {
                var go = new GameObject("TrapZone");
                go.transform.SetParent(transform, false);
                _trapZone = go.AddComponent<SpriteRenderer>();
                _trapZone.sprite = SquareSprite();
                _trapZone.sortingOrder = 1;
                PackAssets.ApplySpriteMaterial(_trapZone);
            }

            Vector2 size = _trapMax - _trapMin;
            _trapZone.transform.position = new Vector3((_trapMin.x + _trapMax.x) * 0.5f, (_trapMin.y + _trapMax.y) * 0.5f, 0f);
            _trapZone.transform.localScale = new Vector3(Mathf.Max(0.1f, size.x), Mathf.Max(0.1f, size.y), 1f);
            SetTrapZoneHighlight(false);
        }

        static Vector3 Viewport(Camera cam, float x, float y)
        {
            if (cam == null)
                return new Vector3((x - 0.5f) * 12f, (y - 0.5f) * 6f, 0f);

            var point = cam.ViewportToWorldPoint(new Vector3(x, y, 10f));
            point.z = 0f;
            return point;
        }
    }
}
