// SceneFader.cs - full-screen fade in/out + scene transitions. Self-contained (builds its own canvas).
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace FantasyDungeonPixelPack
{
    /// <summary>Screen fader for scene transitions. Use SceneFader.FadeToScene("Name") or FadeIn/FadeOut.</summary>
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }
        public float defaultDuration = 0.6f;

        Image _img;
        Canvas _canvas;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            var cgo = new GameObject("FaderCanvas");
            cgo.transform.SetParent(transform, false);
            _canvas = cgo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;
            var igo = new GameObject("Black");
            igo.transform.SetParent(cgo.transform, false);
            _img = igo.AddComponent<Image>();
            _img.color = new Color(0, 0, 0, 0);
            _img.raycastTarget = false;
            var rt = _img.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        public static SceneFader Ensure()
        {
            if (Instance == null) new GameObject("SceneFader").AddComponent<SceneFader>();
            return Instance;
        }

        /// <summary>Fade to black, load scene, fade back in.</summary>
        public static void FadeToScene(string sceneName, float duration = -1f)
        {
            Ensure().StartCoroutine(Instance.FadeLoadRoutine(sceneName, duration > 0 ? duration : Instance.defaultDuration));
        }

        public static Coroutine FadeOut(float duration = -1f)
        {
            var f = Ensure();
            return f.StartCoroutine(f.FadeRoutine(1f, duration > 0 ? duration : f.defaultDuration));
        }

        public static Coroutine FadeIn(float duration = -1f)
        {
            var f = Ensure();
            return f.StartCoroutine(f.FadeRoutine(0f, duration > 0 ? duration : f.defaultDuration));
        }

        IEnumerator FadeLoadRoutine(string sceneName, float dur)
        {
            yield return FadeRoutine(1f, dur);
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return FadeRoutine(0f, dur);
        }

        IEnumerator FadeRoutine(float target, float dur)
        {
            float start = _img.color.a;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(start, target, t / dur);
                _img.color = new Color(0, 0, 0, a);
                yield return null;
            }
            _img.color = new Color(0, 0, 0, target);
        }
    }
}
