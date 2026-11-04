using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerOfDungeon
{
    [ExecuteAlways]
    public sealed class MonsterScreen : MonoBehaviour
    {
        const string CanvasName = "MonsterCanvas";
        const int UiVersion = 10;

        static readonly Color FieldColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        static readonly Color PanelColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        static readonly Color LineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        static readonly Color TextDark = new Color(0.15f, 0.15f, 0.15f, 1f);
        static readonly Color PortraitColor = new Color(0.52f, 0.56f, 0.48f, 1f);
        static readonly Color DeskColor = new Color(0.47f, 0.38f, 0.30f, 1f);
        static readonly Color BarColor = new Color(0.16f, 0.16f, 0.18f, 1f);

        [SerializeField] int builtVersion;

        Font _font;
        Sprite _squareSprite;
        Sprite _circleSprite;
        readonly IDialogueProvider _dialogue = new ScriptedDialogueProvider();
        int _dialogueIndex;
        bool _dialogueOpen;

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
            EnsureSprites();
            EnsureCamera();
            EnsureEventSystem();

            if (builtVersion != UiVersion || transform.Find(CanvasName) == null)
                RebuildUi();

            ApplyFonts();
            ApplyMonsterArt();
            BindButtons();
            if (Application.isPlaying)
            {
                GameMusic.Stop();
                BeginWorryIfNeeded();
            }
        }

        void RebuildUi()
        {
            var existing = transform.Find(CanvasName);
            if (existing != null)
            {
                existing.name = CanvasName + "_Old";
                if (Application.isPlaying)
                    Destroy(existing.gameObject);
                else
                    DestroyImmediate(existing.gameObject);
            }

            BuildUi();
            builtVersion = UiVersion;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        void BuildUi()
        {
            var goblin = new Goblin(DemoSession.Current.WorryMonsterName);

            var canvasGo = new GameObject(CanvasName);
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateFill(canvasGo.transform, "Field", FieldColor);
            CreatePortrait(canvasGo.transform);
            CreateDesk(canvasGo.transform);
            CreateTimeBar(canvasGo.transform);
            CreateCircleButton(canvasGo.transform, "Back", "<", new Vector2(48f, -48f), new Vector2(0f, 1f), true);
            CreateCircleButton(canvasGo.transform, "Extra", "", new Vector2(-48f, -48f), new Vector2(1f, 1f), false);
            CreateWorryFive(canvasGo.transform);
            CreateStatPanel(canvasGo.transform, goblin);
            CreateCornerButton(canvasGo.transform, "Box", new Vector2(-70f, 150f));
            CreateCornerButton(canvasGo.transform, "Gift", new Vector2(-70f, 70f));
            CreateDialoguePanel(canvasGo.transform);
            CreateUnlockButton(canvasGo.transform);
        }

        void CreateDesk(Transform parent)
        {
            var desk = CreateImage(parent, "Desk", Color.white, _squareSprite);
            var rect = desk.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(1920f, 520f);
            var image = desk.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        void CreateWorryFive(Transform parent)
        {
            var go = CreateImage(parent, "WorryFive", Color.white, _squareSprite);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(120f, -24f);
            rect.sizeDelta = new Vector2(320f, 56f);
            AddOutline(go);
            CreateLabel(go.transform, $"{DemoSession.Current.WorryMonsterName}의 고민", 22, TextDark);
            go.SetActive(false);
        }

        void CreateTimeBar(Transform parent)
        {
            var bar = CreateImage(parent, "TimeBar", BarColor, _squareSprite);
            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -18f);
            rect.sizeDelta = new Vector2(220f, 48f);
            CreateLabel(bar.transform, "06:00", 26, Color.white);
        }

        void CreateStatPanel(Transform parent, Monster monster)
        {
            var panel = CreateImage(parent, "StatPanel", PanelColor, _squareSprite);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(80f, -90f);
            rect.sizeDelta = new Vector2(520f, 620f);
            AddOutline(panel);

            CreateHeader(panel.transform, monster.Name);
            CreateRow(panel.transform, "종족", "고블린", -70f);
            CreateRow(panel.transform, "HP", monster.MaxHealth.ToString(), -130f);
            CreateRow(panel.transform, "공격력", monster.AttackPower.ToString(), -190f);
            CreateRow(panel.transform, "공격속도", monster.AttackSpeed.ToString("0.##"), -250f);
            CreateRow(panel.transform, "이동속도", monster.MoveSpeed.ToString("0.##"), -310f);
            CreateBond(panel.transform, -390f);
            CreateRow(panel.transform, "전략", "기본 전투", -500f);
            SetChildName(panel.transform, "전략", "StrategyValue", "기본 전투");
        }

        void CreateHeader(Transform parent, string title)
        {
            var header = CreateImage(parent, "Header", Color.clear, _squareSprite);
            var rect = header.GetComponent<RectTransform>();
            StretchTop(rect, 58f);
            CreateLabel(header.transform, title, 28, TextDark);

            var badge = CreateImage(header.transform, "Badge", Color.white, _squareSprite);
            var badgeRect = badge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 0.5f);
            badgeRect.anchorMax = new Vector2(1f, 0.5f);
            badgeRect.pivot = new Vector2(1f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(-16f, 0f);
            badgeRect.sizeDelta = new Vector2(90f, 28f);
            AddOutline(badge);
            CreateLabel(badge.transform, "Easy IV", 16, TextDark);
        }

        void CreateRow(Transform parent, string key, string value, float y)
        {
            var row = CreateImage(parent, key, Color.white, _squareSprite);
            var rect = row.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(-24f, 52f);
            AddOutline(row);

            CreateSideLabel(row.transform, key, TextAnchor.MiddleLeft, new Vector2(16f, 0f), new Vector2(0f, 0.5f));
            CreateSideLabel(row.transform, value, TextAnchor.MiddleRight, new Vector2(-16f, 0f), new Vector2(1f, 0.5f));
        }

        void CreateBond(Transform parent, float y)
        {
            var row = CreateImage(parent, "Bond", Color.white, _squareSprite);
            var rect = row.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(-24f, 84f);
            AddOutline(row);

            CreateSideLabel(row.transform, "유대감", TextAnchor.MiddleLeft, new Vector2(16f, 16f), new Vector2(0f, 0.5f));
            CreateSideLabel(row.transform, "Lv.1 EXP 80", TextAnchor.MiddleRight, new Vector2(-16f, 16f), new Vector2(1f, 0.5f), "BondValue");

            var track = CreateImage(row.transform, "Track", new Color(0.82f, 0.82f, 0.82f, 1f), _squareSprite);
            var trackRect = track.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0f);
            trackRect.pivot = new Vector2(0.5f, 0f);
            trackRect.anchoredPosition = new Vector2(0f, 12f);
            trackRect.sizeDelta = new Vector2(-32f, 14f);

            var fill = CreateImage(track.transform, "Fill", new Color(0.35f, 0.78f, 0.42f, 1f), _squareSprite);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.8f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        void CreatePortrait(Transform parent)
        {
            var body = CreateImage(parent, "Portrait", Color.white, _squareSprite);
            var bodyRect = body.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.5f, 0f);
            bodyRect.anchorMax = new Vector2(0.5f, 0f);
            bodyRect.pivot = new Vector2(0.5f, 0f);
            bodyRect.anchoredPosition = new Vector2(0f, 170f);
            bodyRect.sizeDelta = new Vector2(820f, 1020f);
            var image = body.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        void ApplyMonsterArt()
        {
            ApplyUiArt("Field", PackAssets.InteractionBackground(), false);
            ApplyUiArt("Portrait", PackAssets.GoblinFront(), true);
            ApplyUiArt("Desk", PackAssets.Desk(), true);
        }

        void ApplyUiArt(string path, Sprite sprite, bool preserveAspect)
        {
            if (sprite == null)
                return;

            var node = FindUi(path);
            if (node == null)
                return;

            var image = node.GetComponent<Image>();
            if (image == null)
                return;

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = preserveAspect;
            image.type = Image.Type.Simple;
        }

        void CreateDialoguePanel(Transform parent)
        {
            var go = CreateImage(parent, "Dialogue", new Color(0.97f, 0.97f, 0.94f, 0.98f), _squareSprite);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 36f);
            rect.sizeDelta = new Vector2(980f, 220f);
            AddOutline(go);
            CreateBarText(go.transform, "Speaker", "고블린", 22, new Vector2(24f, -24f));
            CreateBarText(go.transform, "Body", "", 22, new Vector2(24f, -70f));
            CreateActionButton(go.transform, "NextLine", "다음", new Vector2(-24f, 28f));
        }

        void CreateUnlockButton(Transform parent)
        {
            var go = CreateImage(parent, "UnlockFocus", Color.white, _squareSprite);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 48f);
            rect.sizeDelta = new Vector2(280f, 64f);
            AddOutline(go);
            CreateLabel(go.transform, "점사 해금", 26, TextDark);
            go.AddComponent<Button>();
        }

        void CreateActionButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
        {
            var go = CreateImage(parent, objectName, Color.white, _squareSprite);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(140f, 48f);
            AddOutline(go);
            CreateLabel(go.transform, label, 20, TextDark);
            go.AddComponent<Button>();
        }

        void CreateBarText(Transform parent, string objectName, string content, int fontSize, Vector2 anchoredPosition)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(-48f, objectName == "Body" ? 90f : 36f);
            var text = go.AddComponent<Text>();
            ConfigureText(text, content, fontSize, objectName == "Body" ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft, TextDark);
        }

        void CreateCircleButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 anchor, bool goBack)
        {
            var go = CreateImage(parent, objectName, Color.white, _circleSprite);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(56f, 56f);
            AddOutline(go);
            if (!string.IsNullOrEmpty(label))
                CreateLabel(go.transform, label, 24, TextDark);
            if (goBack)
                go.AddComponent<Button>();
        }

        void CreateCornerButton(Transform parent, string objectName, Vector2 anchoredPosition)
        {
            var go = CreateImage(parent, objectName, Color.white, _squareSprite);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(64f, 64f);
            AddOutline(go);
        }

        void BindButtons()
        {
            BindButton("Back", LoadDungeon);
            BindButton("Dialogue/NextLine", AdvanceDialogue);
            BindButton("UnlockFocus", UnlockFocus);
        }

        void BindButton(string path, UnityEngine.Events.UnityAction action)
        {
            var node = FindUi(path);
            if (node == null)
                return;

            var button = node.GetComponent<Button>();
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        void BeginWorryIfNeeded()
        {
            var session = DemoSession.Current;
            _dialogueOpen = session.Phase == DemoPhase.Worry && !session.WorryComplete;
            _dialogueIndex = 0;
            RefreshSessionUi();
        }

        void AdvanceDialogue()
        {
            if (!_dialogueOpen)
                return;

            var lines = _dialogue.GetLines(ResourceIds.WorryFive);
            if (_dialogueIndex < lines.Count - 1)
            {
                _dialogueIndex++;
                RefreshSessionUi();
                return;
            }

            _dialogueOpen = false;
            DemoSession.Current.GrantWorryReward();
            DemoSession.Current.Phase = DemoPhase.Strategy;
            RefreshSessionUi();
        }

        void UnlockFocus()
        {
            var session = DemoSession.Current;
            if (!session.WorryComplete)
                session.GrantWorryReward();
            if (session.Phase == DemoPhase.Worry)
                session.Phase = DemoPhase.Strategy;
            session.TrySetFocusFire(true);
            LoadDungeon();
        }

        void RefreshSessionUi()
        {
            var session = DemoSession.Current;
            SetText("StatPanel/Header", session.WorryMonsterName);
            SetText("WorryFive", $"{session.WorryMonsterName}의 고민");
            SetText("StatPanel/Bond/BondValue", $"Lv.{session.BondLevel} EXP {session.BondExp}");
            SetText("StatPanel/전략/StrategyValue", session.FocusFire ? "점사" : "기본 전투");

            var fill = FindUi("StatPanel/Bond/Track/Fill");
            if (fill != null)
            {
                var rect = fill.GetComponent<RectTransform>();
                float t = session.BondLevel >= 2 ? 1f : Mathf.Clamp01(session.BondExp / (float)DemoSession.BondLevel2Threshold);
                rect.anchorMax = new Vector2(t, 1f);
            }

            var worryFive = FindUi("WorryFive");
            if (worryFive != null)
                worryFive.gameObject.SetActive(session.Phase == DemoPhase.Worry || _dialogueOpen);

            var unlock = FindUi("UnlockFocus");
            if (unlock != null)
            {
                bool show = session.WorryComplete && !session.FocusFire && !_dialogueOpen;
                unlock.gameObject.SetActive(show);
                SetText("UnlockFocus/Label", "점사 해금");
                var unlockButton = unlock.GetComponent<Button>();
                if (unlockButton != null)
                    unlockButton.interactable = true;
            }

            var panel = FindUi("Dialogue");
            if (panel != null)
            {
                panel.gameObject.SetActive(_dialogueOpen);
                if (_dialogueOpen)
                {
                    var lines = _dialogue.GetLines(ResourceIds.WorryFive);
                    int index = Mathf.Clamp(_dialogueIndex, 0, lines.Count - 1);
                    SetText("Dialogue/Speaker", lines[index].Speaker);
                    SetText("Dialogue/Body", lines[index].Text);
                    SetText("Dialogue/NextLine/Label", index >= lines.Count - 1 ? "유대감 +20" : "다음");
                }
            }
        }

        Transform FindUi(string path)
        {
            var canvas = FindNamedChild(transform, CanvasName);
            if (canvas == null || string.IsNullOrEmpty(path))
                return canvas;

            var current = canvas;
            var parts = path.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                current = FindNamedChild(current, parts[i]);
                if (current == null)
                    return null;
            }

            return current;
        }

        static Transform FindNamedChild(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                    return child;
            }

            return null;
        }

        void SetText(string path, string value)
        {
            var node = FindUi(path);
            if (node == null)
                return;

            var text = node.GetComponent<Text>();
            if (text == null)
                text = node.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.font = _font;
                text.text = value;
                RequestGlyphs(value, text.fontSize);
            }
        }

        static void SetChildName(Transform parent, string rowName, string childName, string expectedText)
        {
            var row = parent.Find(rowName);
            if (row == null)
                return;

            var child = row.Find(expectedText);
            if (child != null)
                child.name = childName;
        }

        static void LoadDungeon()
        {
            if (!Application.isPlaying)
                return;

            SceneManager.LoadScene("Dungeon");
        }

        GameObject CreateImage(Transform parent, string objectName, Color color, Sprite sprite)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = Image.Type.Simple;
            return go;
        }

        void CreateFill(Transform parent, string objectName, Color color)
        {
            var go = CreateImage(parent, objectName, color, _squareSprite);
            Stretch(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().raycastTarget = false;
        }

        void CreateLabel(Transform parent, string content, int fontSize, Color color)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            Stretch(go.AddComponent<RectTransform>());
            var text = go.AddComponent<Text>();
            ConfigureText(text, content, fontSize, TextAnchor.MiddleCenter, color);
        }

        void CreateSideLabel(Transform parent, string content, TextAnchor alignment, Vector2 anchoredPosition, Vector2 anchor, string objectName = null)
        {
            var go = new GameObject(objectName ?? content);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = alignment == TextAnchor.MiddleLeft ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(240f, 40f);
            var text = go.AddComponent<Text>();
            ConfigureText(text, content, 22, alignment, TextDark);
        }

        void ConfigureText(Text text, string content, int fontSize, TextAnchor alignment, Color color)
        {
            text.font = _font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            RequestGlyphs(content, fontSize);
        }

        void AddOutline(GameObject go)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = LineColor;
            outline.effectDistance = new Vector2(2f, -2f);
        }

        void ApplyFonts()
        {
            var texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].font = _font;
                RequestGlyphs(texts[i].text, texts[i].fontSize);
            }
        }

        void RequestGlyphs(string content, int fontSize)
        {
            if (_font == null || string.IsNullOrEmpty(content))
                return;
            _font.RequestCharactersInTexture(content, Mathf.Max(16, fontSize), FontStyle.Normal);
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void StretchTop(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, height);
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
            cam.backgroundColor = FieldColor;
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

        void EnsureSprites()
        {
            if (_squareSprite == null)
                _squareSprite = CreateFilledSprite(32, false);
            if (_circleSprite == null)
                _circleSprite = CreateFilledSprite(64, true);
        }

        static Sprite CreateFilledSprite(int size, bool circle)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color32[size * size];
            float radius = size * 0.5f - 0.5f;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool filled = !circle || (x - center.x) * (x - center.x) + (y - center.y) * (y - center.y) <= radius * radius;
                    pixels[y * size + x] = filled ? Color.white : new Color32(0, 0, 0, 0);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.DontSave;
            var sprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        static Font ResolveFont()
        {
            var font = PackAssets.KoreanFont();
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                font.hideFlags = HideFlags.DontSave;
                font.RequestCharactersInTexture(
                    "고블린종족공격력속도이동유대감전략기본전투점사해금이유없이화가나요마왕갑자기무슨있었어머몰라그냥답답하고짜증그래도괜찮아같이있어줄게다음해결책상의고민ABCDEFGHIJKLMNOPQRSTUVWXYZDayEasyIVLvEXP HP<>0123456789.:+ ",
                    32,
                    FontStyle.Normal);
            }

            return font;
        }
    }
}
