// StatusEffects.cs - poison/burn/slow status effects with tinting. Pairs with the elite enemy variants.
using System.Collections;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Status-effect receiver: ApplyPoison/ApplyBurn (damage over time) and ApplySlow.
    /// Tints the sprite while active. Add to anything with Health.
    /// Use EffectOnHit on an attacker (e.g. Toxic Bat) to inflict effects automatically.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class StatusEffects : MonoBehaviour
    {
        Health _health;
        SpriteRenderer _sr;
        TopDownCharacterController _move;
        EnemyAI _ai;
        Color _orig;
        int _active;

        void Awake()
        {
            _health = GetComponent<Health>();
            _sr = GetComponent<SpriteRenderer>();
            _move = GetComponent<TopDownCharacterController>();
            _ai = GetComponent<EnemyAI>();
            if (_sr != null) _orig = _sr.color;
        }

        public void ApplyPoison(int ticks = 4, int damagePerTick = 3) =>
            StartCoroutine(Dot(ticks, damagePerTick, new Color(0.55f, 1f, 0.45f)));

        public void ApplyBurn(int ticks = 3, int damagePerTick = 5) =>
            StartCoroutine(Dot(ticks, damagePerTick, new Color(1f, 0.6f, 0.3f)));

        public void ApplySlow(float duration = 2.5f, float factor = 0.5f) =>
            StartCoroutine(Slow(duration, factor, new Color(0.55f, 0.8f, 1f)));

        IEnumerator Dot(int ticks, int dmg, Color tint)
        {
            _active++;
            for (int i = 0; i < ticks; i++)
            {
                if (_health.IsDead) break;
                if (_sr != null) _sr.color = tint;
                _health.TakeDamage(dmg);
                yield return new WaitForSeconds(0.9f);
            }
            if (--_active <= 0 && _sr != null) _sr.color = _orig;
        }

        IEnumerator Slow(float duration, float factor, Color tint)
        {
            _active++;
            float oldAi = _ai != null ? _ai.moveSpeed : 0f;
            if (_ai != null) _ai.moveSpeed *= factor;
            if (_sr != null) _sr.color = tint;
            yield return new WaitForSeconds(duration);
            if (_ai != null) _ai.moveSpeed = oldAi;
            if (--_active <= 0 && _sr != null) _sr.color = _orig;
        }
    }

    /// <summary>
    /// Put on an enemy: every successful EnemyAI attack also inflicts the chosen status effect
    /// on the player (poison for toxic enemies, burn for fire, slow for frost).
    /// </summary>
    public class EffectOnHit : MonoBehaviour
    {
        public enum Kind { Poison, Burn, Slow }
        public Kind effect = Kind.Poison;

        void Start()
        {
            var ai = GetComponent<EnemyAI>();
            if (ai == null) return;
            StartCoroutine(Watch(ai));
        }

        System.Collections.IEnumerator Watch(EnemyAI ai)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) yield break;
            var h = player.GetComponent<Health>();
            var fx = player.GetComponent<StatusEffects>();
            if (h == null || fx == null) yield break;
            int lastHp = h.CurrentHealth;
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                if (h.IsDead) yield break;
                // if the player lost HP while close to us, we likely hit them
                if (h.CurrentHealth < lastHp &&
                    Vector2.Distance(transform.position, player.transform.position) < ai.attackRange + 0.4f)
                {
                    switch (effect)
                    {
                        case Kind.Poison: fx.ApplyPoison(); break;
                        case Kind.Burn: fx.ApplyBurn(); break;
                        case Kind.Slow: fx.ApplySlow(); break;
                    }
                }
                lastHp = h.CurrentHealth;
            }
        }
    }
}
