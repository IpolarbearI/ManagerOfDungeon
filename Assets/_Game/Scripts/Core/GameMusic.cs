using UnityEngine;

namespace ManagerOfDungeon
{
    public sealed class GameMusic : MonoBehaviour
    {
        static GameMusic _instance;

        AudioSource _source;
        string _track = string.Empty;

        public static void PlayDungeon()
        {
            Ensure().Play("dungeon", PackAssets.DungeonBgm());
        }

        public static void PlayBattle()
        {
            Ensure().Play("battle", PackAssets.BattleBgm());
        }

        public static void Stop()
        {
            if (_instance == null)
                return;

            _instance.Setup();
            if (_instance._source != null)
                _instance._source.Stop();
            _instance._track = string.Empty;
        }

        static GameMusic Ensure()
        {
            if (_instance != null)
                return _instance;

            var existing = FindAnyObjectByType<GameMusic>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            var go = new GameObject("GameMusic");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<GameMusic>();
            _instance.Setup();
            return _instance;
        }

        void Setup()
        {
            if (_source != null)
                return;

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.volume = 0.55f;
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Setup();
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        void Play(string track, AudioClip clip)
        {
            Setup();
            if (clip == null || _source == null)
                return;

            if (_track == track && _source.isPlaying && _source.clip == clip)
                return;

            if (clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();

            _track = track;
            _source.clip = clip;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.Play();
        }
    }
}
