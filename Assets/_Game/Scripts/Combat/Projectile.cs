using UnityEngine;

namespace ManagerOfDungeon
{
    public sealed class Projectile : MonoBehaviour
    {
        CombatDirector _combat;
        CombatUnit _owner;
        CombatUnit _target;
        float _damage;
        float _speed;

        public void Launch(CombatDirector combat, CombatUnit owner, CombatUnit target, float damage, float speed)
        {
            _combat = combat;
            _owner = owner;
            _target = target;
            _damage = damage;
            _speed = speed;
            name = ResourceIds.ObjArrow;
        }

        void Update()
        {
            if (!Application.isPlaying || _combat == null || _owner == null || !_owner.IsAlive || _target == null || !_target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 toTarget = _target.transform.position - transform.position;
            toTarget.z = 0f;
            float step = _speed * Time.deltaTime;
            if (toTarget.magnitude <= CombatRanges.ProjectileHitRadius || toTarget.magnitude <= step)
            {
                _target.TakeHit(_damage);
                Destroy(gameObject);
                return;
            }

            float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            transform.position += toTarget.normalized * step;
        }
    }
}
