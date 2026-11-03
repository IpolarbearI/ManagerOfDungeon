// PauseMenu.cs - ESC pause menu: Resume / Save / Quit to Title. Pauses time.
using UnityEngine;
using UnityEngine.UI;

namespace FantasyDungeonPixelPack
{
    /// <summary>ESC pause menu with Resume, Save Game and Quit to Title. Sets Time.timeScale.</summary>
    public class PauseMenu : MonoBehaviour
    {
        public string titleSceneName = "TitleScreen";
        public static bool IsPaused { get; private set; }

        GameObject _root;
        Text _saveFeedback;

        void Awake() { BuildUI(); _root.SetActive(false); }

        void Update()
        {
            if (GameInput.PausePressed() && !DialogueBoxUI.IsOpen && !BossIntro.IsPlaying)
            {
                if (IsPaused) Resume(); else Pause();
            }
        }

        void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            _saveFeedback.text = "";
            _root.SetActive(true);
            AudioManager.PlaySFX("ui_open");
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            _root.SetActive(false);
        }

        void SaveGame()
        {
            var save = FindObjectOfType<SaveSystem>();
            if (save != null) { save.Save(); _saveFeedback.text = "Game saved!"; }
            else _saveFeedback.text = "No save system found.";
        }

        void QuitToTitle()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            SceneFader.FadeToScene(titleSceneName);
        }

        void BuildUI()
        {
            var canvasGO = new GameObject("PauseCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 980;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            _root = new GameObject("Panel");
            _root.transform.SetParent(canvasGO.transform, false);
            var dim = _root.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, 0.7f);
            var rt = dim.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var box = new GameObject("Box").AddComponent<Image>();
            box.transform.SetParent(_root.transform, false);
            box.color = new Color(0.09f, 0.08f, 0.12f, 0.98f);
            var brt = box.rectTransform;
            brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(420, 420);

            var title = MakeText(box.transform, "Title", 40, FontStyle.Bold, new Color(1f, 0.85f, 0.4f), "PAUSED");
            SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(380, 60), new Vector2(0, -22));

            MakeButton(box.transform, "Resume", -130, Resume);
            MakeButton(box.transform, "Save Game", -210, SaveGame);
            MakeButton(box.transform, "Quit to Title", -290, QuitToTitle);

            _saveFeedback = MakeText(box.transform, "Feedback", 22, FontStyle.Italic, new Color(0.5f, 0.9f, 0.5f), "");
            SetRect(_saveFeedback.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(380, 34), new Vector2(0, 14));
        }

        void MakeButton(Transform parent, string label, float y, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.18f, 0.26f, 1f);
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(320, 62);
            rt.anchoredPosition = new Vector2(0, y);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(action);
            btn.onClick.AddListener(() => AudioManager.PlaySFX("ui_click"));
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.4f);
            btn.colors = colors;
            var t = MakeText(go.transform, "Label", 28, FontStyle.Bold, new Color(0.95f, 0.93f, 0.88f), label);
            SetRect(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        Text MakeText(Transform parent, string name, int size, FontStyle style, Color color, string content)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = GameFonts.UI;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.text = content;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            return t;
        }

        void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
        }
    }
}
