// DamageZone.cs - hurts anything with Health that stays inside (lava, spikes, poison pools).
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Hazard area: damages any Health that stays inside the trigger (lava, spikes, poison).
    /// Add to an object with a trigger Collider2D.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DamageZone : MonoBehaviour
    {
        public int damage = 5;
        public float interval = 0.7f;
        [Tooltip("Only damage the player (ignore enemies).")]
        public bool playerOnly = true;

        float _t;

        void OnTriggerStay2D(Collider2D other)
        {
            if (playerOnly && !other.CompareTag("Player")) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;
            var h = other.GetComponent<Health>();
            if (h != null && !h.IsDead)
            {
                h.TakeDamage(damage);
                _t = interval;
            }
        }
    }
}
