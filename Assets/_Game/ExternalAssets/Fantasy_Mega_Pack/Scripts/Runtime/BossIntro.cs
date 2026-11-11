// BossIntro.cs - cinematic boss introduction: camera push + name banner + sting. THE clip moment.
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Plays a short boss-intro cinematic: camera pushes to the boss, dark letterbox bars,
    /// boss name slams in, then control returns. Call BossIntro.Play("THE SLIME KING", bossTransform).
    /// </summary>
    public class BossIntro : MonoBehaviour
    {
        public static bool IsPlaying { get; private set; }

        public static void Play(string bossName, Transform boss, float duration = 2.8f)
        {
            if (IsPlaying || boss == null) return;
            var go = new GameObject("BossIntro");
            var bi = go.AddComponent<BossIntro>();
            bi.StartCoroutine(bi.Routine(bossName, boss, duration));
        }

        IEnumerator Routine(string bossName, Transform boss, float duration)
        {
            IsPlaying = true;
            AudioManager.PlaySFX("boss_sting");

            var cam = Camera.main;
            var follow = cam != null ? cam.GetComponent<CameraFollow>() : null;
            Vector3 camStart = cam != null ? cam.transform.position : Vector3.zero;
            float sizeStart = cam != null ? cam.orthographicSize : 5f;
            if (follow != null) follow.enabled = false;

            // letterbox + name UI
            var cgo = new GameObject("BossIntroCanvas");
            cgo.transform.SetParent(transform, false);
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 950;

            Image MakeBar(float anchorY0, float anchorY1)
            {
                var b = new GameObject("Bar").AddComponent<Image>();
                b.transform.SetParent(cgo.transform, false);
                b.color = new Color(0, 0, 0, 0.85f);
                var rt = b.rectTransform;
                rt.anchorMin = new Vector2(0, anchorY0); rt.anchorMax = new Vector2(1, anchorY1);
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                return b;
            }
            MakeBar(0f, 0.12f); MakeBar(0.88f, 1f);

            var tgo = new GameObject("BossName");
            tgo.transform.SetParent(cgo.transform, false);
            var txt = tgo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 64;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(1f, 0.85f, 0.25f);
            txt.text = bossName;
            var trt = txt.rectTransform;
            trt.anchorMin = new Vector2(0.5f, 0.78f); trt.anchorMax = new Vector2(0.5f, 0.78f);
            trt.sizeDelta = new Vector2(900, 90);

            var sh = new GameObject("Shadow");
            sh.transform.SetParent(tgo.transform, false);
            var shadow = tgo.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.9f);
            shadow.effectDistance = new Vector2(4, -4);

            // camera push toward boss + name slam
            float t = 0f;
            Vector3 target = new Vector3(boss.position.x, boss.position.y, camStart.z);
            float push = duration * 0.45f;
            Vector3 nameFull = Vector3.one;
            tgo.transform.localScale = nameFull * 2.2f;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (cam != null)
                {
                    float k = Mathf.SmoothStep(0f, 1f, Mathf.Min(t / push, 1f));
                    cam.transform.position = Vector3.Lerp(camStart, target, k * 0.85f);
                    cam.orthographicSize = Mathf.Lerp(sizeStart, sizeStart * 0.72f, k);
                }
                float ns = Mathf.Lerp(2.2f, 1f, Mathf.Min(t / 0.35f, 1f));
                tgo.transform.localScale = nameFull * ns;
                yield return null;
            }

            // return camera
            t = 0f;
            float back = 0.5f;
            Vector3 pushedPos = cam != null ? cam.transform.position : camStart;
            float pushedSize = cam != null ? cam.orthographicSize : sizeStart;
            while (t < back)
            {
                t += Time.deltaTime;
                if (cam != null)
                {
                    float k = t / back;
                    cam.transform.position = Vector3.Lerp(pushedPos, camStart, k);
                    cam.orthographicSize = Mathf.Lerp(pushedSize, sizeStart, k);
                }
                yield return null;
            }
            if (follow != null) follow.enabled = true;
            IsPlaying = false;
            Destroy(gameObject);
        }
    }
}
