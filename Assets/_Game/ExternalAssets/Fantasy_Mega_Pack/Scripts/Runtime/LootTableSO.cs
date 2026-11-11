// LootTableSO.cs - weighted random loot table asset.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Weighted random loot table asset (Create > FantasyDungeon > Loot Table). Each entry: item, chance, min/max amount.
    /// </summary>
    [CreateAssetMenu(fileName = "LootTable", menuName = "FantasyDungeon/Loot Table")]
    public class LootTableSO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public ItemSO item;
            [Range(0f, 1f)] public float chance = 0.5f;
            public int minAmount = 1;
            public int maxAmount = 1;
        }

        public List<Entry> entries = new List<Entry>();

        /// <summary>Rolls the table once and returns the dropped items with amounts.</summary>
        public List<KeyValuePair<ItemSO, int>> Roll()
        {
            var res = new List<KeyValuePair<ItemSO, int>>();
            foreach (var e in entries)
            {
                if (e.item != null && UnityEngine.Random.value <= e.chance)
                {
                    int a = UnityEngine.Random.Range(e.minAmount, e.maxAmount + 1);
                    if (a > 0) res.Add(new KeyValuePair<ItemSO, int>(e.item, a));
                }
            }
            return res;
        }
    }
}
