// EquipmentSystem.cs - equip weapon/armor/shield/accessory from ItemSO; bonuses flow into combat.
using System;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Equipment slots (Weapon/Armor/Shield/Accessory). Equip(item) from the inventory;
    /// TotalAttackBonus/TotalDefenseBonus are picked up by PlayerCombat and Health.
    /// Press I to toggle the equipment + inventory panel (OnGUI, zero dependencies).
    /// </summary>
    public class EquipmentSystem : MonoBehaviour
    {
        public ItemSO weapon, armor, shield, accessory;
        public event Action OnChanged;

        InventorySystem _inv;
        bool _open;

        public int TotalAttackBonus =>
            (weapon ? weapon.bonusAttack : 0) + (accessory ? accessory.bonusAttack : 0);
        public int TotalDefenseBonus =>
            (armor ? armor.bonusDefense : 0) + (shield ? shield.bonusDefense : 0) + (accessory ? accessory.bonusDefense : 0);

        void Awake() { _inv = GetComponent<InventorySystem>(); }

        /// <summary>Equips an item into its matching slot (swaps the old one back into the inventory).</summary>
        public void Equip(ItemSO item)
        {
            if (item == null) return;
            ItemSO old = null;
            switch (item.type)
            {
                case ItemSO.ItemType.Weapon: old = weapon; weapon = item; break;
                case ItemSO.ItemType.Armor: old = armor; armor = item; break;
                case ItemSO.ItemType.Shield: old = shield; shield = item; break;
                case ItemSO.ItemType.Accessory: old = accessory; accessory = item; break;
                default: return;
            }
            if (_inv != null)
            {
                _inv.Remove(item, 1);
                if (old != null) _inv.Add(old, 1);
            }
            OnChanged?.Invoke();
        }

        void Update()
        {
            if (TogglePressed()) _open = !_open;
        }

        void OnGUI()
        {
            if (!_open) return;
            var r = new Rect(Screen.width - 330, 12, 318, 420);
            GUI.Box(r, "EQUIPMENT  +  INVENTORY   [I]");
            var st = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            GUI.Label(new Rect(r.x + 12, r.y + 26, 300, 20), "Weapon:    " + (weapon ? weapon.itemName + "  (+" + weapon.bonusAttack + " ATK)" : "-"), st);
            GUI.Label(new Rect(r.x + 12, r.y + 46, 300, 20), "Armor:     " + (armor ? armor.itemName + "  (+" + armor.bonusDefense + " DEF)" : "-"), st);
            GUI.Label(new Rect(r.x + 12, r.y + 66, 300, 20), "Shield:    " + (shield ? shield.itemName + "  (+" + shield.bonusDefense + " DEF)" : "-"), st);
            GUI.Label(new Rect(r.x + 12, r.y + 86, 300, 20), "Accessory: " + (accessory ? accessory.itemName : "-"), st);
            if (_inv == null) return;
            float y = r.y + 116;
            GUI.Label(new Rect(r.x + 12, y, 300, 20), "— Inventory (click to equip/use) —", st); y += 22;
            foreach (var s in _inv.slots)
            {
                if (s.IsEmpty || y > r.yMax - 26) continue;
                string label = s.item.itemName + "  x" + s.count;
                if (GUI.Button(new Rect(r.x + 12, y, 294, 22), label))
                {
                    if (s.item.type == ItemSO.ItemType.Potion || s.item.type == ItemSO.ItemType.Consumable)
                    {
                        var h = GetComponent<Health>(); var stats = GetComponent<StatsSystem>();
                        if (h != null && s.item.healHP > 0) h.Heal(s.item.healHP);
                        if (stats != null && s.item.restoreMP > 0) { stats.currentMP = Mathf.Min(stats.maxMP, stats.currentMP + s.item.restoreMP); }
                        FloatingDamageNumbers.Show(transform.position, Mathf.Max(s.item.healHP, s.item.restoreMP), new Color(0.4f, 1f, 0.5f));
                        _inv.Remove(s.item, 1);
                    }
                    else Equip(s.item);
                    break;
                }
                y += 24;
            }
        }

        bool TogglePressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.iKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.I);
#endif
        }
    }
}
