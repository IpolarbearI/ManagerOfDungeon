using UnityEngine;

namespace ManagerOfDungeon
{
    public sealed class CombatUnit : MonoBehaviour
    {
        public string ActorId { get; private set; }
        public CombatTeam Team { get; private set; }
        public string UnitName { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float AttackPower { get; private set; }
        public float AttackSpeed { get; private set; }
        public float MoveSpeed { get; private set; }
        public float AttackRange { get; private set; }
        public bool IsRanged { get; private set; }
        public bool IsAlive => Health > 0.001f;
        public int DisplayHealth => Mathf.FloorToInt(Health);
        public int DisplayMaxHealth => Mathf.FloorToInt(MaxHealth);
        public CombatUnit CurrentTarget { get; private set; }
        public Vector3 HomePosition { get; private set; }

        Character _character;
        Monster _monster;
        SpriteRenderer _body;
        SpriteRenderer _selectRing;
        Transform _hpRoot;
        SpriteRenderer _hpBack;
        SpriteRenderer _hpFill;
        TextMesh _label;
        LineRenderer _aimLine;
        WarriorView _warriorView;
        GoblinView _goblinView;
        Color _bodyColor;
        float _attackCooldown;
        float _motionLock;
        float _heroRetarget;

        public void SetupFromCharacter(string actorId, Character character, CombatTeam team, Color color, Vector3 home)
        {
            _character = character;
            _monster = null;
            ActorId = actorId;
            Setup(
                character.Name,
                character.MaxHealth,
                character.AttackPower,
                character.AttackSpeed,
                character.MoveSpeed,
                character.AttackRange,
                character.IsRanged,
                team,
                color,
                character is Warrior,
                false,
                character is Archer,
                home);
        }

        public void SetupFromMonster(string actorId, Monster monster, CombatTeam team, Color color, Vector3 home)
        {
            _monster = monster;
            _character = null;
            ActorId = actorId;
            Setup(
                monster.Name,
                monster.MaxHealth,
                monster.AttackPower,
                monster.AttackSpeed,
                monster.MoveSpeed,
                monster.AttackRange,
                monster.IsRanged,
                team,
                color,
                false,
                true,
                false,
                home);
        }

        public void PrepareTarget(CombatDirector combat, bool applyFocus)
        {
            if (!IsAlive || combat == null)
                return;

            if (applyFocus && combat.UseFocusFire && !IsRanged && Team == CombatTeam.Defender)
            {
                var focus = combat.FindRangedAllyTarget(this);
                CurrentTarget = focus != null ? focus : combat.FindNearestEnemy(this);
            }
            else
            {
                CurrentTarget = ChooseTarget(combat);
            }

            UpdateAimLine(combat.ShowTargetDebug);
        }

        public void Tick(CombatDirector combat, float deltaTime)
        {
            if (!IsAlive || combat == null)
                return;

            _attackCooldown -= deltaTime;
            _motionLock -= deltaTime;
            _heroRetarget -= deltaTime;

            if (CurrentTarget == null)
            {
                SetMoving(false, Vector3.zero);
                return;
            }

            Vector3 toTarget = CurrentTarget.transform.position - transform.position;
            toTarget.z = 0f;
            float distance = toTarget.magnitude;
            Face(toTarget);

            if (distance > AttackRange)
            {
                if (_motionLock > 0f)
                {
                    SetMoving(false, toTarget);
                    return;
                }

                transform.position += toTarget.normalized * MoveSpeed * deltaTime;
                SetMoving(true, toTarget);
                return;
            }

            SetMoving(false, toTarget);
            if (_motionLock > 0f || _attackCooldown > 0f)
                return;

            float interval = AttackSpeed <= 0f ? 1f : 1f / AttackSpeed;
            _attackCooldown = interval;
            _motionLock = CombatRanges.AttackMotionLock;
            PlayAttack();

            if (IsRanged)
                combat.SpawnProjectile(this, CurrentTarget, AttackPower);
            else
                CurrentTarget.TakeHit(AttackPower);
        }

        public void TakeHit(float amount)
        {
            if (!IsAlive)
                return;

            amount = Snap(amount);
            if (amount <= 0f)
                return;

            Health = Snap(Mathf.Max(0f, Health - amount));
            RefreshLabel();
            if (!IsAlive)
            {
                PlayDeath();
                if (_aimLine != null)
                    _aimLine.enabled = false;
            }
            else
            {
                PlayHurt();
            }
        }

        public void RestoreForNextBattle()
        {
            if (_character != null)
            {
                _character.RestoreHealth();
                Health = _character.MaxHealth;
                MaxHealth = _character.MaxHealth;
            }
            else if (_monster != null)
            {
                _monster.RestoreHealth();
                Health = _monster.MaxHealth;
                MaxHealth = _monster.MaxHealth;
            }

            transform.position = HomePosition;
            _attackCooldown = 0f;
            _motionLock = 0f;
            _heroRetarget = 0f;
            CurrentTarget = null;
            if (_body != null && _goblinView == null && _warriorView == null)
                _body.color = _bodyColor;
            Vector3 homeFacing = Team == CombatTeam.Attacker ? Vector3.right : Vector3.left;
            if (_warriorView != null)
                _warriorView.ResetToIdle(true);
            else if (_goblinView != null)
                _goblinView.ResetToIdle(homeFacing);
            else
                SetMoving(false, homeFacing);
            RefreshCombatStats();
            RefreshLabel();
            UpdateAimLine(false);
        }

        public void EquipMonsterWeapon(Weapon weapon)
        {
            if (_monster == null)
                return;

            _monster.EquipWeapon(weapon);
            RefreshCombatStats();
            RefreshLabel();
        }

        public bool HasOldWoodBow()
        {
            return _monster != null && _monster.Weapon is OldWoodBow;
        }

        public void SetSelected(bool selected)
        {
            if (_selectRing != null)
                _selectRing.enabled = selected;
        }

        public void ShowIdleAim(bool show)
        {
            if (!show)
            {
                UpdateAimLine(false);
                return;
            }

            UpdateAimLine(CurrentTarget != null);
        }

        void Setup(
            string unitName,
            float maxHealth,
            float attackPower,
            float attackSpeed,
            float moveSpeed,
            float attackRange,
            bool isRanged,
            CombatTeam team,
            Color color,
            bool useHeroKnight,
            bool useGoblin,
            bool useArcher,
            Vector3 home)
        {
            UnitName = unitName;
            MaxHealth = maxHealth < 1f ? 1f : maxHealth;
            Health = MaxHealth;
            AttackPower = attackPower;
            AttackSpeed = attackSpeed < 0.05f ? 0.05f : attackSpeed;
            MoveSpeed = moveSpeed < 0f ? 0f : moveSpeed;
            AttackRange = attackRange < 0.1f ? CombatRanges.Melee : attackRange;
            IsRanged = isRanged;
            Team = team;
            HomePosition = home;
            transform.position = home;
            _bodyColor = color;

            _body = GetComponent<SpriteRenderer>();
            if (_body == null)
                _body = gameObject.AddComponent<SpriteRenderer>();

            if (useHeroKnight)
            {
                _body.enabled = false;
                transform.localScale = Vector3.one;
                _warriorView = WarriorView.Spawn(transform, transform.position);
                if (_warriorView != null)
                    _warriorView.transform.localPosition = Vector3.zero;
            }
            else if (useGoblin || useArcher)
            {
                transform.localScale = Vector3.one;
                _goblinView = useArcher
                    ? GoblinView.SpawnArcher(transform, transform.position)
                    : GoblinView.Spawn(transform, transform.position);
                if (_goblinView != null)
                {
                    _body.enabled = false;
                    _goblinView.transform.localPosition = Vector3.zero;
                }
                else
                {
                    _body.sprite = CreateSquareSprite();
                    _body.color = color;
                    _body.sortingOrder = 5;
                    transform.localScale = new Vector3(0.85f, 0.85f, 1f);
                }
            }
            else
            {
                _body.sprite = CreateSquareSprite();
                _body.color = color;
                _body.sortingOrder = 5;
                transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }

            EnsureSelectRing();
            EnsureAimLine();
            EnsureHealthBar();
            RefreshLabel();
        }

        void RefreshCombatStats()
        {
            if (_monster != null)
            {
                AttackPower = _monster.AttackPower;
                AttackRange = _monster.AttackRange;
                IsRanged = _monster.IsRanged;
                _goblinView?.ShowBow(IsRanged);
            }
            else if (_character != null)
            {
                AttackPower = _character.AttackPower;
                AttackRange = _character.AttackRange;
                IsRanged = _character.IsRanged;
            }
        }

        CombatUnit ChooseTarget(CombatDirector combat)
        {
            if (Team == CombatTeam.Attacker)
            {
                bool outOfRange = CurrentTarget == null
                    || !CurrentTarget.IsAlive
                    || Vector3.Distance(transform.position, CurrentTarget.transform.position) > AttackRange;
                if (outOfRange && _heroRetarget <= 0f)
                {
                    _heroRetarget = CombatRanges.HeroRetargetInterval;
                    return combat.FindNearestEnemy(this);
                }

                if (CurrentTarget != null && CurrentTarget.IsAlive)
                    return CurrentTarget;
                return combat.FindNearestEnemy(this);
            }

            return combat.FindNearestEnemy(this);
        }

        void EnsureSelectRing()
        {
            if (_selectRing != null)
                return;

            var go = new GameObject("Select");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
            _selectRing = go.AddComponent<SpriteRenderer>();
            _selectRing.sprite = CreateSquareSprite();
            _selectRing.color = new Color(1f, 0.92f, 0.2f, 0.35f);
            _selectRing.sortingOrder = 4;
            _selectRing.enabled = false;
        }

        void EnsureAimLine()
        {
            if (_aimLine != null)
                return;

            _aimLine = gameObject.AddComponent<LineRenderer>();
            _aimLine.positionCount = 2;
            _aimLine.widthMultiplier = 0.06f;
            _aimLine.numCapVertices = 2;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
                _aimLine.material = new Material(shader);
            _aimLine.startColor = Team == CombatTeam.Attacker
                ? new Color(0.3f, 0.55f, 1f, 0.85f)
                : new Color(0.95f, 0.35f, 0.2f, 0.85f);
            _aimLine.endColor = _aimLine.startColor;
            _aimLine.enabled = false;
            _aimLine.sortingOrder = 6;
        }

        void UpdateAimLine(bool show)
        {
            if (_aimLine == null)
                return;

            bool visible = show && IsAlive && CurrentTarget != null && CurrentTarget.IsAlive;
            _aimLine.enabled = visible;
            if (!visible)
                return;

            _aimLine.SetPosition(0, transform.position);
            _aimLine.SetPosition(1, CurrentTarget.transform.position);
        }

        void LateUpdate()
        {
            if (_hpRoot == null)
                return;

            float top = 1.7f;
            var sprite = VisualSprite();
            if (sprite != null && sprite.enabled && sprite.sprite != null)
                top = sprite.bounds.max.y - transform.position.y + 0.22f;

            _hpRoot.localPosition = new Vector3(0f, Mathf.Max(top, 1.2f), 0f);
        }

        SpriteRenderer VisualSprite()
        {
            if (_goblinView != null)
            {
                var pack = _goblinView.GetComponent<SpriteRenderer>();
                if (pack != null)
                    return pack;
            }

            if (_warriorView != null)
            {
                var warrior = _warriorView.GetComponent<SpriteRenderer>();
                if (warrior != null)
                    return warrior;
            }

            return _body != null && _body.enabled ? _body : null;
        }

        void EnsureHealthBar()
        {
            if (_hpFill != null)
                return;

            var root = new GameObject("HealthBar");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            _hpRoot = root.transform;

            _hpBack = CreateBarPiece(root.transform, "Back", new Vector3(0f, 0f, 0f), new Vector3(1.05f, 0.12f, 1f), new Color(0.1f, 0.1f, 0.1f, 0.92f), 30, new Vector2(0.5f, 0.5f));
            _hpFill = CreateBarPiece(root.transform, "Fill", new Vector3(-0.5f, 0f, 0f), new Vector3(1f, 0.08f, 1f), new Color(0.28f, 0.86f, 0.32f, 1f), 31, new Vector2(0f, 0.5f));

            var labelGo = new GameObject("Value");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            labelGo.transform.localScale = new Vector3(0.055f, 0.055f, 1f);
            _label = labelGo.AddComponent<TextMesh>();
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.fontSize = 42;
            _label.characterSize = 0.45f;
            _label.color = Color.white;
        }

        SpriteRenderer CreateBarPiece(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Vector3 scale,
            Color color,
            int order,
            Vector2 pivot)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateBarSprite(pivot);
            renderer.color = color;
            renderer.sortingOrder = order;
            PackAssets.ApplySpriteMaterial(renderer);
            renderer.color = color;
            return renderer;
        }

