// TitleScreenUI.cs - title screen: game logo text, New Game / Quit. Optional background sprite.
using UnityEngine;
using UnityEngine.UI;

namespace FantasyDungeonPixelPack
{
    /// <summary>Title screen UI. Set gameSceneName + optional background sprite, drop into a scene.</summary>
    public class TitleScreenUI : MonoBehaviour
    {
        public string gameTitle = "DUNGEON OF THE\nSLIME KING";
        public string subtitle = "A Fantasy Dungeon adventure";
        public string gameSceneName = "SlimeKingdom";
        public Sprite background;

        void Awake() { BuildUI(); }

        void Start() { AudioManager.PlayBGM("title"); }

        void StartGame()
        {
            AudioManager.PlaySFX("ui_click");
            SceneFader.FadeToScene(gameSceneName, 0.9f);
        }

        void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        void BuildUI()
        {
            var canvasGO = new GameObject("TitleCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var bg = new GameObject("BG").AddComponent<Image>();
            bg.transform.SetParent(canvasGO.transform, false);
            if (background != null) { bg.sprite = background; bg.preserveAspect = false; bg.color = Color.white; }
            else bg.color = new Color(0.06f, 0.05f, 0.09f, 1f);
            var brt = bg.rectTransform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

            var dim = new GameObject("Dim").AddComponent<Image>();
            dim.transform.SetParent(canvasGO.transform, false);
            dim.color = new Color(0, 0, 0, background != null ? 0.45f : 0f);
            var drt = dim.rectTransform;
            drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one;
            drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;

            var title = MakeText(canvasGO.transform, "Title", 92, FontStyle.Bold, new Color(0.55f, 0.9f, 0.3f), gameTitle);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(1400, 240), Vector2.zero);
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.1f, 0.25f, 0.05f, 0.9f);
            shadow.effectDistance = new Vector2(6, -6);

            var sub = MakeText(canvasGO.transform, "Subtitle", 30, FontStyle.Italic, new Color(0.85f, 0.8f, 0.65f), subtitle);
            SetRect(sub.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(900, 50), Vector2.zero);

            MakeButton(canvasGO.transform, "New Game", new Vector2(0.5f, 0.36f), StartGame);
            MakeButton(canvasGO.transform, "Quit", new Vector2(0.5f, 0.25f), QuitGame);

            var credit = MakeText(canvasGO.transform, "Credit", 20, FontStyle.Normal, new Color(0.55f, 0.52f, 0.45f),
                "Built 100% with the free Fantasy Dungeon asset pack by Nika Studio");
            SetRect(credit.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1200, 30), new Vector2(0, 16));

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                GameInput.AddUIInputModule(es);
            }
        }

        void MakeButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.16f, 0.2f, 0.12f, 0.95f);
            var rt = img.rectTransform;
            rt.anchorMin = anchor; rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(380, 72);
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(0.55f, 0.9f, 0.3f, 0.8f);
            ol.effectDistance = new Vector2(2, -2);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(action);
            var t = MakeText(go.transform, "Label", 32, FontStyle.Bold, new Color(0.92f, 0.95f, 0.85f), label);
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
