// DemoRecorder.cs - records the camera to a PNG frame sequence while playing (for trailers/GIFs).
using System.IO;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Frame recorder for marketing: while the scene plays, renders the camera to PNGs
    /// (default every 0.4s) into [project]/Recordings. Stitch with ffmpeg into a trailer.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class DemoRecorder : MonoBehaviour
    {
        public float interval = 0.4f;
        public int width = 1280;
        public int height = 720;
        public string folder = "Recordings";

        Camera _cam;
        float _t;
        int _n;
        string _dir;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _dir = Path.Combine(Application.dataPath, "..", folder);
            Directory.CreateDirectory(_dir);
        }

        void LateUpdate()
        {
            _t += Time.deltaTime;
            if (_t < interval) return;
            _t = 0f;
            var rt = RenderTexture.GetTemporary(width, height, 24);
            var prev = _cam.targetTexture;
            _cam.targetTexture = rt;
            _cam.Render();
            _cam.targetTexture = prev;
            var act = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = act;
            RenderTexture.ReleaseTemporary(rt);
            File.WriteAllBytes(Path.Combine(_dir, $"frame_{_n:D4}.png"), tex.EncodeToPNG());
            Destroy(tex);
            _n++;
        }
    }
}
