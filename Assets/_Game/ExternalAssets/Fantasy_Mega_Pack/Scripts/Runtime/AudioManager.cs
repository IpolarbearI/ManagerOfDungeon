// AudioManager.cs - BGM zones with crossfade + named SFX. Clips are optional (silent if unset).
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Central audio: AudioManager.PlayBGM("dungeon"), AudioManager.PlaySFX("sword").
    /// Register clips in the inspector (or via builder). Missing names fail silently.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [System.Serializable]
        public class NamedClip { public string name; public AudioClip clip; [Range(0f, 1f)] public float volume = 1f; }

        public List<NamedClip> music = new List<NamedClip>();
        public List<NamedClip> sfx = new List<NamedClip>();
        [Range(0f, 1f)] public float musicVolume = 0.55f;
        [Range(0f, 1f)] public float sfxVolume = 0.9f;
        public float crossfade = 1.2f;

        public static AudioManager Instance { get; private set; }
        AudioSource _bgmA, _bgmB, _sfxSource;
        bool _usingA = true;
        string _currentBGM = "";

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _bgmA = gameObject.AddComponent<AudioSource>(); _bgmA.loop = true; _bgmA.playOnAwake = false;
            _bgmB = gameObject.AddComponent<AudioSource>(); _bgmB.loop = true; _bgmB.playOnAwake = false;
            _sfxSource = gameObject.AddComponent<AudioSource>(); _sfxSource.playOnAwake = false;
        }

        public static void PlayBGM(string name)
        {
            if (Instance == null || Instance._currentBGM == name) return;
            var nc = Instance.music.Find(m => m.name == name);
            if (nc == null || nc.clip == null)
            {
                var clip = Resources.Load<AudioClip>("Audio/BGM/" + name);
                if (clip == null) return;
                nc = new NamedClip { name = name, clip = clip, volume = 1f };
                Instance.music.Add(nc);
            }
            Instance._currentBGM = name;
            Instance.StartCoroutine(Instance.CrossfadeRoutine(nc));
        }

        public static void StopBGM()
        {
            if (Instance == null) return;
            Instance._currentBGM = "";
            Instance.StartCoroutine(Instance.FadeOutAll());
        }

        public static void PlaySFX(string name, float pitchJitter = 0.06f)
        {
            if (Instance == null) return;
            var nc = Instance.sfx.Find(s => s.name == name);
            if (nc == null || nc.clip == null)
            {
                var clip = Resources.Load<AudioClip>("Audio/SFX/" + name);
                if (clip == null) return;
                nc = new NamedClip { name = name, clip = clip, volume = 1f };
                Instance.sfx.Add(nc);
            }
            Instance._sfxSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            Instance._sfxSource.PlayOneShot(nc.clip, nc.volume * Instance.sfxVolume);
        }

        IEnumerator CrossfadeRoutine(NamedClip next)
        {
            var from = _usingA ? _bgmA : _bgmB;
            var to = _usingA ? _bgmB : _bgmA;
            _usingA = !_usingA;
            to.clip = next.clip;
            to.volume = 0f;
            to.Play();
            float t = 0f;
            float fromStart = from.volume;
            float target = next.volume * musicVolume;
            while (t < crossfade)
            {
                t += Time.unscaledDeltaTime;
                float k = t / crossfade;
                to.volume = Mathf.Lerp(0f, target, k);
                from.volume = Mathf.Lerp(fromStart, 0f, k);
                yield return null;
            }
            to.volume = target;
            from.Stop();
        }

        IEnumerator FadeOutAll()
        {
            float t = 0f;
            float a = _bgmA.volume, b = _bgmB.volume;
            while (t < crossfade)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - t / crossfade;
                _bgmA.volume = a * k; _bgmB.volume = b * k;
                yield return null;
            }
            _bgmA.Stop(); _bgmB.Stop();
        }
    }
}
