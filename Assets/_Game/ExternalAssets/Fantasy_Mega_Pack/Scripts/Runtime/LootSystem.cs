// LootSystem.cs - weighted loot tables + drop-on-death + walk-over pickups.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Drops loot from a LootTableSO as walk-over pickups when this object's Health dies.
    /// </summary>
    public class LootDropper : MonoBehaviour
    {
        public LootTableSO table;

        void Start()
        {
            var h = GetComponent<Health>();
            if (h != null) h.onDeath.AddListener(DropNow);
        }

        public void DropNow()
        {
            if (table == null) return;
            foreach (var kv in table.Roll()) SpawnPickup(kv.Key, kv.Value);
        }

        void SpawnPickup(ItemSO item, int amount)
        {
            var go = new GameObject("Pickup_" + item.itemName);
            go.transform.position = transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.4f);
            go.transform.localScale = Vector3.one * 0.5f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = item.icon; sr.sortingOrder = 8;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true; col.radius = 0.6f;
            var p = go.AddComponent<Pickup>();
            p.item = item; p.amount = amount;
            p.Burst(transform.position);
        }
    }

    /// <summary>
    /// Walk-over item pickup: adds its ItemSO to the Player's InventorySystem on trigger contact.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        public ItemSO item;
        public int amount = 1;
        Vector3 _burstTarget;
        float _burstT = -1f;
        Vector3 _baseScale;

        /// <summary>Juicy spawn: item flies outward from origin and settles with a scale pop.</summary>
        public void Burst(Vector3 origin)
        {
            _baseScale = transform.localScale;
            _burstTarget = transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(0.5f, 1.1f));
            transform.position = origin;
            transform.localScale = _baseScale * 0.2f;
            _burstT = 0f;
        }

        void Update()
        {
            if (_burstT < 0f) return;
            _burstT += Time.deltaTime * 3.5f;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Min(_burstT, 1f));
            transform.position = Vector3.Lerp(transform.position, _burstTarget, k * 0.25f);
            float pop = _burstT < 0.7f ? Mathf.Lerp(0.2f, 1.25f, _burstT / 0.7f) : Mathf.Lerp(1.25f, 1f, (_burstT - 0.7f) / 0.3f);
            transform.localScale = _baseScale * pop;
            if (_burstT >= 1f) { transform.localScale = _baseScale; _burstT = -1f; }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_burstT >= 0f && _burstT < 0.45f) return; // don't vacuum mid-burst
            if (!other.CompareTag("Player")) return;
            var inv = other.GetComponent<InventorySystem>();
            if (inv != null && inv.Add(item, amount))
            {
                AudioManager.PlaySFX("coin");
                Destroy(gameObject);
            }
        }
    }
}
