// TopDownCharacterController.cs
// Demo-only sample controller for the Fantasy Dungeon Pixel Pack.
// Works with BOTH the legacy Input Manager and the new Input System package.
// Drop on a GameObject with: SpriteRenderer + Animator + Rigidbody2D (Dynamic, Gravity 0).
// Animator params: MoveX (float), MoveY (float), Speed (float), IsRunning (bool),
//   Attack (trigger), Hurt (trigger), Die (trigger).
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    /// <summary>
    /// 8-direction top-down movement (walk/run) driving the pack's 2D blend-tree animators. Put on the player prefab. WASD/arrows + Shift to run; supports both Input systems.
    /// </summary>
    public class TopDownCharacterController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Walk speed in units/second.")]
        public float walkSpeed = 3f;
        [Tooltip("Run speed multiplier while holding the run key (Left Shift).")]
        public float runMultiplier = 1.8f;

        Rigidbody2D _rb;
        Animator _anim;
        Vector2 _input;
        Vector2 _lastFacing = Vector2.down;
        bool _run;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        void Update()
        {
            ReadInput(out _input, out _run, out bool attack, out bool hurt, out bool die);
            if (_input.sqrMagnitude > 1f) _input.Normalize();

            float speed = _input.sqrMagnitude > 0.01f ? 1f : 0f;
            if (speed > 0f) _lastFacing = _input;

            _anim.SetFloat("MoveX", _lastFacing.x);
            _anim.SetFloat("MoveY", _lastFacing.y);
            _anim.SetFloat("Speed", speed);
            _anim.SetBool("IsRunning", _run && speed > 0f);

            if (attack) _anim.SetTrigger("Attack");
            if (hurt) _anim.SetTrigger("Hurt");
            if (die) _anim.SetTrigger("Die");
        }

        void FixedUpdate()
        {
            _rb.linearVelocity = _input * walkSpeed * (_run ? runMultiplier : 1f);
        }

        void ReadInput(out Vector2 mv, out bool run, out bool attack, out bool hurt, out bool die)
        {
            mv = Vector2.zero; run = false; attack = false; hurt = false; die = false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) mv.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) mv.x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) mv.y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) mv.y += 1f;
            run = kb.leftShiftKey.isPressed;
            attack = kb.spaceKey.wasPressedThisFrame;
            hurt = kb.hKey.wasPressedThisFrame;
            die = kb.kKey.wasPressedThisFrame;
#else
            mv.x = Input.GetAxisRaw("Horizontal");
            mv.y = Input.GetAxisRaw("Vertical");
            run = Input.GetKey(KeyCode.LeftShift);
            attack = Input.GetKeyDown(KeyCode.Space);
            hurt = Input.GetKeyDown(KeyCode.H);
            die = Input.GetKeyDown(KeyCode.K);
#endif
        }
    }
}
