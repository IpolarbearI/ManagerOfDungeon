// BossEnrage.cs - turns any enemy into a boss: more HP, enrage phase at low health, optional minion summon.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Boss behaviour add-on for EnemyAI + Health: scales up HP, enters an enrage phase
    /// below a health threshold (faster + stronger + red tint) and can summon minions.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyAI))]
    public class BossEnrage : MonoBehaviour
    {
        [Header("Boss")]
        public int bossHealth = 120;
        [Range(0.1f, 0.9f)] public float enrageAt = 0.4f;
        public float enrageSpeedMultiplier = 1.6f;
        public int enrageDamageBonus = 4;

        [Header("Minions (optional)")]
        public GameObject minionPrefab;
        public int minionCount = 2;

        Health _health;
        EnemyAI _ai;
        SpriteRenderer _sr;
        bool _enraged;

        void Start()
        {
            _health = GetComponent<Health>();
            _ai = GetComponent<EnemyAI>();
            _sr = GetComponent<SpriteRenderer>();
            _health.maxHealth = bossHealth;
            _health.SetHealth(bossHealth);
            transform.localScale = transform.localScale * 1.25f;
        }

        void Update()
        {
            if (_enraged || _health.IsDead) return;
            if ((float)_health.CurrentHealth / _health.maxHealth <= enrageAt) Enrage();
        }

        void Enrage()
        {
            _enraged = true;
            _ai.moveSpeed *= enrageSpeedMultiplier;
            _ai.damage += enrageDamageBonus;
            _ai.attackCooldown *= 0.7f;
            if (_sr != null) _sr.color = new Color(1f, 0.55f, 0.55f);
            for (int i = 0; i < minionCount; i++)
            {
                if (minionPrefab == null) break;
                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle.normalized * 1.2f);
                var m = Instantiate(minionPrefab, pos, Quaternion.identity);
                m.tag = "Enemy";
                if (m.GetComponent<EnemyAI>() == null) m.AddComponent<EnemyAI>();
            }
        }
    }
}
