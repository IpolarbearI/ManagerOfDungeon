// PlayerDodge.cs - dodge-roll dash with i-frames (Q / right-mouse). The most-requested action-RPG move.
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    [RequireComponent(typeof(Rigidbody2D))]
    /// <summary>
    /// Dodge-roll dash with invulnerability frames on Q or right mouse. Reads facing from the animator, sets Health.invulnerable during the roll.
    /// </summary>
    public class PlayerDodge : MonoBehaviour
    {
        public float dodgeSpeed = 9f;
        public float dodgeTime = 0.18f;
        public float cooldown = 0.55f;

        Rigidbody2D _rb;
        Animator _anim;
        Health _health;
        float _timer;
        float _cd;
        Vector2 _dir;

        public bool IsDodging => _timer > 0f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _health = GetComponent<Health>();
        }

        void Update()
        {
            if (_cd > 0f) _cd -= Time.deltaTime;
            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f && _health != null) _health.invulnerable = false;
            }
            else if (_cd <= 0f && DodgePressed())
            {
                StartDodge();
            }
        }

        void StartDodge()
        {
            Vector2 f = _anim != null ? new Vector2(_anim.GetFloat("MoveX"), _anim.GetFloat("MoveY")) : Vector2.down;
            if (f.sqrMagnitude < 0.01f) f = Vector2.down;
            _dir = f.normalized;
            _timer = dodgeTime;
            _cd = cooldown;
            if (_health != null) _health.invulnerable = true;
            if (_anim != null) foreach (var p in _anim.parameters) if (p.name == "Roll") { _anim.SetTrigger("Roll"); break; }
        }

        void FixedUpdate()
        {
            if (_timer > 0f) _rb.linearVelocity = _dir * dodgeSpeed;
        }

        bool DodgePressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            bool q = kb != null && kb.qKey.wasPressedThisFrame;
            bool rmb = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
            return q || rmb;
#else
            return Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1);
#endif
        }
    }
}
