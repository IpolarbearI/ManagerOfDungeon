// PlayerCombat.cs - melee attack on Space; damages Health components in front of the player.
// Works with both legacy Input Manager and the new Input System.
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    [RequireComponent(typeof(Animator))]
    /// <summary>
    /// Melee attack on Space: plays the Attack animation and damages every Health in an arc in front of the player.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combat")]
        public int damage = 12;
        public float attackRadius = 1.1f;
        public float reach = 0.6f;

        Animator _anim;

        void Awake() { _anim = GetComponent<Animator>(); }

        void Update()
        {
            if (AttackPressed()) DoAttack();
        }

        void DoAttack()
        {
            _anim.SetTrigger("Attack");
            AudioManager.PlaySFX("sword");
            int dmg = damage;
            var eq = GetComponent<EquipmentSystem>();
            if (eq != null) dmg += eq.TotalAttackBonus;
            Vector2 facing = new Vector2(_anim.GetFloat("MoveX"), _anim.GetFloat("MoveY"));
            if (facing.sqrMagnitude < 0.01f) facing = Vector2.down;
            Vector2 center = (Vector2)transform.position + Vector2.up * 0.3f + facing.normalized * reach;
            var hits = Physics2D.OverlapCircleAll(center, attackRadius);
            foreach (var h in hits)
            {
                if (h.gameObject == gameObject) continue;
                var hp = h.GetComponent<Health>();
                if (hp != null && !hp.IsDead)
                {
                    var kb = h.GetComponent<Knockback>();
                    if (kb != null) kb.ApplyFrom(transform.position);
                    hp.TakeDamage(dmg);
                }
            }
        }

        bool AttackPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }
    }
}
