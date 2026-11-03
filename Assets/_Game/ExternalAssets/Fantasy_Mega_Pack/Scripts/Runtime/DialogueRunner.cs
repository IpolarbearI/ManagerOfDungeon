// DialogueRunner.cs - simple typewriter dialogue (no external UI dependency, OnGUI box). Advance with Space/E.
using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Typewriter dialogue box (OnGUI, zero UI dependencies). Call DialogueRunner.Instance.Begin(speaker, lines); advance with Space/E.
    /// </summary>
    public class DialogueRunner : MonoBehaviour
    {
        public static DialogueRunner Instance;

        [TextArea] public string[] sampleLines;
        public string speaker = "NPC";
        public float charsPerSecond = 30f;

        string[] _lines;
        int _line;
        float _revealed;
        bool _active;
        Action _onDone;

        void Awake() { Instance = this; }

        public void Begin(string who, string[] lines, Action onDone = null)
        {
            speaker = who; _lines = lines; _line = 0; _revealed = 0f; _active = lines != null && lines.Length > 0; _onDone = onDone;
        }

        public bool IsActive => _active;

        void Update()
        {
            if (!_active) return;
            _revealed += charsPerSecond * Time.deltaTime;
            if (AdvancePressed())
            {
                if (_revealed < _lines[_line].Length) { _revealed = _lines[_line].Length; }
                else
                {
                    _line++; _revealed = 0f;
                    if (_line >= _lines.Length) { _active = false; _onDone?.Invoke(); }
                }
            }
        }

        void OnGUI()
        {
            if (!_active) return;
            int w = Mathf.Min(640, Screen.width - 40);
            var r = new Rect((Screen.width - w) / 2, Screen.height - 150, w, 120);
            GUI.Box(r, "");
            var st = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, alignment = TextAnchor.UpperLeft, padding = new RectOffset(14, 14, 10, 10) };
            string line = _lines[_line];
            int n = Mathf.Clamp((int)_revealed, 0, line.Length);
            GUI.Label(r, "<b>" + speaker + "</b>\n" + line.Substring(0, n), st);
            GUI.Label(new Rect(r.xMax - 120, r.yMax - 26, 110, 20), "[Space] next");
        }

        bool AdvancePressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            return kb != null && (kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E);
#endif
        }
    }
}
