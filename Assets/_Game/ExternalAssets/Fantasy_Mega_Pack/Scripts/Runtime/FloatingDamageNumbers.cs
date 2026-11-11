// FloatingDamageNumbers.cs - pooled-free floating combat text (no TextMeshPro dependency, uses built-in TextMesh).
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Static helper: FloatingDamageNumbers.Show(position, amount, color) spawns a rising, fading damage number (TextMesh, no dependencies).
    /// </summary>
    public static class FloatingDamageNumbers
    {
        public static void Show(Vector3 worldPos, int amount, Color color)
        {
            var go = new GameObject("DamageText");
            go.transform.position = worldPos + Vector3.up * 1.0f;
            var tm = go.AddComponent<TextMesh>();
            tm.text = amount.ToString();
            tm.color = color;
            tm.fontSize = 48;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 200;
            go.AddComponent<FloatAway>();
        }
    }

    /// <summary>
    /// Internal mover for damage numbers: floats up, fades out, self-destructs.
    /// </summary>
    public class FloatAway : MonoBehaviour
    {
        public float life = 0.8f;
        Vector3 _vel = new Vector3(0f, 1.4f, 0f);
        float _t;
        TextMesh _tm;

        void Start() { _tm = GetComponent<TextMesh>(); }

        void Update()
        {
            _t += Time.deltaTime;
            transform.position += _vel * Time.deltaTime;
            _vel.y -= 2.2f * Time.deltaTime;
            if (_tm != null) { var c = _tm.color; c.a = Mathf.Clamp01(1f - _t / life); _tm.color = c; }
            if (_t >= life) Destroy(gameObject);
        }
    }
}
