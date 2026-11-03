// SpeechBubble.cs - world-space speech bubble above a character's head. Self-contained.
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Small comic-style speech bubble above a transform. Use SpeechBubble.Say(target, "text", 2.5f).
    /// For ambient NPC quips; use DialogueBoxUI for full story conversations.
    /// </summary>
    public class SpeechBubble : MonoBehaviour
    {
        public Transform follow;
        public float yOffset = 1.4f;

        Canvas _canvas;
        Text _text;
        Image _bg;

        public static SpeechBubble Say(Transform target, string text, float duration = 2.5f)
        {
            var go = new GameObject("SpeechBubble");
            var sb = go.AddComponent<SpeechBubble>();
            sb.follow = target;
            sb.Build(text);
            sb.StartCoroutine(sb.LifeRoutine(duration));
            return sb;
        }

        void Build(string text)
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.sortingOrder = 500;
            var rt = _canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220, 70);
            transform.localScale = Vector3.one * 0.012f;

            var bgo = new GameObject("BG");
            bgo.transform.SetParent(transform, false);
            _bg = bgo.AddComponent<Image>();
            _bg.color = new Color(0.08f, 0.07f, 0.11f, 0.94f);
            var frame = bgo.AddComponent<UnityEngine.UI.Outline>();
            frame.effectColor = new Color(0.85f, 0.7f, 0.35f, 0.95f);
            frame.effectDistance = new Vector2(2f, -2f);
            var brt = _bg.rectTransform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

            var tgo = new GameObject("Text");
            tgo.transform.SetParent(bgo.transform, false);
            _text = tgo.AddComponent<Text>();
            _text.font = GameFonts.UI;
            _text.fontSize = 22;
            _text.fontStyle = FontStyle.Bold;
            _text.color = new Color(0.95f, 0.92f, 0.82f);
            _text.alignment = TextAnchor.MiddleCenter;
            _text.horizontalOverflow = HorizontalWrapMode.Wrap;
            _text.verticalOverflow = VerticalWrapMode.Overflow;
            _text.text = text;
            var trt = _text.rectTransform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10, 6); trt.offsetMax = new Vector2(-10, -6);

            // size bubble to text
            float w = Mathf.Clamp(text.Length * 11f + 30f, 90f, 260f);
            float h = text.Length > 22 ? 92f : 60f;
            rt.sizeDelta = new Vector2(w, h);

            // little tail
            var tail = new GameObject("Tail");
            tail.transform.SetParent(transform, false);
            var timg = tail.AddComponent<Image>();
            timg.color = _bg.color;
            var trt2 = timg.rectTransform;
            trt2.sizeDelta = new Vector2(16, 16);
            trt2.anchorMin = new Vector2(0.5f, 0f); trt2.anchorMax = new Vector2(0.5f, 0f);
            trt2.anchoredPosition = new Vector2(0, -6);
            trt2.localRotation = Quaternion.Euler(0, 0, 45);
        }

        void LateUpdate()
        {
            if (follow == null) { Destroy(gameObject); return; }
            transform.position = follow.position + Vector3.up * yOffset;
        }

        IEnumerator LifeRoutine(float duration)
        {
            // pop in
            Vector3 full = transform.localScale;
            transform.localScale = full * 0.3f;
            float t = 0f;
            while (t < 0.15f) { t += Time.deltaTime; transform.localScale = full * Mathf.Lerp(0.3f, 1f, t / 0.15f); yield return null; }
            transform.localScale = full;
            yield return new WaitForSeconds(duration);
            t = 0f;
            while (t < 0.12f) { t += Time.deltaTime; transform.localScale = full * Mathf.Lerp(1f, 0f, t / 0.12f); yield return null; }
            Destroy(gameObject);
        }
    }
}
