// HealthPickup.cs - walk-over heart/potion that heals the player.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Walk-over healing pickup: heals the Player's Health on contact, then despawns.
    /// Add a small trigger Collider2D + SpriteRenderer (use any potion icon from UI/Icons).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HealthPickup : MonoBehaviour
    {
        public int healAmount = 20;
        public float bobAmplitude = 0.06f;
        public float bobSpeed = 3f;

        Vector3 _start;

        void Start() { _start = transform.position; }

        void Update()
        {
            transform.position = _start + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            AudioManager.PlaySFX("potion");
            if (!other.CompareTag("Player")) return;
            var h = other.GetComponent<Health>();
            if (h == null || h.IsDead) return;
            h.Heal(healAmount);
            FloatingDamageNumbers.Show(other.transform.position, healAmount, new Color(0.4f, 1f, 0.5f));
            Destroy(gameObject);
        }
    }
}