        void RefreshLabel()
        {
            if (_label != null)
                _label.text = $"{DisplayHealth}/{DisplayMaxHealth}";

            if (_hpFill == null)
                return;

            float ratio = MaxHealth <= 0f ? 0f : Mathf.Clamp01(Health / MaxHealth);
            _hpFill.transform.localScale = new Vector3(Mathf.Max(0.001f, ratio), 0.08f, 1f);
        }

        void Face(Vector3 direction)
        {
            _warriorView?.Face(direction.x);
            _goblinView?.Face(direction);
        }

        void SetMoving(bool moving, Vector3 direction)
        {
            _warriorView?.SetMoving(moving);
            _goblinView?.SetMoving(moving, direction);
        }

        void PlayAttack()
        {
            _warriorView?.PlayAttack();
            _goblinView?.PlayAttack();
        }

        void PlayHurt()
        {
            _warriorView?.PlayHurt();
            _goblinView?.PlayHurt();
        }

        void PlayDeath()
        {
            if (_warriorView != null)
                _warriorView.PlayDeath();
            else if (_goblinView != null)
                _goblinView.PlayDeath();
            else if (_body != null)
                _body.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        }

        static float Snap(float value)
        {
            return Mathf.Round(value * 100f) / 100f;
        }

        static Sprite CreateSquareSprite()
        {
            return CreateBarSprite(new Vector2(0.5f, 0.5f));
        }

        static Sprite CreateBarSprite(Vector2 pivot)
        {
            var tex = Texture2D.whiteTexture;
            return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), pivot, 1f);
        }
    }
}
