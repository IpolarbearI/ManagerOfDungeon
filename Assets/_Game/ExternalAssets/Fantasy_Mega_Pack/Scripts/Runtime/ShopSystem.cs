// ShopSystem.cs - buy/sell shop for the Merchant NPC. Currency = any ItemSO (default: Gold Coin).
using System.Collections.Generic;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Merchant shop: put on an NPC with a trigger collider. Player presses E to open.
    /// Buy items for coins, sell inventory items for half value. Uses ItemSO.value.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShopSystem : MonoBehaviour, IInteractable
    {
        [Tooltip("Items this merchant sells.")]
        public List<ItemSO> stock = new List<ItemSO>();
        [Tooltip("The coin item used as currency.")]
        public ItemSO currency;

        bool _open;
        InventorySystem _customer;

        public string Prompt => "Shop";

        public void Interact(GameObject interactor)
        {
            _customer = interactor.GetComponent<InventorySystem>();
            _open = !_open && _customer != null;
        }

        void OnGUI()
        {
            if (!_open || _customer == null) return;
            int coins = currency != null ? _customer.CountOf(currency) : 0;
            var r = new Rect(Screen.width / 2 - 180, 60, 360, 92 + Mathf.Max(stock.Count, 1) * 24 + 60);
            GUI.Box(r, "MERCHANT — your gold: " + coins);
            float y = r.y + 30;
            var st = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(r.x + 12, y, 200, 20), "BUY:", st); y += 22;
            foreach (var item in stock)
            {
                if (item == null) continue;
                if (GUI.Button(new Rect(r.x + 12, y, 336, 22), item.itemName + "   —   " + item.value + "g"))
                {
                    if (currency != null && _customer.CountOf(currency) >= item.value)
                    {
                        _customer.Remove(currency, item.value);
                        _customer.Add(item, 1);
                    }
                }
                y += 24;
            }
            y += 8;
            GUI.Label(new Rect(r.x + 12, y, 280, 20), "SELL (half value, click):", st); y += 22;
            foreach (var s in _customer.slots)
            {
                if (s.IsEmpty || s.item == currency || y > r.yMax - 30) continue;
                if (GUI.Button(new Rect(r.x + 12, y, 336, 22), s.item.itemName + " x" + s.count + "   →   +" + Mathf.Max(1, s.item.value / 2) + "g"))
                {
                    _customer.Remove(s.item, 1);
                    if (currency != null) _customer.Add(currency, Mathf.Max(1, s.item.value / 2));
                    break;
                }
                y += 24;
            }
            if (GUI.Button(new Rect(r.x + r.width - 70, r.yMax - 28, 58, 22), "Close")) _open = false;
        }
    }
}
