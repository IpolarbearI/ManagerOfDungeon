// GameHUD.cs - Canvas HUD: HP/MP/XP bars, level, gold counter, 4-slot ability hotbar with cooldowns.
using UnityEngine;
using UnityEngine.UI;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Main gameplay HUD (Canvas): health/mana/xp bars with level badge, gold counter,
    /// and the 1-4 ability hotbar with cooldown overlays. Finds player systems automatically.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }
        [Tooltip("Item used as currency for the gold counter (e.g. Gold Coin).")]
        public ItemSO goldItem;

        Health _health;
        StatsSystem _stats;
        InventorySystem _inventory;
        AbilityHotbar _hotbar;

        Image _hpFill, _mpFill, _xpFill;
        Text _hpText, _levelText, _goldText;
        Image[] _slotCooldowns = new Image[4];
        Text[] _slotKeys = new Text[4];

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
        }

        void Start() { FindPlayer(); AudioManager.PlayBGM("village"); }

        void FindPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            _health = player.GetComponent<Health>();
            _stats = player.GetComponent<StatsSystem>();
            _inventory = player.GetComponent<InventorySystem>();
            _hotbar = player.GetComponent<AbilityHotbar>();
        }

        void Update()
        {
            if (_health == null) { FindPlayer(); if (_health == null) return; }

            float hp = _health.maxHealth > 0 ? (float)_health.CurrentHealth / _health.maxHealth : 0f;
            _hpFill.fillAmount = Mathf.Lerp(_hpFill.fillAmount, hp, Time.deltaTime * 10f);
            _hpText.text = _health.CurrentHealth + " / " + _health.maxHealth;

            if (_stats != null)
            {
                _mpFill.fillAmount = _stats.MPPercent;
                _xpFill.fillAmount = _stats.XPPercent;
                _levelText.text = "Lv " + _stats.level;
            }

            if (_inventory != null && goldItem != null)
                _goldText.text = _inventory.CountOf(goldItem).ToString();

            if (_hotbar != null)
            {
                for (int i = 0; i < 4 && i < _hotbar.abilities.Length; i++)
                {
                    var ab = _hotbar.abilities[i];
                    _slotCooldowns[i].fillAmount = (ab != null && ab.cooldown > 0f) ? Mathf.Clamp01(ab.cd / ab.cooldown) : 0f;
                }
            }
        }

        void BuildUI()
        {
            var canvasGO = new GameObject("HUDCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // ----- top-left: bars panel -----
            var panel = MakeImage(canvasGO.transform, "BarsPanel", new Color(0.05f, 0.05f, 0.08f, 0.65f));
            var prt = panel.rectTransform;
            prt.anchorMin = new Vector2(0, 1); prt.anchorMax = new Vector2(0, 1);
            prt.pivot = new Vector2(0, 1);
            prt.sizeDelta = new Vector2(380, 130);
            prt.anchoredPosition = new Vector2(18, -18);

            _levelText = MakeText(panel.transform, "Level", 26, FontStyle.Bold, new Color(1f, 0.85f, 0.4f));
            SetRect(_levelText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(90, 30), new Vector2(12, -8));

            _hpFill = MakeBar(panel.transform, "HP", new Color(0.85f, 0.2f, 0.2f), -42, out _hpText);
            _mpFill = MakeBar(panel.transform, "MP", new Color(0.25f, 0.45f, 0.95f), -76, out _);
            _xpFill = MakeBar(panel.transform, "XP", new Color(0.95f, 0.8f, 0.25f), -106, out _);
            _xpFill.transform.parent.localScale = new Vector3(1f, 0.55f, 1f);

            // ----- top-left: gold -----
            var goldPanel = MakeImage(canvasGO.transform, "GoldPanel", new Color(0.05f, 0.05f, 0.08f, 0.65f));
            var grt = goldPanel.rectTransform;
            grt.anchorMin = new Vector2(0, 1); grt.anchorMax = new Vector2(0, 1);
            grt.pivot = new Vector2(0, 1);
            grt.sizeDelta = new Vector2(150, 44);
            grt.anchoredPosition = new Vector2(18, -156);
            var coin = MakeText(goldPanel.transform, "CoinIcon", 24, FontStyle.Bold, new Color(1f, 0.84f, 0.2f));
            coin.text = "●";
            SetRect(coin.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 30), new Vector2(10, 0));
            _goldText = MakeText(goldPanel.transform, "Gold", 24, FontStyle.Bold, Color.white);
            _goldText.alignment = TextAnchor.MiddleLeft;
            SetRect(_goldText.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0.5f), new Vector2(-50, 36), new Vector2(44, 0));
            _goldText.text = "0";

            // ----- bottom-center: hotbar -----
            var hotbar = MakeImage(canvasGO.transform, "Hotbar", new Color(0.05f, 0.05f, 0.08f, 0.55f));
            var hrt = hotbar.rectTransform;
            hrt.anchorMin = new Vector2(0.5f, 0); hrt.anchorMax = new Vector2(0.5f, 0);
            hrt.pivot = new Vector2(0.5f, 0);
            hrt.sizeDelta = new Vector2(4 * 78 + 18, 92);
            hrt.anchoredPosition = new Vector2(0, 14);
            for (int i = 0; i < 4; i++)
            {
                var slot = MakeImage(hotbar.transform, "Slot" + (i + 1), new Color(0.18f, 0.16f, 0.22f, 0.9f));
                var srt = slot.rectTransform;
                srt.anchorMin = new Vector2(0, 0.5f); srt.anchorMax = new Vector2(0, 0.5f);
                srt.pivot = new Vector2(0, 0.5f);
                srt.sizeDelta = new Vector2(70, 70);
                srt.anchoredPosition = new Vector2(12 + i * 78, 0);

                var cdImg = MakeImage(slot.transform, "CD", new Color(0, 0, 0, 0.7f));
                cdImg.type = Image.Type.Filled;
                cdImg.fillMethod = Image.FillMethod.Radial360;
                cdImg.fillOrigin = (int)Image.Origin360.Top;
                cdImg.fillClockwise = false;
                var crt = cdImg.rectTransform;
                crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
                crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
                _slotCooldowns[i] = cdImg;

                var key = MakeText(slot.transform, "Key", 18, FontStyle.Bold, new Color(0.9f, 0.88f, 0.8f));
                key.text = (i + 1).ToString();
                key.alignment = TextAnchor.LowerRight;
                SetRect(key.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                key.rectTransform.offsetMax = new Vector2(-4, -2);
                _slotKeys[i] = key;
            }
        }

        Image MakeBar(Transform parent, string label, Color color, float y, out Text valueText)
        {
            var bg = MakeImage(parent, label + "BG", new Color(0.15f, 0.14f, 0.18f, 0.95f));
            var rt = bg.rectTransform;
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(350, 26);
            rt.anchoredPosition = new Vector2(15, y);

            var fill = MakeImage(bg.transform, "Fill", color);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            var frt = fill.rectTransform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = new Vector2(2, 2); frt.offsetMax = new Vector2(-2, -2);

            valueText = MakeText(bg.transform, "Value", 17, FontStyle.Bold, Color.white);
            valueText.alignment = TextAnchor.MiddleCenter;
            SetRect(valueText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return fill;
        }

        Image MakeImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        Text MakeText(Transform parent, string name, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = GameFonts.UI;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
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
