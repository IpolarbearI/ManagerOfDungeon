// ArenaTester.cs - reusable test-arena controller: spawn + cycle every character, trigger every animation.
// Hotkeys (legacy Input Manager AND new Input System):
//   A / D : previous / next character
//   1 Idle · 2 Walk · 3 Run · 4 Attack · 5 Hurt · 6 Die
//   Arrows: change facing   R: respawn
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// TestArena helper: cycle characters with A/D, trigger each animation with 1-6.
    /// </summary>
    public class ArenaTester : MonoBehaviour
    {
        public GameObject[] characterPrefabs;
        public Vector3 spawnPos = Vector3.zero;

        int _idx;
        GameObject _current;
        Animator _anim;
        Vector2 _facing = Vector2.down;
        string _state = "Idle";
        string _hud = "";

        void Start() { Spawn(); }

        void Spawn()
        {
            if (characterPrefabs == null || characterPrefabs.Length == 0) return;
            if (_current != null) Destroy(_current);
            _current = (GameObject)Instantiate(characterPrefabs[_idx], spawnPos, Quaternion.identity);
            _anim = _current.GetComponentInChildren<Animator>();
            foreach (var c in _current.GetComponents<MonoBehaviour>())
                if (c != null) c.enabled = false; // disable gameplay scripts for clean testing
            _facing = Vector2.down; _state = "Idle";
            ApplyAnim();
            BuildHud();
        }

        void Update()
        {
            if (characterPrefabs == null || characterPrefabs.Length == 0) return;
            if (KeyDown("D")) { _idx = (_idx + 1) % characterPrefabs.Length; Spawn(); }
            if (KeyDown("A")) { _idx = (_idx - 1 + characterPrefabs.Length) % characterPrefabs.Length; Spawn(); }
            if (KeyDown("R")) Spawn();
            if (_anim == null) return;

            if (KeyDown("1")) { _state = "Idle"; ApplyAnim(); BuildHud(); }
            if (KeyDown("2")) { _state = "Walk"; ApplyAnim(); BuildHud(); }
            if (KeyDown("3")) { _state = "Run"; ApplyAnim(); BuildHud(); }
            if (KeyDown("4")) { _state = "Attack"; SafeTrigger("Attack"); BuildHud(); }
            if (KeyDown("5")) { _state = "Hurt"; SafeTrigger("Hurt"); BuildHud(); }
            if (KeyDown("6")) { _state = "Die"; SafeTrigger("Die"); BuildHud(); }

            Vector2 nf = _facing;
            if (KeyDown("Down")) nf = Vector2.down;
            if (KeyDown("Up")) nf = Vector2.up;
            if (KeyDown("Left")) nf = Vector2.left;
            if (KeyDown("Right")) nf = Vector2.right;
            if (nf != _facing) { _facing = nf; ApplyAnim(); BuildHud(); }
        }

        void ApplyAnim()
        {
            if (_anim == null) return;
            SafeSetFloat("MoveX", _facing.x);
            SafeSetFloat("MoveY", _facing.y);
            SafeSetFloat("Speed", (_state == "Walk" || _state == "Run") ? 1f : 0f);
            SafeSetBool("IsRunning", _state == "Run");
        }

        void BuildHud()
        {
            string name = characterPrefabs[_idx] != null ? characterPrefabs[_idx].name : "-";
            _hud = "<b>OMNIKA - Test Arena</b>\nCharacter: <b>" + name + "</b>  (" + (_idx + 1) + "/" + characterPrefabs.Length +
                   ")\nState: " + _state + "   Facing: " + _facing +
                   "\n[A]/[D] char   [1-6] Idle/Walk/Run/Atk/Hurt/Die\n[Arrows] facing   [R] respawn";
        }

        bool Has(string p) { foreach (var pr in _anim.parameters) if (pr.name == p) return true; return false; }
        void SafeSetFloat(string p, float v) { if (Has(p)) _anim.SetFloat(p, v); }
        void SafeSetBool(string p, bool v) { if (Has(p)) _anim.SetBool(p, v); }
        void SafeTrigger(string p) { if (Has(p)) _anim.SetTrigger(p); }

        void OnGUI()
        {
            var st = new GUIStyle(GUI.skin.box) { fontSize = 15, alignment = TextAnchor.UpperLeft, richText = true };
            GUI.Box(new Rect(10, 10, 370, 125), _hud, st);
        }

        bool KeyDown(string k)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            if (kb == null) return false;
            switch (k)
            {
                case "A": return kb.aKey.wasPressedThisFrame;
                case "D": return kb.dKey.wasPressedThisFrame;
                case "R": return kb.rKey.wasPressedThisFrame;
                case "1": return kb.digit1Key.wasPressedThisFrame;
                case "2": return kb.digit2Key.wasPressedThisFrame;
                case "3": return kb.digit3Key.wasPressedThisFrame;
                case "4": return kb.digit4Key.wasPressedThisFrame;
                case "5": return kb.digit5Key.wasPressedThisFrame;
                case "6": return kb.digit6Key.wasPressedThisFrame;
                case "Down": return kb.downArrowKey.wasPressedThisFrame;
                case "Up": return kb.upArrowKey.wasPressedThisFrame;
                case "Left": return kb.leftArrowKey.wasPressedThisFrame;
                case "Right": return kb.rightArrowKey.wasPressedThisFrame;
            }
            return false;
#else
            switch (k)
            {
                case "A": return Input.GetKeyDown(KeyCode.A);
                case "D": return Input.GetKeyDown(KeyCode.D);
                case "R": return Input.GetKeyDown(KeyCode.R);
                case "1": return Input.GetKeyDown(KeyCode.Alpha1);
                case "2": return Input.GetKeyDown(KeyCode.Alpha2);
                case "3": return Input.GetKeyDown(KeyCode.Alpha3);
                case "4": return Input.GetKeyDown(KeyCode.Alpha4);
                case "5": return Input.GetKeyDown(KeyCode.Alpha5);
                case "6": return Input.GetKeyDown(KeyCode.Alpha6);
                case "Down": return Input.GetKeyDown(KeyCode.DownArrow);
                case "Up": return Input.GetKeyDown(KeyCode.UpArrow);
                case "Left": return Input.GetKeyDown(KeyCode.LeftArrow);
                case "Right": return Input.GetKeyDown(KeyCode.RightArrow);
            }
            return false;
#endif
        }
    }
}
