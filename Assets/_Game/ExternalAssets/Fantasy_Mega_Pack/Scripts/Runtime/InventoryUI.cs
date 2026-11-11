// InventoryUI.cs - Canvas inventory grid (toggle I): click to use potions/consumables or equip gear.
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Visual inventory grid (toggle with I). Click a slot: potions/consumables are used,
    /// weapons/armor/shields/accessories are equipped. Shows equipped gear row + item tooltip.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance { get; private set; }
        public KeyCode toggleKey = KeyCode.I;

        InventorySystem _inv;
        EquipmentSystem _equip;
        StatsSystem _stats;
        Health _health;

        GameObject _root;
        readonly List<Image> _slotIcons = new List<Image>();
        readonly List<Text> _slotCounts = new List<Text>();
        readonly List<Button> _slotButtons = new List<Button>();
        Image[] _equipIcons = new Image[4];
        Text _tooltip;
        bool _open;
        const int COLS = 6, ROWS = 4;

        static readonly Color[] RarityColors = {
            new Color(0.75f,0.75f,0.75f), new Color(0.4f,0.85f,0.4f), new Color(0.35f,0.55f,0.95f),
            new Color(0.75f,0.4f,0.95f), new Color(1f,0.7f,0.2f) };

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            _root.SetActive(false);
        }

        void Start() { FindPlayer(); }

        void FindPlayer()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            _inv = p.GetComponent<InventorySystem>();
            _equip = p.GetComponent<EquipmentSystem>();
            _stats = p.GetComponent<StatsSystem>();
            _health = p.GetComponent<Health>();
            if (_inv != null) _inv.OnChanged += Refresh;
            if (_equip != null) _equip.OnChanged += Refresh;
        }

        void Update()
        {
            if (GameInput.InventoryPressed() && !DialogueBoxUI.IsOpen)
            {
                _open = !_open;
                _root.SetActive(_open);
                if (_open) { if (_inv == null) FindPlayer(); Refresh(); AudioManager.PlaySFX("ui_open"); }
            }
        }

        void OnSlotClicked(int index)
        {
            if (_inv == null || index >= _inv.slots.Count) return;
            var slot = _inv.slots[index];
            if (slot.IsEmpty) return;
            var item = slot.item;
            switch (item.type)
            {
                case ItemSO.ItemType.Potion:
                case ItemSO.ItemType.Consumable:
                    bool used = false;
                    if (item.healHP > 0 && _health != null && _health.CurrentHealth < _health.maxHealth)
                    { _health.Heal(item.healHP); used = true; }
                    if (item.restoreMP > 0 && _stats != null && _stats.currentMP < _stats.maxMP)
                    { _stats.currentMP = Mathf.Min(_stats.maxMP, _stats.currentMP + item.restoreMP); used = true; }
                    if (used) { _inv.Remove(item, 1); AudioManager.PlaySFX("potion"); }
                    break;
                case ItemSO.ItemType.Weapon:
                case ItemSO.ItemType.Armor:
                case ItemSO.ItemType.Shield:
                case ItemSO.ItemType.Accessory:
                    if (_equip != null) { _equip.Equip(item); AudioManager.PlaySFX("equip"); }
                    break;
            }
            Refresh();
        }

        void Refresh()
        {
            if (_inv == null || _root == null || !_root.activeSelf) return;
            for (int i = 0; i < _slotIcons.Count; i++)
            {
                bool has = i < _inv.slots.Count && !_inv.slots[i].IsEmpty;
                _slotIcons[i].enabled = has;
                _slotCounts[i].text = "";
                if (has)
                {
                    var s = _inv.slots[i];
                    _slotIcons[i].sprite = s.item.icon;
                    _slotIcons[i].color = Color.white;
                    if (s.count > 1) _slotCounts[i].text = s.count.ToString();
                    var outline = _slotButtons[i].GetComponent<Outline>();
                    if (outline != null) outline.effectColor = RarityColors[Mathf.Clamp(s.item.rarity, 0, 4)];
                }
            }
            if (_equip != null)
            {
                SetEquipIcon(0, _equip.weapon);
                SetEquipIcon(1, _equip.armor);
                SetEquipIcon(2, _equip.shield);
                SetEquipIcon(3, _equip.accessory);
            }
        }

        void SetEquipIcon(int i, ItemSO item)
        {
            _equipIcons[i].enabled = item != null;
            if (item != null) _equipIcons[i].sprite = item.icon;
        }

        void ShowTooltip(int index)
        {
            if (_inv == null || index >= _inv.slots.Count || _inv.slots[index].IsEmpty) { _tooltip.text = ""; return; }
            var it = _inv.slots[index].item;
            string stats = "";
            if (it.bonusAttack != 0) stats += "  ATK+" + it.bonusAttack;
            if (it.bonusDefense != 0) stats += "  DEF+" + it.bonusDefense;
            if (it.healHP != 0) stats += "  Heals " + it.healHP + " HP";
            if (it.restoreMP != 0) stats += "  Restores " + it.restoreMP + " MP";
            _tooltip.text = it.itemName + stats + "\n<i>" + it.description + "</i>";
        }

        void BuildUI()
        {
            var canvasGO = new GameObject("InventoryCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 800;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            _root = new GameObject("Panel");
            _root.transform.SetParent(canvasGO.transform, false);
            var bg = _root.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.07f, 0.11f, 0.96f);
            var rt = bg.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(640, 640);

            var title = MakeText(_root.transform, "Title", 34, FontStyle.Bold, new Color(1f, 0.85f, 0.4f));
            title.text = "INVENTORY";
            title.alignment = TextAnchor.MiddleCenter;
            SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 50), new Vector2(0, -14));

            // equipment row
            string[] eqLabels = { "WPN", "ARM", "SHD", "ACC" };
            for (int i = 0; i < 4; i++)
            {
                var eq = MakeSlotVisual(_root.transform, "Equip" + i, out Image icon, out _);
                var ert = eq.GetComponent<RectTransform>();
                ert.anchorMin = new Vector2(0.5f, 1); ert.anchorMax = new Vector2(0.5f, 1);
                ert.pivot = new Vector2(0.5f, 1);
                ert.sizeDelta = new Vector2(80, 80);
                ert.anchoredPosition = new Vector2(-135 + i * 90, -72);
                _equipIcons[i] = icon;
                var lbl = MakeText(eq.transform, "L", 14, FontStyle.Bold, new Color(0.6f, 0.58f, 0.5f));
                lbl.text = eqLabels[i];
                lbl.alignment = TextAnchor.UpperCenter;
                SetRect(lbl.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(80, 18), new Vector2(0, -2));
            }

            // grid
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    int index = r * COLS + c;
                    var slot = MakeSlotVisual(_root.transform, "Slot" + index, out Image icon, out Text count);
                    var srt = slot.GetComponent<RectTransform>();
                    srt.anchorMin = new Vector2(0.5f, 1); srt.anchorMax = new Vector2(0.5f, 1);
                    srt.pivot = new Vector2(0.5f, 1);
                    srt.sizeDelta = new Vector2(86, 86);
                    srt.anchoredPosition = new Vector2(-237.5f + c * 95, -190 - r * 95);
                    _slotIcons.Add(icon);
                    _slotCounts.Add(count);
                    var btn = slot.AddComponent<Button>();
                    int captured = index;
                    btn.onClick.AddListener(() => OnSlotClicked(captured));
                    _slotButtons.Add(btn);
                    var trigger = slot.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                    var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
                    entry.callback.AddListener(_ => ShowTooltip(captured));
                    trigger.triggers.Add(entry);
                }
            }

            _tooltip = MakeText(_root.transform, "Tooltip", 22, FontStyle.Normal, new Color(0.92f, 0.9f, 0.85f));
            _tooltip.alignment = TextAnchor.UpperCenter;
            SetRect(_tooltip.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(-40, 60), new Vector2(0, 8));

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                GameInput.AddUIInputModule(es);
            }
        }

        GameObject MakeSlotVisual(Transform parent, string name, out Image icon, out Text count)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.17f, 0.15f, 0.21f, 1f);
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(0.35f, 0.32f, 0.4f);
            ol.effectDistance = new Vector2(2, -2);
            var igo = new GameObject("Icon");
            igo.transform.SetParent(go.transform, false);
            icon = igo.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.enabled = false;
            var irt = icon.rectTransform;
            irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(8, 8); irt.offsetMax = new Vector2(-8, -8);
            count = MakeText(go.transform, "Count", 18, FontStyle.Bold, Color.white);
            count.alignment = TextAnchor.LowerRight;
            count.raycastTarget = false;
            SetRect(count.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            count.rectTransform.offsetMax = new Vector2(-5, -3);
            return go;
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
            t.supportRichText = true;
            return t;
        }

        void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
        }
    }
}
