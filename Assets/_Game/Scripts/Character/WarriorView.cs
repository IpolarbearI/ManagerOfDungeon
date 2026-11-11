using UnityEngine;

namespace ManagerOfDungeon
{
    public sealed class WarriorView : MonoBehaviour
    {
        Animator _animator;
        SpriteRenderer _spriteRenderer;
        int _attackIndex = 1;

        public static WarriorView Spawn(Transform parent, Vector3 worldPosition)
        {
            var prefab = LoadPrefab();
            if (prefab == null)
                return null;

            var instance = Instantiate(prefab, worldPosition, Quaternion.identity, parent);
            instance.name = "Warrior";
            StripDemoComponents(instance);
            instance.transform.localScale = new Vector3(0.575f, 0.575f, 1f);

            var view = instance.GetComponent<WarriorView>();
            if (view == null)
                view = instance.AddComponent<WarriorView>();
            view.Bind();
            view.SetMoving(false);
            return view;
        }

        public void SetMoving(bool moving)
        {
            if (_animator != null)
                _animator.SetInteger("AnimState", moving ? 1 : 0);
        }

        public void Face(float directionX)
        {
            if (_spriteRenderer == null || Mathf.Abs(directionX) < 0.01f)
                return;

            _spriteRenderer.flipX = directionX < 0f;
        }

        public void PlayAttack()
        {
            if (_animator == null)
                return;

            _animator.SetTrigger("Attack" + _attackIndex);
            _attackIndex++;
            if (_attackIndex > 3)
                _attackIndex = 1;
        }

        public void PlayHurt()
        {
            if (_animator != null)
                _animator.SetTrigger("Hurt");
        }

        public void PlayDeath()
        {
            if (_animator != null)
                _animator.SetTrigger("Death");
        }

        public void ResetToIdle(bool faceRight)
        {
            if (_animator != null)
            {
                _animator.ResetTrigger("Death");
                _animator.ResetTrigger("Hurt");
                _animator.ResetTrigger("Attack1");
                _animator.ResetTrigger("Attack2");
                _animator.ResetTrigger("Attack3");
                _animator.SetBool("Grounded", true);
                _animator.SetInteger("AnimState", 0);
                int idle = Animator.StringToHash("Idle");
                if (_animator.HasState(0, idle))
                    _animator.Play(idle, 0, 0f);
            }

            Face(faceRight ? 1f : -1f);
        }

        void Bind()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
                _spriteRenderer.sortingOrder = 20;

            if (_animator != null)
            {
                _animator.SetBool("Grounded", true);
                _animator.SetInteger("AnimState", 0);
            }
        }

        static GameObject LoadPrefab()
        {
            return PackAssets.LoadPrefab(PackAssets.WarriorPrefab);
        }

        static void StripDemoComponents(GameObject instance)
        {
            var demo = instance.GetComponent<HeroKnight>();
            if (demo != null)
                demo.enabled = false;

            var body = instance.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.bodyType = RigidbodyType2D.Kinematic;
                body.gravityScale = 0f;
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            var colliders = instance.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
        }
    }
}
