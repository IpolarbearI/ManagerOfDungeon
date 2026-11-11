// DeathScreen.cs - hooks the player's death, shows YOU DIED + respawn button (reload scene).
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace FantasyDungeonPixelPack
{
    /// <summary>Listens for player death and shows a death screen with Respawn / Quit options.</summary>
    public class DeathScreen : MonoBehaviour
    {
        public string titleSceneName = "TitleScreen";
        GameObject _root;
        bool _hooked;

        void Awake() { BuildUI(); _root.SetActive(false); }

        void Update()
        {
            if (_hooked) return;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            var hp = p.GetComponent<Health>();
            if (hp == null) return;
            hp.onDeath.AddListener(OnPlayerDeath);
            _hooked = true;
        }

        void OnPlayerDeath() { StartCoroutine(ShowAfterDelay()); }

        IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSeconds(1.4f);
            AudioManager.StopBGM();
            _root.SetActive(true);
        }

        void Respawn()
        {
            _root.SetActive(false);
            SceneFader.FadeToScene(SceneManager.GetActiveScene().name);
        }

        void QuitToTitle()
        {
            _root.SetActive(false);
            SceneFader.FadeToScene(titleSceneName);
        }

        void BuildUI()
        {
            var canvasGO = new GameObject("DeathCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 990;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            _root = new GameObject("Panel");
            _root.transform.SetParent(canvasGO.transform, false);
            var dim = _root.AddComponent<Image>();
            dim.color = new Color(0.1f, 0, 0, 0.82f);
            var rt = dim.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var title = MakeText(_root.transform, "Title", 84, FontStyle.Bold, new Color(0.85f, 0.15f, 0.15f), "YOU DIED");
            SetRect(title.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(800, 110), Vector2.zero);

            MakeButton(_root.transform, "Respawn", new Vector2(0, -40), Respawn);
            MakeButton(_root.transform, "Quit to Title", new Vector2(0, -125), QuitToTitle);
        }

        void MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.22f, 0.1f, 0.12f, 1f);
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.45f); rt.anchorMax = new Vector2(0.5f, 0.45f);
            rt.sizeDelta = new Vector2(320, 64);
            rt.anchoredPosition = pos;
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(action);
            var t = MakeText(go.transform, "Label", 28, FontStyle.Bold, new Color(0.95f, 0.9f, 0.88f), label);
            SetRect(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        Text MakeText(Transform parent, string name, int size, FontStyle style, Color color, string content)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = GameFonts.UI;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.text = content;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            return t;
        }

        void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
        }
    }
}
