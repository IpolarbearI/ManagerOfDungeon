// MinimapSystem.cs - corner minimap showing player, enemies, NPCs and chests (OnGUI, zero setup).
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Drop-in minimap (top-right): dots for the player (cyan), enemies (red), chests (gold).
    /// No render textures, no extra cameras — works everywhere. Tune worldRadius to zoom.
    /// </summary>
    public class MinimapSystem : MonoBehaviour
    {
        public float worldRadius = 14f;
        public int size = 150;

        Transform _player;
        Texture2D _dot;

        void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
            _dot = new Texture2D(1, 1);
            _dot.SetPixel(0, 0, Color.white);
            _dot.Apply();
        }

        void OnGUI()
        {
            if (_player == null) return;
            var r = new Rect(Screen.width - size - 12, Screen.height - size - 12, size, size);
            GUI.Box(r, "MAP");
            DrawDot(r, _player.position, new Color(0.35f, 0.78f, 1f), 7);
            foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
                DrawDot(r, e.transform.position, new Color(1f, 0.35f, 0.3f), 5);
            foreach (var c in Object.FindObjectsByType<ChestInteractable>(FindObjectsSortMode.None))
                DrawDot(r, c.transform.position, new Color(1f, 0.85f, 0.3f), 5);
        }

        void DrawDot(Rect map, Vector3 world, Color col, int px)
        {
            Vector2 d = (world - _player.position) / worldRadius; // -1..1
            if (Mathf.Abs(d.x) > 1f || Mathf.Abs(d.y) > 1f) return;
            float x = map.x + map.width / 2f + d.x * (map.width / 2f - 6);
            float y = map.y + map.height / 2f - d.y * (map.height / 2f - 6);
            var old = GUI.color;
            GUI.color = col;
            GUI.DrawTexture(new Rect(x - px / 2f, y - px / 2f, px, px), _dot);
            GUI.color = old;
        }
    }
}
