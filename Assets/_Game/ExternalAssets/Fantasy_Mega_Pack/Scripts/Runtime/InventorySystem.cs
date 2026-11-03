// InventorySystem.cs - slot-based inventory with stacking. Drop on the player.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Slot-based inventory with stacking. Add(item, count) / Remove / CountOf; fires OnChanged for UI refreshes.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Serializable]
        public class Slot
        {
            public ItemSO item;
            public int count;
            public bool IsEmpty => item == null || count <= 0;
        }

        public int size = 24;
        public List<Slot> slots = new List<Slot>();

        public event Action OnChanged;

        void Awake()
        {
            while (slots.Count < size) slots.Add(new Slot());
        }

        public bool Add(ItemSO item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;
            if (item.stackable)
            {
                foreach (var s in slots)
                {
                    if (s.item == item && s.count < item.maxStack)
                    {
                        int can = Mathf.Min(amount, item.maxStack - s.count);
                        s.count += can; amount -= can;
                        if (amount <= 0) { OnChanged?.Invoke(); return true; }
                    }
                }
            }
            foreach (var s in slots)
            {
                if (s.IsEmpty)
                {
                    s.item = item;
                    s.count = Mathf.Min(amount, item.maxStack);
                    amount -= s.count;
                    if (amount <= 0) { OnChanged?.Invoke(); return true; }
                }
            }
            OnChanged?.Invoke();
            return amount <= 0;
        }

        public bool Remove(ItemSO item, int amount = 1)
        {
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                var s = slots[i];
                if (s.item == item)
                {
                    int take = Mathf.Min(amount, s.count);
                    s.count -= take; amount -= take;
                    if (s.count <= 0) s.item = null;
                    if (amount <= 0) { OnChanged?.Invoke(); return true; }
                }
            }
            OnChanged?.Invoke();
            return false;
        }

        public int CountOf(ItemSO item)
        {
            int c = 0;
            foreach (var s in slots) if (s.item == item) c += s.count;
            return c;
        }
    }
}
