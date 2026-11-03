// DialogueBoxUI.cs - Canvas story dialogue: bottom box, speaker name, portrait slot, typewriter, E/Space to advance.
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Story dialogue box (bottom of screen) with speaker name, optional portrait and typewriter text.
    /// Use: DialogueBoxUI.Show("Elder", lines, portraitSprite, onDone). Advance with E/Space/Click.
    /// For short ambient quips above heads use SpeechBubble instead.
    /// </summary>
    public class DialogueBoxUI : MonoBehaviour
    {
        public static DialogueBoxUI Instance { get; private set; }
        public static bool IsOpen => Instance != null && Instance._open;
        public float charsPerSecond = 45f;

        GameObject _root;
        Text _nameText, _bodyText, _hint;
        Image _portrait;
        string[] _lines;
        int _index;
        bool _open, _typing;
        System.Action _onDone;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            _root.SetActive(false);
        }

        public static void Show(string speaker, string[] lines, Sprite portrait = null, System.Action onDone = null)
        {
            if (Instance == null)
            {
                var go = new GameObject("DialogueBoxUI");
                go.AddComponent<DialogueBoxUI>();
            }
            Instance.Open(speaker, lines, portrait, onDone);
        }

        void Open(string speaker, string[] lines, Sprite portrait, System.Action onDone)
        {
            _lines = lines; _index = 0; _onDone = onDone;
            _nameText.text = speaker;
            _portrait.sprite = portrait;
            _portrait.transform.parent.gameObject.SetActive(portrait != null);
            _root.SetActive(true);
            _open = true;
            StopAllCoroutines();
            StartCoroutine(TypeLine());
        }

        void Update()
        {
            if (!_open) return;
            bool advance = GameInput.AdvancePressed();
            if (!advance) return;
            if (_typing)
            {
                StopAllCoroutines();
                _bodyText.text = _lines[_index];
                _typing = false;
                _hint.text = "[E] Next";
            }
            else
            {
                _index++;
                if (_index >= _lines.Length) Close();
                else StartCoroutine(TypeLine());
            }
        }

        void Close()
        {
            _open = false;
            _root.SetActive(false);
            var cb = _onDone; _onDone = null;
            cb?.Invoke();
        }

        IEnumerator TypeLine()
        {
            _typing = true;
            _hint.text = "[E] Skip";
            string line = _lines[_index];
            _bodyText.text = "";
            float t = 0f;
            int shown = 0;
            while (shown < line.Length)
            {
                t += Time.deltaTime * charsPerSecond;
                int n = Mathf.Min(line.Length, Mathf.FloorToInt(t));
                if (n != shown)
                {
                    shown = n;
                    _bodyText.text = line.Substring(0, shown);
                }
                yield return null;
            }
            _typing = false;
            _hint.text = _index < _lines.Length - 1 ? "[E] Next" : "[E] Close";
        }

        void BuildUI()
        {
            DontDestroyOnLoad(gameObject);
            var canvasGO = new GameObject("DialogueCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            _root = new GameObject("Box");
            _root.transform.SetParent(canvasGO.transform, false);
            var box = _root.AddComponent<Image>();
            box.color = new Color(0.07f, 0.06f, 0.1f, 0.92f);
            var rt = box.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(1240, 220);
            rt.anchoredPosition = new Vector2(0, 30);

            var border = new GameObject("Border").AddComponent<Image>();
            border.transform.SetParent(_root.transform, false);
            border.color = new Color(0.85f, 0.7f, 0.35f, 0.9f);
            var brt = border.rectTransform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = new Vector2(1, 0);
            brt.pivot = new Vector2(0.5f, 0);
            brt.sizeDelta = new Vector2(0, 4);
            brt.anchoredPosition = new Vector2(0, 220);

            // portrait frame (left)
            var pframe = new GameObject("PortraitFrame").AddComponent<Image>();
            pframe.transform.SetParent(_root.transform, false);
            pframe.color = new Color(0.16f, 0.14f, 0.2f, 1f);
            var pfrt = pframe.rectTransform;
            pfrt.anchorMin = new Vector2(0, 0.5f); pfrt.anchorMax = new Vector2(0, 0.5f);
            pfrt.sizeDelta = new Vector2(170, 170);
            pfrt.anchoredPosition = new Vector2(105, 0);
            var pgo = new GameObject("Portrait").AddComponent<Image>();
            pgo.transform.SetParent(pframe.transform, false);
            pgo.preserveAspect = true;
            var prt = pgo.rectTransform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(8, 8); prt.offsetMax = new Vector2(-8, -8);
            _portrait = pgo;

            _nameText = MakeText("Name", 30, FontStyle.Bold, new Color(1f, 0.85f, 0.4f));
            var nrt = _nameText.rectTransform;
            nrt.anchorMin = new Vector2(0, 1); nrt.anchorMax = new Vector2(0, 1);
            nrt.pivot = new Vector2(0, 1);
            nrt.sizeDelta = new Vector2(500, 40);
            nrt.anchoredPosition = new Vector2(215, -16);

            _bodyText = MakeText("Body", 28, FontStyle.Normal, new Color(0.95f, 0.94f, 0.9f));
            _bodyText.alignment = TextAnchor.UpperLeft;
            var bdrt = _bodyText.rectTransform;
            bdrt.anchorMin = new Vector2(0, 0); bdrt.anchorMax = new Vector2(1, 1);
            bdrt.offsetMin = new Vector2(215, 18);
            bdrt.offsetMax = new Vector2(-30, -62);

            _hint = MakeText("Hint", 20, FontStyle.Italic, new Color(0.7f, 0.68f, 0.6f));
            _hint.alignment = TextAnchor.LowerRight;
            var hrt = _hint.rectTransform;
            hrt.anchorMin = new Vector2(1, 0); hrt.anchorMax = new Vector2(1, 0);
            hrt.pivot = new Vector2(1, 0);
            hrt.sizeDelta = new Vector2(200, 30);
            hrt.anchoredPosition = new Vector2(-18, 10);
        }

        Text MakeText(string name, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            var t = go.AddComponent<Text>();
            t.font = GameFonts.UI;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            return t;
        }
    }
}
