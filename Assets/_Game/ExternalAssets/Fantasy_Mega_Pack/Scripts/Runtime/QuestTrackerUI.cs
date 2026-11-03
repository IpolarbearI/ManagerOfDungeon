// QuestTrackerUI.cs - Canvas quest tracker (top-right): live objective list with progress + complete flash.
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FantasyDungeonPixelPack
{
    /// <summary>Top-right quest tracker showing active objectives with progress, ticks completed ones.</summary>
    public class QuestTrackerUI : MonoBehaviour
    {
        GameObject _root;
        Text _list;
        float _refreshTimer;

        void Awake() { BuildUI(); }

        void Update()
        {
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer > 0f) return;
            _refreshTimer = 0.25f;
            var qs = QuestSystem.Instance;
            if (qs == null || qs.objectives.Count == 0) { _root.SetActive(false); return; }
            _root.SetActive(true);
            var sb = new System.Text.StringBuilder();
            foreach (var o in qs.objectives)
            {
                if (o.IsDone) sb.Append("<color=#7fd47f>✓ ").Append(o.title).Append("</color>\n");
                else sb.Append("• ").Append(o.title).Append("  <color=#ffd966>").Append(Mathf.Min(o.current, o.required)).Append("/").Append(o.required).Append("</color>\n");
            }
            _list.text = sb.ToString().TrimEnd('\n');
        }

        void BuildUI()
        {
            var canvasGO = new GameObject("QuestCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _root = new GameObject("Panel");
            _root.transform.SetParent(canvasGO.transform, false);
            var bg = _root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.08f, 0.6f);
            bg.raycastTarget = false;
            var rt = bg.rectTransform;
            rt.anchorMin = new Vector2(1, 1); rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(430, 190);
            rt.anchoredPosition = new Vector2(-18, -18);

            var header = new GameObject("Header").AddComponent<Text>();
            header.transform.SetParent(_root.transform, false);
            header.font = GameFonts.UI;
            header.fontSize = 24;
            header.fontStyle = FontStyle.Bold;
            header.color = new Color(1f, 0.85f, 0.4f);
            header.text = "QUESTS";
            header.raycastTarget = false;
            var hrt = header.rectTransform;
            hrt.anchorMin = new Vector2(0, 1); hrt.anchorMax = new Vector2(1, 1);
            hrt.pivot = new Vector2(0.5f, 1);
            hrt.sizeDelta = new Vector2(0, 34);
            hrt.anchoredPosition = new Vector2(0, -8);
            header.alignment = TextAnchor.MiddleLeft;
            hrt.offsetMin = new Vector2(14, hrt.offsetMin.y);

            _list = new GameObject("List").AddComponent<Text>();
            _list.transform.SetParent(_root.transform, false);
            _list.font = GameFonts.UI;
            _list.fontSize = 22;
            _list.color = new Color(0.92f, 0.9f, 0.85f);
            _list.supportRichText = true;
            _list.raycastTarget = false;
            _list.alignment = TextAnchor.UpperLeft;
            var lrt = _list.rectTransform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(14, 8); lrt.offsetMax = new Vector2(-10, -44);
        }
    }
}
