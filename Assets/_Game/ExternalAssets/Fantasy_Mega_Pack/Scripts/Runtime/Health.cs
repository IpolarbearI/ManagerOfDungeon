// Health.cs - HP, damage, hurt/death hooks. Works with the pack's Animator (Hurt/Die triggers).
using UnityEngine;
using UnityEngine.Events;

namespace FantasyDungeonPixelPack
{
    [RequireComponent(typeof(Animator))]
    /// <summary>
    /// Damageable health with hurt/death animation triggers, onHurt/onDeath events, i-frame support (invulnerable) and floating damage numbers. Add to players, enemies or props.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Health")]
        public int maxHealth = 30;
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        [Tooltip("Set true during dodge-roll i-frames to ignore damage.")]
        public bool invulnerable = false;

        [Header("Events")]
        public UnityEvent onHurt;
        public UnityEvent onDeath;

        Animator _anim;

        void Awake()
        {
            _anim = GetComponent<Animator>();
            CurrentHealth = maxHealth;
            // UnityEvents are null when Health is added via AddComponent at runtime
            if (onHurt == null) onHurt = new UnityEvent();
            if (onDeath == null) onDeath = new UnityEvent();
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || invulnerable || amount <= 0) return;
            var eq = GetComponent<EquipmentSystem>();
            if (eq != null) amount = Mathf.Max(1, amount - eq.TotalDefenseBonus);
            FloatingDamageNumbers.Show(transform.position, amount, CompareTag("Player") ? new Color(1f, 0.4f, 0.3f) : new Color(1f, 0.85f, 0.2f));
            CurrentHealth -= amount;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }
            else
            {
                _anim.SetTrigger("Hurt");
                FlashWhite();
                AudioManager.PlaySFX("hit");
                onHurt?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        }

        /// <summary>Sets current HP directly (clamped 1..maxHealth). Used by SaveSystem on load.</summary>
        public void SetHealth(int value)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Clamp(value, 1, maxHealth);
        }

        SpriteRenderer _flashSr;
        Color _flashOrig;
        Coroutine _flashCo;

        void FlashWhite()
        {
            if (_flashSr == null)
            {
                _flashSr = GetComponent<SpriteRenderer>();
                if (_flashSr == null) return;
                _flashOrig = _flashSr.color;
            }
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = StartCoroutine(FlashRoutine());
        }

        System.Collections.IEnumerator FlashRoutine()
        {
            _flashSr.color = new Color(1f, 1f, 1f, 1f) * 1.6f;
            yield return new WaitForSeconds(0.08f);
            _flashSr.color = _flashOrig;
        }

        void Die()
        {
            IsDead = true;
            _anim.SetTrigger("Die");
            onDeath?.Invoke();
            var rb = GetComponent<Rigidbody2D>();
            if (rb) rb.linearVelocity = Vector2.zero;
            var col = GetComponent<Collider2D>();
            if (col) col.enabled = false;
            var ai = GetComponent<EnemyAI>();
            if (ai) ai.enabled = false;
            Destroy(gameObject, 2f);
        }
    }
}
