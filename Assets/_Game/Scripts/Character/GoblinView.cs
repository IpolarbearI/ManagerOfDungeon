using UnityEngine;

namespace ManagerOfDungeon
{
    public sealed class GoblinView : MonoBehaviour
    {
        Animator _animator;
        SpriteRenderer _spriteRenderer;
        SpriteRenderer _bowRenderer;
        Vector3 _facing = Vector3.left;
        Vector3 _homeFacing = Vector3.left;

        public static GoblinView Spawn(Transform parent, Vector3 worldPosition)
        {
            return Spawn(
                parent,
                worldPosition,
                PackAssets.GoblinPrefab,
                PackAssets.GoblinIdle,
                ResourceIds.CharGoblinIdle,
                0.725f,
                Vector3.left);
        }

        public static GoblinView SpawnArcher(Transform parent, Vector3 worldPosition)
        {
            return Spawn(
                parent,
                worldPosition,
                PackAssets.ArcherPrefab,
                PackAssets.ArcherIdle,
                ResourceIds.CharHeroArcher,
                0.825f,
                Vector3.right);
        }

        static GoblinView Spawn(
            Transform parent,
            Vector3 worldPosition,
            string prefabPath,
            string idleSpritePath,
            string objectName,
            float scale,
            Vector3 facing)
        {
            var prefab = PackAssets.LoadPrefab(prefabPath);
            if (prefab == null)
                return null;

            var instance = Object.Instantiate(prefab, worldPosition, Quaternion.identity, parent);
            instance.name = objectName;
            instance.transform.localScale = new Vector3(scale, scale, 1f);
            DisablePhysics(instance);

            var view = instance.GetComponent<GoblinView>();
            if (view == null)
                view = instance.AddComponent<GoblinView>();
            view.Bind(idleSpritePath);
            view._homeFacing = facing.sqrMagnitude > 0.0001f ? facing.normalized : Vector3.left;
            view.ResetToIdle(view._homeFacing);
            if (prefabPath == PackAssets.ArcherPrefab)
                view.ShowBow(true);
            return view;
        }

        public void ResetToIdle()
        {
            ResetToIdle(_homeFacing);
        }

        public void ResetToIdle(Vector3 facing)
        {
            if (_animator != null)
            {
                ResetTriggerIfExists("Attack");
                ResetTriggerIfExists("Hurt");
                ResetTriggerIfExists("Die");
                _animator.SetBool("IsRunning", false);
                _animator.SetFloat("Speed", 0f);
                int idle = Animator.StringToHash("Idle");
                if (_animator.HasState(0, idle))
                    _animator.Play(idle, 0, 0f);
                else
                    _animator.Play(0, 0, 0f);
            }

            Vector3 dir = facing.sqrMagnitude > 0.0001f ? facing : _homeFacing;
            Face(dir);
            if (_animator != null)
            {
                _animator.SetFloat("Speed", 0f);
                _animator.SetBool("IsRunning", false);
            }

            UpdateHeldBow();
        }

        public void SetMoving(bool moving, Vector3 direction)
        {
            if (_animator == null)
                return;

            Face(direction);
            _animator.SetFloat("Speed", moving ? 1f : 0f);
            _animator.SetBool("IsRunning", moving);
        }

        public void Face(Vector3 direction)
        {
            if (_animator == null)
                return;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Vector3 n = direction.normalized;
            _facing = n;
            _animator.SetFloat("MoveX", n.x);
            _animator.SetFloat("MoveY", n.y);
            UpdateHeldBow();
        }

        public void PlayAttack()
        {
            SetTriggerIfExists("Attack");
        }

        public void PlayHurt()
        {
            SetTriggerIfExists("Hurt");
        }

        public void PlayDeath()
        {
            SetTriggerIfExists("Die");
        }

        public void ShowBow(bool visible)
        {
            EnsureBow();
            if (_bowRenderer != null)
                _bowRenderer.enabled = visible;
            if (visible)
                UpdateHeldBow();
        }

        void Bind(string idleSpritePath)
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                if (_spriteRenderer.sprite == null)
                    _spriteRenderer.sprite = PackAssets.LoadSprite(idleSpritePath);
                PackAssets.ApplySpriteMaterial(_spriteRenderer);
                _spriteRenderer.sortingOrder = 20;
            }

            if (_animator != null)
            {
                _animator.SetFloat("Speed", 0f);
                _animator.SetBool("IsRunning", false);
                _animator.SetFloat("MoveX", -1f);
                _animator.SetFloat("MoveY", 0f);
            }
        }

        void EnsureBow()
        {
            if (_bowRenderer != null)
                return;

            var sprite = PackAssets.LoadSprite(PackAssets.HeldBow);
            if (sprite == null)
                sprite = PackAssets.LoadSprite(PackAssets.OldWoodBow);
            if (sprite == null)
                return;

            var go = new GameObject(ResourceIds.ObjOldWoodBow);
            go.transform.SetParent(transform, false);
            _bowRenderer = go.AddComponent<SpriteRenderer>();
            _bowRenderer.sprite = sprite;
            PackAssets.ApplySpriteMaterial(_bowRenderer);
            _bowRenderer.sortingOrder = 21;
            _bowRenderer.enabled = false;
            UpdateHeldBow();
        }

        void UpdateHeldBow()
        {
            if (_bowRenderer == null)
                return;

            Transform bow = _bowRenderer.transform;
            float ax = Mathf.Abs(_facing.x);
            float ay = Mathf.Abs(_facing.y);

            if (ay > ax && _facing.y > 0f)
            {
                bow.localPosition = new Vector3(-0.02f, 0.42f, 0f);
                bow.localRotation = Quaternion.Euler(0f, 0f, 12f);
                bow.localScale = new Vector3(0.34f, 0.34f, 1f);
                _bowRenderer.sortingOrder = 19;
                return;
            }

            if (ay > ax)
            {
                bow.localPosition = new Vector3(0.08f, 0.30f, 0f);
                bow.localRotation = Quaternion.Euler(0f, 0f, -12f);
                bow.localScale = new Vector3(0.36f, 0.36f, 1f);
                _bowRenderer.sortingOrder = 21;
                return;
            }

            if (_facing.x >= 0f)
            {
                bow.localPosition = new Vector3(0.13f, 0.36f, 0f);
                bow.localRotation = Quaternion.Euler(0f, 0f, -22f);
                bow.localScale = new Vector3(0.38f, 0.38f, 1f);
            }
            else
            {
                bow.localPosition = new Vector3(-0.13f, 0.36f, 0f);
                bow.localRotation = Quaternion.Euler(0f, 0f, 22f);
                bow.localScale = new Vector3(-0.38f, 0.38f, 1f);
            }

            _bowRenderer.sortingOrder = 21;
        }

        void SetTriggerIfExists(string parameterName)
        {
            if (!HasParameter(parameterName))
                return;

            _animator.SetTrigger(parameterName);
        }

        void ResetTriggerIfExists(string parameterName)
        {
            if (!HasParameter(parameterName))
                return;

            _animator.ResetTrigger(parameterName);
        }

        bool HasParameter(string parameterName)
        {
            if (_animator == null)
                return false;

            var parameters = _animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == parameterName)
                    return true;
            }

            return false;
        }

        static void DisablePhysics(GameObject instance)
        {
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
