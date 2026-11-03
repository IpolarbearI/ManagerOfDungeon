// ScreenShake.cs - camera shake on demand; auto-hooks the player's Health for hit feedback.
using System.Collections;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Camera shake for combat juice. Put on the main camera. Call ScreenShake.Shake(0.15f, 0.2f)
    /// from anywhere, or let it auto-shake when the Player gets hurt (autoHookPlayer).
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Instance;

        [Tooltip("Automatically shake when the Player's Health fires onHurt.")]
        public bool autoHookPlayer = true;
        public float hitDuration = 0.18f;
        public float hitStrength = 0.12f;

        Vector3 _offset;

        void Awake() { Instance = this; }

        void Start()
        {
            if (!autoHookPlayer) return;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                var h = p.GetComponent<Health>();
                if (h != null) h.onHurt.AddListener(() => Shake(hitDuration, hitStrength));
            }
        }

        /// <summary>Shakes the camera for duration seconds with the given strength (world units).</summary>
        public static void Shake(float duration, float strength)
        {
            if (Instance != null) Instance.StartCoroutine(Instance.DoShake(duration, strength));
        }

        IEnumerator DoShake(float duration, float strength)
        {
            float t = 0f;
            while (t < duration)
            {
                _offset = (Vector3)(Random.insideUnitCircle * strength);
                transform.position += _offset;
                yield return null;
                transform.position -= _offset;
                t += Time.deltaTime;
            }
        }
    }
}
