// TorchFlicker.cs - flickering glow for torches/braziers (sprite alpha+scale pulse, no lighting package needed).
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Cozy torch flicker: pulses the SpriteRenderer's alpha and scale with layered noise.
    /// Put on a glow sprite (see Sprites/VFX/glow_warm.png) placed over a torch/candle/brazier.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TorchFlicker : MonoBehaviour
    {
        [Range(0f, 1f)] public float baseAlpha = 0.55f;
        public float flickerAmount = 0.25f;
        public float speed = 7f;
        public float scaleAmount = 0.08f;

        SpriteRenderer _sr;
        Vector3 _baseScale;
        float _seed;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            _seed = Random.value * 100f;
        }

        void Update()
        {
            float n = Mathf.PerlinNoise(_seed, Time.time * speed) * 0.7f
                    + Mathf.PerlinNoise(_seed + 31f, Time.time * speed * 2.3f) * 0.3f;
            var c = _sr.color;
            c.a = Mathf.Clamp01(baseAlpha + (n - 0.5f) * 2f * flickerAmount);
            _sr.color = c;
            transform.localScale = _baseScale * (1f + (n - 0.5f) * 2f * scaleAmount);
        }
    }
}
