// EnemyAI.cs - simple top-down enemy: idle -> chase player -> attack. Drives the pack Animator.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    /// <summary>
    /// Simple top-down enemy brain: idle until the Player is in chaseRange, chase, then attack on cooldown within attackRange. Requires the target to be tagged 'Player'.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI")]
        public float moveSpeed = 1.5f;
        public float chaseRange = 4.5f;
        public float attackRange = 0.95f;
        public float attackCooldown = 1.2f;
        public int damage = 5;

        Transform _player;
        Health _playerHealth;
        Rigidbody2D _rb;
        Animator _anim;
        Vector2 _facing = Vector2.down;
        float _cd;

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) { _player = p.transform; _playerHealth = p.GetComponent<Health>(); }
        }

        void Update()
        {
            if (_player == null) { SetAnim(_facing, 0f); _rb.linearVelocity = Vector2.zero; return; }
            float dist = Vector2.Distance(transform.position, _player.position);
            Vector2 dir = ((Vector2)(_player.position - transform.position)).normalized;

            if (dist <= attackRange)
            {
                _rb.linearVelocity = Vector2.zero;
                SetAnim(dir, 0f);
                _cd -= Time.deltaTime;
                if (_cd <= 0f)
                {
                    _cd = attackCooldown;
                    _anim.SetTrigger("Attack");
                    if (_playerHealth != null && !_playerHealth.IsDead) _playerHealth.TakeDamage(damage);
                }
            }
            else if (dist <= chaseRange)
            {
                _facing = dir;
                _rb.linearVelocity = dir * moveSpeed;
                SetAnim(dir, 1f);
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
                SetAnim(_facing, 0f);
            }
        }

        void SetAnim(Vector2 f, float speed)
        {
            _anim.SetFloat("MoveX", f.x);
            _anim.SetFloat("MoveY", f.y);
            _anim.SetFloat("Speed", speed);
        }
    }
}
