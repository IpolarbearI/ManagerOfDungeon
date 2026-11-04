using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ManagerOfDungeon
{
    [ExecuteAlways]
    public sealed class DungeonRoomScreen : MonoBehaviour
    {
        const string CanvasName = "DungeonCanvas";
        const int UiVersion = 17;
        const float Frame61Width = 2191f;
        const float Frame61Height = 124f;

        static readonly Color FieldColor = new Color(0.73f, 0.73f, 0.73f, 1f);
        static readonly Color BarColor = new Color(0.16f, 0.16f, 0.18f, 1f);
        static readonly Color LineColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        static readonly Color FillColor = new Color(0.93f, 0.93f, 0.93f, 1f);
        static readonly Color TextDark = new Color(0.15f, 0.15f, 0.15f, 1f);
        static readonly Color ChipColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        static readonly Color ToolColor = new Color(0.27f, 0.27f, 0.27f, 1f);
        static readonly Color ToolIcon = new Color(0.92f, 0.92f, 0.92f, 1f);

        [SerializeField] int builtVersion;

        Font _font;
        Sprite _squareSprite;
        Sprite _circleSprite;
        bool _demoBound;
        bool _designMenuOpen;
        bool _trapPaletteOpen;
        bool _storageOpen;
        bool _logOpen;
        AlphaDemoDirector _director;

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
            ApplyUiFrames();
            if (Application.isPlaying)
            {
                var leftover = transform.Find("WarriorVisuals");
                if (leftover != null)
                    Destroy(leftover.gameObject);
            }
            else
            {
                SpawnWarriorVisuals();
            }
        }

        void Start()
        {
            if (Application.isPlaying)
                BindDemo();
        }

        void RebuildUi()
        {
            _storageOpen = false;
            _logOpen = false;
            _designMenuOpen = false;
            _trapPaletteOpen = false;

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
            var canvasGo = new GameObject(CanvasName);
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateTopBar(canvasGo.transform);
            CreateToken(canvasGo.transform, "Entrance", "입구", false, new Vector2(110f, 260f), new Vector2(90f, 0f), new Vector2(0f, 0.5f));
            CreateToken(canvasGo.transform, "Core", "코어", true, new Vector2(220f, 220f), new Vector2(-90f, 0f), new Vector2(1f, 0.5f));
            CreateRoundButton(canvasGo.transform, "PrevRoom", "<", new Vector2(90f, -80f), new Vector2(0f, 0.5f));
            CreateRoundButton(canvasGo.transform, "NextRoom", ">", new Vector2(-90f, -80f), new Vector2(1f, 0.5f));
            CreateDesignDock(canvasGo.transform);
            CreateStoragePanel(canvasGo.transform);
            CreateLogPanel(canvasGo.transform);
            CreateUnitCard(canvasGo.transform);
            CreateBottomBar(canvasGo.transform);
        }

        void CreateTopBar(Transform parent)
        {
            var bar = CreatePanel(parent, "TopBar", BarColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            var rect = bar.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0f, -88f);
            rect.offsetMax = Vector2.zero;

            CreateBarChip(bar.transform, "Alert", "", new Vector2(190f, 0f), new Vector2(0f, 0.5f));
            CreateBarChip(bar.transform, "Report", "", new Vector2(280f, 0f), new Vector2(0f, 0.5f));
            CreateBarChip(bar.transform, "Menu", "", new Vector2(-24f, 0f), new Vector2(1f, 0.5f));
        }

        void CreateDesignDock(Transform parent)
        {
            var dock = new GameObject("DesignDock");
            dock.transform.SetParent(parent, false);
            var dockRect = dock.AddComponent<RectTransform>();
            dockRect.anchorMin = new Vector2(0f, 0f);
            dockRect.anchorMax = new Vector2(0f, 0f);
            dockRect.pivot = new Vector2(0f, 0f);
            dockRect.anchoredPosition = new Vector2(24f, 136f);
            dockRect.sizeDelta = new Vector2(760f, 140f);

            CreateToolButton(dock.transform, "Design", "설계", "▦", 0f);
            var menu = new GameObject("DesignMenu");
            menu.transform.SetParent(dock.transform, false);
            Stretch(menu.AddComponent<RectTransform>());
            CreateToolButton(menu.transform, "Trap", "함정", "▲", 92f);
            CreateToolButton(menu.transform, "Terrain", "지형", "≡", 184f);
            CreateToolButton(menu.transform, "Storage", "보관함", "▣", 276f);
            CreateToolButton(menu.transform, "Core", "코어 강화", "◆", 368f);
            CreateToolButton(menu.transform, "Log", "기록", "☰", 460f);
            CreateToolButton(menu.transform, "Back", "이전", "◀", 552f);
            menu.SetActive(false);

            var palette = new GameObject("TrapPalette");
            palette.transform.SetParent(dock.transform, false);
            var paletteRect = palette.AddComponent<RectTransform>();
            paletteRect.anchorMin = new Vector2(0f, 0f);
            paletteRect.anchorMax = new Vector2(0f, 0f);
            paletteRect.pivot = new Vector2(0f, 0f);
            paletteRect.anchoredPosition = new Vector2(96f, 118f);
            paletteRect.sizeDelta = new Vector2(120f, 120f);
            CreateToolButton(palette.transform, "Spike", "가시 바닥", "※", 0f);
            palette.SetActive(false);
        }

        void CreateStoragePanel(Transform parent)
        {
            var root = CreatePanel(parent, "StoragePanel", Color.clear, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().raycastTarget = false;

            var dim = CreatePanel(root.transform, "Dim", new Color(0f, 0f, 0f, 0.45f), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Stretch(dim.GetComponent<RectTransform>());
            dim.AddComponent<Button>();

            var window = CreatePanel(root.transform, "Window", Color.clear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.sizeDelta = new Vector2(1180f, 590f);
            window.GetComponent<Image>().raycastTarget = false;

            CreateUiArt(window.transform, "Title", new Vector2(0f, 1f), new Vector2(28f, -18f), new Vector2(116f, 56f));
            CreateUiArt(window.transform, "Chest", new Vector2(0f, 1f), new Vector2(160f, -14f), new Vector2(64f, 64f));
            CreateUiArt(window.transform, "Tabs", new Vector2(0f, 1f), new Vector2(430f, -22f), new Vector2(340f, 48f));
            var close = CreateUiArt(window.transform, "Close", new Vector2(1f, 1f), new Vector2(-12f, -10f), new Vector2(46f, 44f));
            close.GetComponent<Image>().raycastTarget = true;
            close.AddComponent<Button>();
            CreateUiArt(window.transform, "Grid", new Vector2(0f, 1f), new Vector2(24f, -86f), new Vector2(659f, 475f));
            CreateUiArt(window.transform, "Scroll", new Vector2(0f, 1f), new Vector2(700f, -110f), new Vector2(48f, 424f));
            CreateUiArt(window.transform, "Thumb", new Vector2(0f, 1f), new Vector2(706f, -130f), new Vector2(37f, 76f));
            CreateUiArt(window.transform, "Detail", new Vector2(1f, 1f), new Vector2(-24f, -86f), new Vector2(372f, 439f));

            var manage = CreatePanel(window.transform, "Manage", new Color(1f, 1f, 1f, 0.01f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            var manageRect = manage.GetComponent<RectTransform>();
            manageRect.anchoredPosition = new Vector2(-70f, -470f);
            manageRect.sizeDelta = new Vector2(280f, 56f);
            manage.AddComponent<Button>();

            root.SetActive(false);
        }

        void CreateLogPanel(Transform parent)
        {
            var root = CreatePanel(parent, "LogPanel", Color.clear, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().raycastTarget = false;

            var dim = CreatePanel(root.transform, "Dim", new Color(0f, 0f, 0f, 0.45f), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Stretch(dim.GetComponent<RectTransform>());
            dim.AddComponent<Button>();

            var window = CreatePanel(root.transform, "Window", Color.clear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.sizeDelta = new Vector2(1740f, 620f);
            window.GetComponent<Image>().raycastTarget = false;

            CreateUiArt(window.transform, "Title", new Vector2(0f, 1f), new Vector2(28f, -16f), new Vector2(172f, 56f));
            CreateUiArt(window.transform, "Icon", new Vector2(0f, 1f), new Vector2(214f, -18f), new Vector2(50f, 50f));
            var close = CreateUiArt(window.transform, "Close", new Vector2(1f, 1f), new Vector2(-12f, -10f), new Vector2(46f, 44f));
            close.GetComponent<Image>().raycastTarget = true;
            close.AddComponent<Button>();
            CreateUiArt(window.transform, "Table", new Vector2(0.5f, 1f), new Vector2(-28f, -82f), new Vector2(1604f, 502f));
            CreateUiArt(window.transform, "Scroll", new Vector2(1f, 1f), new Vector2(-18f, -110f), new Vector2(48f, 424f));
            CreateUiArt(window.transform, "Thumb", new Vector2(1f, 1f), new Vector2(-24f, -130f), new Vector2(37f, 76f));

            root.SetActive(false);
        }

        GameObject CreateUiArt(Transform parent, string objectName, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            var go = CreatePanel(parent, objectName, Color.white, anchor, anchor, anchor);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            return go;
        }

        void CreateToolButton(Transform parent, string objectName, string label, string icon, float x)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            var wrap = go.AddComponent<RectTransform>();
            wrap.anchorMin = new Vector2(0f, 0f);
            wrap.anchorMax = new Vector2(0f, 0f);
            wrap.pivot = new Vector2(0f, 0f);
            wrap.anchoredPosition = new Vector2(x, 0f);
            wrap.sizeDelta = new Vector2(92f, 132f);

            var iconGo = CreatePanel(go.transform, "Icon", Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(88f, 100f);
            iconGo.AddComponent<Button>();
            CreateCenteredText(iconGo.transform, icon, 28, ToolIcon);

            var caption = new GameObject("Caption");
            caption.transform.SetParent(go.transform, false);
            var captionRect = caption.AddComponent<RectTransform>();
            captionRect.anchorMin = new Vector2(0.5f, 0f);
            captionRect.anchorMax = new Vector2(0.5f, 0f);
            captionRect.pivot = new Vector2(0.5f, 0f);
            captionRect.anchoredPosition = Vector2.zero;
            captionRect.sizeDelta = new Vector2(96f, 40f);
            var text = caption.AddComponent<Text>();
            ConfigureText(text, label, 16, TextAnchor.UpperCenter, TextDark);
        }

        void CreateUnitCard(Transform parent)
        {
            var go = CreatePanel(parent, "UnitCard", Color.white, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, 132f);
            rect.sizeDelta = new Vector2(760f, 168f);
            AddOutline(go, 2f);

            CreateBarText(go.transform, "Title", "고블린 A - 고블린", 22, TextAnchor.MiddleLeft, new Vector2(20f, 54f), new Vector2(0f, 0.5f), new Vector2(360f, 36f));

            var hp = CreatePanel(go.transform, "HpTrack", new Color(0.82f, 0.82f, 0.82f, 1f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
            var hpRect = hp.GetComponent<RectTransform>();
            hpRect.anchoredPosition = new Vector2(-20f, 54f);
            hpRect.sizeDelta = new Vector2(280f, 22f);

            var fill = CreatePanel(hp.transform, "Fill", new Color(0.45f, 0.45f, 0.45f, 1f), Vector2.zero, Vector2.one, new Vector2(0f, 0.5f));
            var fillImage = fill.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;
            Stretch(fill.GetComponent<RectTransform>());

            CreateBarText(hp.transform, "HpValue", "NNN / NNN", 16, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(280f, 22f));

            CreateActionButton(go.transform, "EquipToggle", "장비 착용 전환", new Vector2(20f, -18f), new Vector2(0f, 0.5f), new Vector2(200f, 44f));
            CreateEquipSlot(go.transform);
            CreateRoundButton(go.transform, "CardClose", "X", new Vector2(-24f, -18f), new Vector2(1f, 0.5f));
            CreateRoundButton(go.transform, "CardBond", "♥", new Vector2(-88f, -18f), new Vector2(1f, 0.5f));
            AddButtonIfMissing(go.transform.Find("CardClose"));
            AddButtonIfMissing(go.transform.Find("CardBond"));
            CreateBarText(go.transform, "Action", "행동 : 기본 공격 중", 20, TextAnchor.MiddleLeft, new Vector2(20f, -62f), new Vector2(0f, 0.5f), new Vector2(500f, 32f));
            go.SetActive(false);
        }

        void CreateEquipSlot(Transform parent)
        {
            var go = CreatePanel(parent, "EquipSlot", FillColor, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(236f, -18f);
            rect.sizeDelta = new Vector2(48f, 48f);
            AddOutline(go, 1.5f);
            go.AddComponent<Button>();
        }

        void CreateBottomBar(Transform parent)
        {
            var bar = CreatePanel(parent, "DemoBar", new Color(0.18f, 0.18f, 0.2f, 0.94f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));
            var rect = bar.GetComponent<RectTransform>();
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, 120f);

            CreateBarText(bar.transform, "Hint", "시연을 시작하세요.", 20, TextAnchor.MiddleLeft, new Vector2(24f, 36f), new Vector2(0f, 0.5f), new Vector2(1100f, 40f));
            CreateBarText(bar.transform, "Status", "", 18, TextAnchor.MiddleLeft, new Vector2(24f, -8f), new Vector2(0f, 0.5f), new Vector2(1100f, 36f));

            CreateActionButton(bar.transform, "StartBattle", "전투 시작", new Vector2(-24f, 0f), new Vector2(1f, 0.5f), new Vector2(128f, 44f));
        }

        void CreateActionButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 anchor, Vector2 size)
        {
            var go = CreatePanel(parent, objectName, FillColor, anchor, anchor, anchor);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            AddOutline(go, 1.5f);
            CreateCenteredText(go.transform, label, 18, TextDark);
            go.AddComponent<Button>();
        }

        void BindDemo()
        {
            _director = GetComponent<AlphaDemoDirector>();
            var combat = GetComponent<CombatDirector>();
            if (_director == null || combat == null)
                return;

            BindButton("DesignDock/Design/Icon", ToggleDesignMenu);
            BindButton("DesignDock/DesignMenu/Trap/Icon", OpenTrapPalette);
            BindButton("DesignDock/DesignMenu/Terrain/Icon", LockedTool);
            BindButton("DesignDock/DesignMenu/Storage/Icon", OpenStorage);
            BindButton("DesignDock/DesignMenu/Core/Icon", LockedTool);
            BindButton("DesignDock/DesignMenu/Log/Icon", OpenLog);
            BindButton("DesignDock/DesignMenu/Back/Icon", CloseDesignStep);
            BindButton("DesignDock/TrapPalette/Spike/Icon", () => _director.BeginSpikePlacement());
            BindButton("StoragePanel/Dim", CloseStorage);
            BindButton("StoragePanel/Window/Close", CloseStorage);
            BindButton("StoragePanel/Window/Manage", () => _director.SetStatus("장비 관리는 준비 중입니다."));
            BindButton("LogPanel/Dim", CloseLog);
            BindButton("LogPanel/Window/Close", CloseLog);
            BindButton("DemoBar/StartBattle", () => _director.TryStartBattle());
            BindButton("TopBar/Report", () => _director.OpenWorry());
            BindButton("UnitCard/EquipToggle", () => _director.TryEquipBow());
            BindButton("UnitCard/EquipSlot", () => _director.TryEquipBow());
            BindButton("UnitCard/CardClose", () => _director.Select(null));
            BindButton("UnitCard/CardBond", () => _director.OpenWorry());
            ApplyPackIcons();
            ApplyUiFrames();
            RefreshWorryBlink();

            if (!_demoBound)
            {
                _demoBound = true;
                _director.Bind(this, combat);
            }
            else
            {
                RefreshDemoUi();
            }
        }

        void ApplyPackIcons()
        {
            ApplyButtonSprite("UnitCard/EquipSlot", PackAssets.LoadSprite(PackAssets.OldWoodBow));
            ApplySpikeIcon();
        }

        void ApplyUiFrames()
        {
            ApplyBarFrame("TopBar", PackAssets.UiTopBar(), false);
            SkinTopBarChips();
            PlaceTopHudHit("TopBar/Alert", 215f, 26f, 74f, 71f);
            PlaceTopHudHit("TopBar/Report", 305f, 26f, 74f, 71f);
            EnsureWorryBlink();
            ApplyDesignIcon("DesignDock/Design/Icon", 0);
            ApplyDesignIcon("DesignDock/DesignMenu/Trap/Icon", 1);
            ApplyDesignIcon("DesignDock/DesignMenu/Terrain/Icon", 2);
            ApplyDesignIcon("DesignDock/DesignMenu/Storage/Icon", 3);
            ApplyDesignIcon("DesignDock/DesignMenu/Core/Icon", 4);
            ApplyDesignIcon("DesignDock/DesignMenu/Log/Icon", 5);
            ApplyDesignIcon("DesignDock/DesignMenu/Back/Icon", 6);
            ApplyButtonFrame("DesignDock/TrapPalette/Spike/Icon", PackAssets.UiButtonBox(), true);
            try
            {
                ApplyStorageArt();
                ApplyLogArt();
            }
            catch (System.Exception)
            {
            }
        }

        void ApplyDesignIcon(string path, int index)
        {
            ApplyButtonFrame(path, PackAssets.UiDesignIcon(index), true);
            HideChildLabel(path);
            var node = FindUi(path);
            if (node == null || node.parent == null)
                return;

            var caption = node.parent.Find("Caption");
            if (caption != null)
                caption.gameObject.SetActive(false);
        }

        void ApplyBarFrame(string path, Sprite sprite, bool preserveAspect)
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
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = true;
        }

        void ApplyButtonFrame(string path, Sprite sprite, bool preserveAspect)
        {
            ApplyBarFrame(path, sprite, preserveAspect);
            var node = FindUi(path);
            if (node == null)
                return;

            var outline = node.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;
        }

        void SkinTopBarChips()
        {
            SkinChip("TopBar/Alert");
            SkinChip("TopBar/Report");
            SkinChip("TopBar/Menu");
        }

        void SkinChip(string path)
        {
            var node = FindUi(path);
            if (node == null)
                return;

            var image = node.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = new Color(1f, 1f, 1f, 0.01f);
            }
        }

        void PlaceTopHudHit(string path, float x, float y, float width, float height)
        {
            var node = FindUi(path);
            if (node == null)
                return;

            var rect = node.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(x / Frame61Width, y / Frame61Height);
            rect.anchorMax = new Vector2((x + width) / Frame61Width, (y + height) / Frame61Height);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        void EnsureWorryBlink()
        {
            var report = FindUi("TopBar/Report");
            if (report == null)
                return;

            var blink = report.Find("Blink");
            if (blink == null)
            {
                var go = new GameObject("Blink");
                go.transform.SetParent(report, false);
                go.AddComponent<RectTransform>();
                var image = go.AddComponent<Image>();
                image.sprite = _squareSprite;
                image.color = new Color(0f, 0f, 0f, 0f);
                image.raycastTarget = false;
                blink = go.transform;
            }

            var rect = blink.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.16f, 0.16f);
            rect.anchorMax = new Vector2(0.84f, 0.84f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        void HideChildLabel(string path)
        {
            var node = FindUi(path + "/Label");
            if (node != null)
                node.gameObject.SetActive(false);
        }

        void ApplyStorageArt()
        {
            ApplyUiArt("StoragePanel/Window/Title", PackAssets.StorageTitle());
            ApplyUiArt("StoragePanel/Window/Chest", PackAssets.StorageChest());
            ApplyUiArt("StoragePanel/Window/Tabs", PackAssets.StorageTabs());
            ApplyUiArt("StoragePanel/Window/Close", PackAssets.StorageClose());
            ApplyUiArt("StoragePanel/Window/Grid", PackAssets.StorageGrid());
            ApplyUiArt("StoragePanel/Window/Scroll", PackAssets.StorageScroll());
            ApplyUiArt("StoragePanel/Window/Thumb", PackAssets.StorageThumb());
            ApplyUiArt("StoragePanel/Window/Detail", PackAssets.StorageDetail());
        }

        void ApplyLogArt()
        {
            ApplyUiArt("LogPanel/Window/Title", PackAssets.LogTitle());
            ApplyUiArt("LogPanel/Window/Icon", PackAssets.LogIcon());
            ApplyUiArt("LogPanel/Window/Close", PackAssets.LogClose());
            ApplyUiArt("LogPanel/Window/Table", PackAssets.LogTable());
            ApplyUiArt("LogPanel/Window/Scroll", PackAssets.LogScroll());
            ApplyUiArt("LogPanel/Window/Thumb", PackAssets.LogThumb());
        }

        void ApplyUiArt(string path, Sprite sprite)
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
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
        }

        void ApplySpikeIcon()
        {
            var sprite = PackAssets.LoadSprite(PackAssets.SpikeIdle);
            var icon = FindUi("DesignDock/TrapPalette/Spike/Icon");
            if (icon == null || sprite == null)
                return;

            var caption = icon.Find("Label");
            if (caption != null)
                caption.gameObject.SetActive(false);

            var existing = icon.Find("Art");
            if (existing != null)
            {
                var image = existing.GetComponent<Image>();
                if (image != null)
                    image.sprite = sprite;
                return;
            }

            var art = new GameObject("Art");
            art.transform.SetParent(icon, false);
            var rect = art.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.15f);
            rect.anchorMax = new Vector2(0.85f, 0.85f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var artImage = art.AddComponent<Image>();
            artImage.sprite = sprite;
            artImage.color = Color.white;
            artImage.preserveAspect = true;
            artImage.raycastTarget = false;
        }

        void ApplyButtonSprite(string path, Sprite sprite)
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
            image.preserveAspect = true;
        }

        void Update()
        {
            if (!Application.isPlaying)
                return;

            RefreshWorryBlink();
        }

        void RefreshWorryBlink()
        {
            var report = FindUi("TopBar/Report");
            if (report == null)
                return;

            var blink = report.Find("Blink");
            var image = blink != null ? blink.GetComponent<Image>() : null;
            bool active = _director != null
                && _director.Session != null
                && _director.Session.Phase == DemoPhase.Worry
                && !_director.Session.WorryComplete;

            if (image == null)
                return;

            if (!active)
            {
                image.color = new Color(0f, 0f, 0f, 0f);
                return;
            }

            float pulse = (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.5f;
            image.color = Color.Lerp(new Color(0f, 0f, 0f, 0.08f), new Color(0f, 0f, 0f, 0.5f), pulse);
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

        public void RefreshDemoUi()
        {
            if (_director == null)
                return;

            SetText("DemoBar/Hint", _director.Hint());
            SetText("DemoBar/Status", _director.Status);
            RefreshUnitCard();
            RefreshDesignDock();

            var session = _director.Session;
            SetButtonInteractable("DemoBar/StartBattle", session.Phase == DemoPhase.PlaceTrap || session.Phase == DemoPhase.Strategy);
        }

        public void HideDesignMenus()
        {
            _designMenuOpen = false;
            _trapPaletteOpen = false;
            _storageOpen = false;
            _logOpen = false;
            RefreshDesignDock();
        }

        void ToggleDesignMenu()
        {
            if (_director != null && _director.Combat != null && _director.Combat.InBattle)
                return;

            _designMenuOpen = !_designMenuOpen;
            if (!_designMenuOpen)
            {
                _trapPaletteOpen = false;
                _director?.CancelSpikePlacement();
            }
            else
            {
                _storageOpen = false;
                _logOpen = false;
                _director?.Select(null);
            }

            RefreshDesignDock();
        }

        void OpenTrapPalette()
        {
            if (_director != null && _director.Session != null
                && (int)_director.Session.Phase < (int)DemoPhase.PlaceTrap
                && _director.Session.Phase != DemoPhase.EquipBow)
            {
                _director.SetStatus("먼저 고목 활을 장착하세요.");
                return;
            }

            _trapPaletteOpen = true;
            RefreshDesignDock();
        }

        void OpenStorage()
        {
            if (_director != null && _director.Combat != null && _director.Combat.InBattle)
                return;

            _storageOpen = true;
            _logOpen = false;
            _designMenuOpen = false;
            _trapPaletteOpen = false;
            _director?.CancelSpikePlacement();
            _director?.Select(null);
            RefreshDesignDock();
        }

        void CloseStorage()
        {
            _storageOpen = false;
            RefreshDesignDock();
        }

        void OpenLog()
        {
            if (_director != null && _director.Combat != null && _director.Combat.InBattle)
                return;

            _logOpen = true;
            _storageOpen = false;
            _designMenuOpen = false;
            _trapPaletteOpen = false;
            _director?.CancelSpikePlacement();
            _director?.Select(null);
            RefreshDesignDock();
        }

        void CloseLog()
        {
            _logOpen = false;
            RefreshDesignDock();
        }

        void CloseDesignStep()
        {
            if (_trapPaletteOpen)
            {
                _trapPaletteOpen = false;
                _director?.CancelSpikePlacement();
                RefreshDesignDock();
                return;
            }

            _designMenuOpen = false;
            _director?.CancelSpikePlacement();
            RefreshDesignDock();
        }

        void LockedTool()
        {
            _director?.SetStatus("아직 사용할 수 없습니다.");
        }

        void RefreshDesignDock()
        {
            bool inBattle = _director != null && _director.Combat != null && _director.Combat.InBattle;
            if (inBattle)
            {
                _designMenuOpen = false;
                _trapPaletteOpen = false;
                _storageOpen = false;
                _logOpen = false;
            }

            bool overlay = _storageOpen || _logOpen;
            SetActive("DesignDock", !inBattle);
            SetActive("DesignDock/DesignMenu", _designMenuOpen && !inBattle && !overlay);
            SetActive("DesignDock/TrapPalette", _designMenuOpen && _trapPaletteOpen && !inBattle && !overlay);
            SetActive("StoragePanel", _storageOpen && !inBattle);
            SetActive("LogPanel", _logOpen && !inBattle);
        }

        void SetActive(string path, bool active)
        {
            var node = FindUi(path);
            if (node != null)
                node.gameObject.SetActive(active);
        }

        Transform FindUi(string path)
        {
            var canvas = FindNamedChild(transform, CanvasName);
            if (canvas == null)
                return null;
            if (string.IsNullOrEmpty(path))
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

        void RefreshUnitCard()
        {
            var card = FindUi("UnitCard");
            if (card == null)
                return;

            var selected = _director.Selected;
            bool inBattle = _director.Combat != null && _director.Combat.InBattle;
            bool show = selected != null && !inBattle && !_designMenuOpen && !_storageOpen && !_logOpen;
            card.gameObject.SetActive(show);
            if (!show)
                return;

            string species = selected.Team == CombatTeam.Defender ? "고블린" : (selected.UnitName.Contains("궁") ? "궁수" : "전사");
            SetText("UnitCard/Title", $"{selected.UnitName} - {species}");
            SetText("UnitCard/HpTrack/HpValue", $"{selected.DisplayHealth} / {selected.DisplayMaxHealth}");

            var fill = FindUi("UnitCard/HpTrack/Fill");
            if (fill != null)
            {
                var image = fill.GetComponent<Image>();
                if (image != null)
                    image.fillAmount = selected.DisplayMaxHealth <= 0 ? 0f : selected.Health / selected.MaxHealth;
            }

            bool canEquip = selected.Team == CombatTeam.Defender
                && (_director.Session.Phase == DemoPhase.Inspect
                    || _director.Session.Phase == DemoPhase.EquipBow
                    || _director.Session.Phase == DemoPhase.PlaceTrap);
            SetButtonInteractable("UnitCard/EquipToggle", canEquip);
            SetButtonInteractable("UnitCard/EquipSlot", canEquip);
            SetText("UnitCard/EquipToggle/Label", selected.HasOldWoodBow() ? "고목활 착용 중" : "장비 착용 전환");

            var slot = FindUi("UnitCard/EquipSlot");
            if (slot != null)
            {
                var image = slot.GetComponent<Image>();
                if (image != null)
                    image.color = selected.HasOldWoodBow() ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            }

            string action = _director.Session.FocusFire ? "점사 공격 중" : "기본 공격 중";
            SetText("UnitCard/Action", $"행동 : {action}");
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
                text.text = value;
        }

        void SetButtonInteractable(string path, bool interactable)
        {
            var node = FindUi(path);
            if (node == null)
                return;

            var button = node.GetComponent<Button>();
            if (button != null)
                button.interactable = interactable;
        }

        void CreateToken(
            Transform parent,
            string objectName,
            string label,
            bool circle,
            Vector2 size,
            Vector2 anchoredPosition,
            Vector2 anchor)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.sprite = circle ? _circleSprite : _squareSprite;
            image.color = FillColor;
            image.type = Image.Type.Simple;
            AddOutline(go, 2f);
            CreateCenteredText(go.transform, label, circle && size.x > 180f ? 28 : 20, TextDark);
        }

        void CreateRoundButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 anchor)
        {
            CreateToken(parent, objectName, label, true, new Vector2(54f, 54f), anchoredPosition, anchor);
        }

        static void AddButtonIfMissing(Transform node)
        {
            if (node != null && node.GetComponent<Button>() == null)
                node.gameObject.AddComponent<Button>();
        }

        GameObject CreatePanel(Transform parent, string objectName, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        void CreateBarText(
            Transform parent,
            string objectName,
            string content,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchoredPosition,
            Vector2 anchor,
            Vector2 size)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.UpperLeft
                ? new Vector2(0f, alignment == TextAnchor.UpperLeft ? 1f : 0.5f)
                : alignment == TextAnchor.MiddleRight ? new Vector2(1f, 0.5f) : new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var text = go.AddComponent<Text>();
            ConfigureText(text, content, fontSize, alignment, objectName == "Hint" || objectName == "Status" || parent.name == "TopBar" ? Color.white : TextDark);
        }

        void CreateBarChip(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 anchor)
        {
            var go = CreatePanel(parent, objectName, ChipColor, anchor, anchor, new Vector2(0.5f, 0.5f));
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(78f, 48f);
            if (!string.IsNullOrEmpty(label))
                CreateCenteredText(go.transform, label, 18, TextDark);
            if (objectName == "Report")
                go.AddComponent<Button>();
        }

        void CreateCenteredText(Transform parent, string content, int fontSize, Color color)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            Stretch(go.AddComponent<RectTransform>());
            var text = go.AddComponent<Text>();
            ConfigureText(text, content, fontSize, TextAnchor.MiddleCenter, color);
        }

        void AddOutline(GameObject go, float distance)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = LineColor;
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
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
        }

        void ApplyFonts()
        {
            var texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].font == null)
                    texts[i].font = _font;
            }
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        void SpawnWarriorVisuals()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            var position = cam.ViewportToWorldPoint(new Vector3(0.14f, 0.48f, 10f));
            position.z = 0f;

            var existing = transform.Find("WarriorVisuals");
            if (existing != null && existing.childCount == 1)
            {
                existing.GetChild(0).position = position;
                return;
            }

            if (existing != null)
            {
                if (Application.isPlaying)
                    Destroy(existing.gameObject);
                else
                    DestroyImmediate(existing.gameObject);
            }

            var root = new GameObject("WarriorVisuals");
            root.transform.SetParent(transform, false);
            WarriorView.Spawn(root.transform, position);
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
            var font = Font.CreateDynamicFontFromOSFont(
                new[] { "Malgun Gothic", "맑은 고딕", "Gulim", "Apple SD Gothic Neo", "Arial" },
                32);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                font.hideFlags = HideFlags.DontSave;
                font.RequestCharactersInTexture(
                    "Day방입구코어기사고블린오크웨어울프설계알림고민메뉴코인전투시작점사부터장착가시바닥이유없이화가난다해결다음유대감전략기본공격원거리근거리사거리장비고목활정보용사몬스터선택설치관찰비교종료착용전환행동중함정지형보관함업그레이드기록이전이▦▲≡▣◆☰◀※♥X<>0123456789:/+%.LvATKHP EXP",
                    32,
                    FontStyle.Normal);
            }

            return font;
        }
    }
}
