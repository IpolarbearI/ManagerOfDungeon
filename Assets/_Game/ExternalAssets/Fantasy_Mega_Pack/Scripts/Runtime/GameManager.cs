// GameManager.cs - tiny game-flow controller: victory when all quests done, game over on player death, R to restart.
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Minimal game-flow controller for the demo game: shows VICTORY when every quest
    /// objective is complete, GAME OVER when the player dies, and restarts the scene on R.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public string gameTitle = "DUNGEON RUN";

        bool _victory;
        bool _gameOver;
        float _titleTimer = 3f;

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var h = player.GetComponent<Health>();
                if (h != null) h.onDeath.AddListener(() => _gameOver = true);
            }
            if (QuestSystem.Instance != null)
                QuestSystem.Instance.onAllComplete += () => _victory = true;
        }

        void Update()
        {
            if (_titleTimer > 0f) _titleTimer -= Time.deltaTime;
            if ((_victory || _gameOver) && RestartPressed())
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void OnGUI()
        {
            if (_titleTimer > 0f)
            {
                var st = new GUIStyle(GUI.skin.label) { fontSize = 42, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                st.normal.textColor = new Color(1f, 0.92f, 0.6f);
                GUI.Label(new Rect(0, Screen.height * 0.18f, Screen.width, 60), gameTitle, st);
                var st2 = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(0, Screen.height * 0.18f + 60, Screen.width, 30), "WASD move · Space attack · Q dodge · E interact", st2);
            }
            if (_victory) Banner("VICTORY!", new Color(0.5f, 1f, 0.6f));
            else if (_gameOver) Banner("GAME OVER", new Color(1f, 0.5f, 0.45f));
        }

        void Banner(string text, Color color)
        {
            var dim = new Rect(0, Screen.height / 2 - 90, Screen.width, 180);
            GUI.Box(dim, "");
            var st = new GUIStyle(GUI.skin.label) { fontSize = 56, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            st.normal.textColor = color;
            GUI.Label(new Rect(0, Screen.height / 2 - 70, Screen.width, 80), text, st);
            var st2 = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, Screen.height / 2 + 14, Screen.width, 40), "Press R to restart", st2);
        }

        bool RestartPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }
    }
}
