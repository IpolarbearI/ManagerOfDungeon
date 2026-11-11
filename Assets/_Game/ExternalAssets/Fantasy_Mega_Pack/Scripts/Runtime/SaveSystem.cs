// SaveSystem.cs - JSON save/load of position, health, stats and inventory (F5 save, F9 load).
// Works with both legacy Input Manager and the new Input System.
using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Drop-in JSON save system. Put it on the player. F5 saves, F9 loads.
    /// Persists: position, current HP, stats (level/xp/mp) and the inventory.
    /// Inventory items are restored by name via <see cref="itemDatabase"/> —
    /// drag every ItemSO your game uses into that list (or leave empty to skip inventory persistence).
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        [System.Serializable]
        public class ItemEntry { public string item; public int count; }

        [System.Serializable]
        public class SaveData
        {
            public float x; public float y;
            public int hp;
            public int level; public int xp; public int mp;
            public List<ItemEntry> items = new List<ItemEntry>();
        }

        [Tooltip("All ItemSO assets used by your game. Needed to restore inventory by name on load.")]
        public List<ItemSO> itemDatabase = new List<ItemSO>();

        Health _health;
        StatsSystem _stats;
        InventorySystem _inventory;

        string FilePath => Path.Combine(Application.persistentDataPath, "fdp_save.json");

        void Awake()
        {
            _health = GetComponent<Health>();
            _stats = GetComponent<StatsSystem>();
            _inventory = GetComponent<InventorySystem>();
        }

        void Update()
        {
            if (SavePressed()) Save();
            if (LoadPressed()) Load();
        }

        /// <summary>Writes the current game state to a JSON file in Application.persistentDataPath.</summary>
        public void Save()
        {
            var d = new SaveData
            {
                x = transform.position.x,
                y = transform.position.y,
                hp = _health != null ? _health.CurrentHealth : 0
            };
            if (_stats != null) { d.level = _stats.level; d.xp = _stats.xp; d.mp = _stats.currentMP; }
            if (_inventory != null)
            {
                foreach (var s in _inventory.slots)
                    if (!s.IsEmpty) d.items.Add(new ItemEntry { item = s.item.itemName, count = s.count });
            }
            File.WriteAllText(FilePath, JsonUtility.ToJson(d, true));
            Debug.Log("[FDP] Game saved -> " + FilePath);
        }

        /// <summary>Restores position, HP, stats and (if itemDatabase is filled) the inventory.</summary>
        public void Load()
        {
            if (!File.Exists(FilePath)) { Debug.Log("[FDP] No save file."); return; }
            var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            transform.position = new Vector3(d.x, d.y, transform.position.z);
            if (_health != null && d.hp > 0) _health.SetHealth(d.hp);
            if (_stats != null)
            {
                _stats.level = Mathf.Max(1, d.level);
                _stats.xp = d.xp;
                _stats.currentMP = d.mp;
            }
            if (_inventory != null && itemDatabase.Count > 0 && d.items != null)
            {
                foreach (var s in _inventory.slots) { s.item = null; s.count = 0; }
                foreach (var e in d.items)
                {
                    var so = itemDatabase.Find(i => i != null && i.itemName == e.item);
                    if (so != null) _inventory.Add(so, e.count);
                }
            }
            Debug.Log("[FDP] Game loaded.");
        }

        bool SavePressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F5);
#endif
        }

        bool LoadPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F9);
#endif
        }
    }
}
