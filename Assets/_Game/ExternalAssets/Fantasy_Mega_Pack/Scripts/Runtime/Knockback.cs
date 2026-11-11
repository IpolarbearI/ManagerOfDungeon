// Knockback.cs - combat-feel add-on: pushes this object away from damage sources and flashes the sprite.
using System.Collections;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Combat juice: when this object's Health takes damage, it gets pushed away from the
    /// attacker and its sprite flashes white. Call ApplyFrom(position) or let PlayerCombat/EnemyAI
    /// trigger it automatically via Health.onHurt.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Knockback : MonoBehaviour
    {
        public float force = 4.5f;
        public float duration = 0.12f;
        public bool flashWhite = true;

        Rigidbody2D _rb;
        SpriteRenderer _sr;
        Color _orig;
        Vector2 _lastSourcePos;
        bool _hasSource;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _orig = _sr.color;
            var h = GetComponent<Health>();
            if (h != null) h.onHurt.AddListener(OnHurt);
        }

        /// <summary>Sets where the hit came from (call before damage for directional knockback).</summary>
        public void ApplyFrom(Vector2 sourcePosition)
        {
            _lastSourcePos = sourcePosition;
            _hasSource = true;
        }

        void OnHurt()
        {
            Vector2 dir = _hasSource
                ? ((Vector2)transform.position - _lastSourcePos).normalized
                : Random.insideUnitCircle.normalized;
            StopAllCoroutines();
            StartCoroutine(Push(dir));
        }

        IEnumerator Push(Vector2 dir)
        {
            float t = 0f;
            if (_sr != null && flashWhite) _sr.color = Color.white * 1.5f;
            while (t < duration)
            {
                _rb.linearVelocity = dir * force;
                t += Time.deltaTime;
                yield return null;
            }
            _rb.linearVelocity = Vector2.zero;
            if (_sr != null) _sr.color = _orig;
            _hasSource = false;
        }
    }
}
