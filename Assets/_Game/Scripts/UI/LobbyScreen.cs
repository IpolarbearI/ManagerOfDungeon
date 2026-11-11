using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerOfDungeon
{
    [ExecuteAlways]
    public sealed class LobbyScreen : MonoBehaviour
    {
        const string CanvasName = "LobbyCanvas";
        const int UiVersion = 6;

        static readonly Color TextColor = new Color(0.90f, 0.84f, 0.68f, 1f);
        static readonly Color TextHover = new Color(1f, 0.96f, 0.82f, 1f);
        static readonly Color Background = Color.black;
        static readonly string[] MenuLabels = { "튜토리얼", "새 게임", "이어하기", "환경설정", "종료" };

        [SerializeField] int builtVersion;

        Font _font;
        bool _introDone;
        Coroutine _intro;

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall -= EnsureUi;
                UnityEditor.EditorApplication.delayCall += EnsureUi;
                return;
            }
#endif
            EnsureUi();
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= EnsureUi;
#endif
        }

        void EnsureUi()
        {
            if (this == null || !isActiveAndEnabled)
                return;

            _font = ResolveFont();
            EnsureCamera();
            EnsureEventSystem();

            if (builtVersion != UiVersion || transform.Find(CanvasName) == null)
                RebuildUi();

            ApplyFonts();
            ApplyLogo();
            BindButtons();
            if (Application.isPlaying)
            {
                GameMusic.PlayDungeon();
                StartIntro();
            }
            else
                ShowMenuImmediate();
        }

        void RebuildUi()
        {
            var existing = transform.Find(CanvasName);
            if (existing != null)
            {
                existing.name = CanvasName + "_Old";
                DestroyGo(existing.gameObject);
            }

            BuildUi();
            builtVersion = UiVersion;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        static void DestroyGo(GameObject go)
        {
            if (Application.isPlaying)
                Destroy(go);
            else
                DestroyImmediate(go);
        }

        void EnsureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Background;
            cam.orthographic = true;
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        void BuildUi()
        {
            var canvasGo = new GameObject(CanvasName);
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateFill(canvasGo.transform, Background);

            var splash = CreateFillObject(canvasGo.transform, "Splash", Color.black);
            splash.AddComponent<Button>().onClick.AddListener(SkipIntro);
            var logo = CreateFillObject(splash.transform, "Logo", Color.white);
            var logoRect = logo.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.sizeDelta = new Vector2(1400f, 900f);
            var logoImage = logo.GetComponent<Image>();
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;
            splash.AddComponent<CanvasGroup>();

            var menuRoot = new GameObject("MenuRoot");
            menuRoot.transform.SetParent(canvasGo.transform, false);
            Stretch(menuRoot.AddComponent<RectTransform>());
            menuRoot.AddComponent<CanvasGroup>();

            CreateLabel(menuRoot.transform, "던전 입구", 36, TextAnchor.UpperLeft, new Vector2(80f, -72f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            var center = new GameObject("Center");
            center.transform.SetParent(menuRoot.transform, false);
            var centerRect = center.AddComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerRect.pivot = new Vector2(0.5f, 0.5f);
            centerRect.anchoredPosition = Vector2.zero;
            centerRect.sizeDelta = new Vector2(640f, 560f);

            var layout = center.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 20f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateTitle(center.transform, "Manager of Dungeon");

            var menu = new GameObject("Menu");
            menu.transform.SetParent(center.transform, false);
            var menuRect = menu.AddComponent<RectTransform>();
            menuRect.sizeDelta = new Vector2(640f, 400f);

            var menuLayout = menu.AddComponent<VerticalLayoutGroup>();
            menuLayout.childAlignment = TextAnchor.UpperCenter;
            menuLayout.spacing = 12f;
            menuLayout.childControlWidth = true;
            menuLayout.childControlHeight = false;
            menuLayout.childForceExpandWidth = true;
            menuLayout.childForceExpandHeight = false;

            var menuSize = menu.AddComponent<LayoutElement>();
            menuSize.minHeight = 400f;
            menuSize.preferredHeight = 400f;

            for (int i = 0; i < MenuLabels.Length; i++)
                AddMenuButton(menu.transform, MenuLabels[i]);
        }

        void CreateTitle(Transform parent, string content)
        {
            var go = new GameObject(content);
            go.transform.SetParent(parent, false);

            var layout = go.AddComponent<LayoutElement>();
            layout.minHeight = 80f;
            layout.preferredHeight = 80f;

            var text = go.AddComponent<Text>();
            ConfigureText(text, content, 48, TextAnchor.MiddleCenter);
        }

        void AddMenuButton(Transform parent, string label)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);

            var layout = go.AddComponent<LayoutElement>();
            layout.minHeight = 56f;
            layout.preferredHeight = 56f;
            layout.minWidth = 320f;

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);

            var button = go.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<Text>();
            ConfigureText(text, label, 40, TextAnchor.MiddleCenter);
            var outline = textGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var hover = go.AddComponent<LobbyButtonHover>();
            hover.Bind(text, TextColor, TextHover);
        }

        void BindButtons()
        {
            // TODO: 기획서의 별도 튜토리얼 씬(고블린1 vs 전사1)이 생기기 전까지 A1 쇼케이스를 연결
            Bind("튜토리얼", StartShowcase);
            Bind("새 게임", StartNewGame);
            Bind("이어하기", ContinueGame);
            Bind("환경설정", LoadSetting);
            Bind("종료", Quit);
        }

        void Bind(string buttonName, UnityEngine.Events.UnityAction onClick)
        {
            var child = transform.Find($"{CanvasName}/MenuRoot/Center/Menu/{buttonName}");
            if (child == null)
                return;

            var button = child.GetComponent<Button>();
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }

        void ApplyFonts()
        {
            var texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                ConfigureText(texts[i], texts[i].text, texts[i].fontSize, texts[i].alignment);
                if (_font != null && !string.IsNullOrEmpty(texts[i].text))
                    _font.RequestCharactersInTexture(texts[i].text, Mathf.Max(16, texts[i].fontSize), FontStyle.Normal);
            }
        }

        void ConfigureText(Text text, string content, int fontSize, TextAnchor alignment)
        {
            text.font = _font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = TextColor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.supportRichText = true;
        }

        void ApplyLogo()
        {
            var logo = transform.Find($"{CanvasName}/Splash/Logo");
            if (logo == null)
                return;

            var image = logo.GetComponent<Image>();
            var sprite = PackAssets.LobbyLogo();
            if (image == null || sprite == null)
                return;

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
        }

        void StartIntro()
        {
            if (_intro != null)
                StopCoroutine(_intro);
            _introDone = false;
            SetGroup("Splash", 1f, true);
            SetGroup("MenuRoot", 0f, false);
            _intro = StartCoroutine(PlayIntro());
        }

        void ShowMenuImmediate()
        {
            _introDone = true;
            SetGroup("Splash", 0f, false);
            SetGroup("MenuRoot", 1f, true);
        }

        void SkipIntro()
        {
            if (!Application.isPlaying || _introDone)
                return;

            if (_intro != null)
                StopCoroutine(_intro);
            ShowMenuImmediate();
        }

        IEnumerator PlayIntro()
        {
            SetGroup("Splash", 1f, true);
            SetGroup("MenuRoot", 0f, false);
            yield return new WaitForSeconds(1.8f);

            float t = 0f;
            while (t < 0.45f && !_introDone)
            {
                t += Time.unscaledDeltaTime;
                SetGroup("Splash", 1f - Mathf.Clamp01(t / 0.45f), true);
                yield return null;
            }

            ShowMenuImmediate();
        }

        void SetGroup(string name, float alpha, bool visible)
        {
            var node = transform.Find($"{CanvasName}/{name}");
            if (node == null)
                return;

            var group = node.GetComponent<CanvasGroup>();
            if (group == null)
                group = node.gameObject.AddComponent<CanvasGroup>();

            group.alpha = alpha;
            group.interactable = visible && alpha > 0.9f;
            group.blocksRaycasts = visible && alpha > 0.05f;
        }

        void CreateFill(Transform parent, Color color)
        {
            CreateFillObject(parent, "Background", color).GetComponent<Image>().raycastTarget = false;
        }

        GameObject CreateFillObject(Transform parent, string objectName, Color color)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            Stretch(rect);
            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        void CreateLabel(
            Transform parent,
            string content,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchoredPosition,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var go = new GameObject(content);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = alignment == TextAnchor.UpperLeft ? new Vector2(0f, 1f) : new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(720f, 80f);

            var text = go.AddComponent<Text>();
            ConfigureText(text, content, fontSize, alignment);
        }

        static void StartShowcase()
        {
            GameSession.StartShowcase();
            LoadDungeon();
        }

        static void StartNewGame()
        {
            GameSession.StartNewGame(System.Environment.TickCount);
            LoadDungeon();
        }

        static void ContinueGame()
        {
            if (!GameSession.Continue())
            {
                Debug.Log("[Lobby] 저장 파일이 없습니다. 새 게임을 시작하세요.");
                return;
            }

            LoadDungeon();
        }

        static void LoadDungeon()
        {
            if (!Application.isPlaying)
                return;

            SceneManager.LoadScene("Dungeon");
        }

        static void LoadSetting()
        {
            if (!Application.isPlaying)
                return;

            SceneManager.LoadScene("Setting");
        }

        static void Quit()
        {
            if (!Application.isPlaying)
                return;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static Font ResolveFont()
        {
            var font = PackAssets.KoreanFont();
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (font != null)
            {
                font.RequestCharactersInTexture(
                    "던전 입구Manager of Dungeon튜토리얼새 게임이어하기환경설정종료abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
                    40,
                    FontStyle.Normal);
                font.RequestCharactersInTexture(
                    "던전 입구Manager of Dungeon튜토리얼새 게임이어하기환경설정종료",
                    48,
                    FontStyle.Normal);
            }

            return font;
        }
    }

    sealed class LobbyButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Text _text;
        [SerializeField] Color _normal;
        [SerializeField] Color _hover;

        public void Bind(Text text, Color normal, Color hover)
        {
            _text = text;
            _normal = normal;
            _hover = hover;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_text != null)
                _text.color = _hover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_text != null)
                _text.color = _normal;
        }
    }
}
