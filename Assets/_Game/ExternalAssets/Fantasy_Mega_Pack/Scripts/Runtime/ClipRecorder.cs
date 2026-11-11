// ClipRecorder.cs - captures the main camera to a PNG frame sequence during Play mode.
// Pair with CameraDolly for trailer fly-throughs, then assemble with ffmpeg:
//   ffmpeg -framerate 12 -i f_%05d.png -vf "scale=1280:-2" -c:v libx264 -pix_fmt yuv420p clip.mp4
using System.IO;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Records Camera.main to numbered PNG frames (world rendering only, no overlay UI).
    /// Attach next to CameraDolly, press Play, frames land in the project folder.
    /// </summary>
    public class ClipRecorder : MonoBehaviour
    {
        [Tooltip("Frames per second to capture.")]
        public float fps = 12f;
        [Tooltip("Stop capturing after this many seconds.")]
        public float duration = 60f;
        [Tooltip("Output folder relative to the project root.")]
        public string outputFolder = "Captures/rec";
        public int width = 1600;
        public int height = 900;
        [Tooltip("Delay before the first frame (lets fades/intros settle).")]
        public float startDelay = 0.5f;

        float _accum;
        float _elapsed;
        int _frame;
        RenderTexture _rt;
        Texture2D _tex;
        string _dir;

        void Start()
        {
            _dir = Path.Combine(Directory.GetCurrentDirectory(), outputFolder);
            Directory.CreateDirectory(_dir);
            _rt = new RenderTexture(width, height, 24);
            _tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        }

        void LateUpdate()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < startDelay || _elapsed > startDelay + duration) return;

            _accum += Time.deltaTime;
            if (_accum < 1f / fps) return;
            _accum -= 1f / fps;

            var cam = Camera.main;
            if (cam == null) return;

            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;
            cam.targetTexture = _rt;
            cam.Render();
            RenderTexture.active = _rt;
            _tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;

            File.WriteAllBytes(Path.Combine(_dir, "f_" + _frame.ToString("D5") + ".png"), _tex.EncodeToPNG());
            _frame++;
        }

        void OnDestroy()
        {
            if (_rt != null) _rt.Release();
        }
    }
}
