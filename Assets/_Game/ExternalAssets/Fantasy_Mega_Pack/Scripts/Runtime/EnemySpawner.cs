// EnemySpawner.cs - wave-based enemy spawner for survival / arena / room encounters.
using System.Collections.Generic;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Wave-based enemy spawner driven by a WaveConfigSO: spawns waves in a radius, waits until all are dead, then starts the next wave.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public WaveConfigSO config;
        public float spawnRadius = 4f;
        public bool autoStart = true;
        [Tooltip("Optional: quest objective id reported for each spawned enemy killed.")]
        public string questObjectiveId;

        int _waveIndex;
        int _alive;
        float _timer;
        bool _spawning;
        int _toSpawn;
        float _spawnTick;
        WaveConfigSO.Wave _wave;

        void Start() { if (autoStart) NextWave(); }

        void NextWave()
        {
            if (config == null || _waveIndex >= config.waves.Count) { enabled = false; return; }
            _wave = config.waves[_waveIndex];
            _toSpawn = _wave.count;
            _spawning = true;
            _spawnTick = 0f;
        }

        void Update()
        {
            if (_spawning)
            {
                _spawnTick -= Time.deltaTime;
                if (_spawnTick <= 0f && _toSpawn > 0)
                {
                    SpawnOne();
                    _toSpawn--;
                    _spawnTick = _wave.spawnInterval;
                    if (_toSpawn <= 0) _spawning = false;
                }
                return;
            }
            if (_alive <= 0)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f) { _waveIndex++; _timer = config.timeBetweenWaves; NextWave(); }
            }
        }

        void SpawnOne()
        {
            if (_wave.enemyPrefabs == null || _wave.enemyPrefabs.Length == 0) return;
            var prefab = _wave.enemyPrefabs[Random.Range(0, _wave.enemyPrefabs.Length)];
            if (prefab == null) return;
            Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle.normalized * spawnRadius);
            var go = Instantiate(prefab, pos, Quaternion.identity);
            go.tag = "Enemy";
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.bodyType = RigidbodyType2D.Dynamic; rb.gravityScale = 0; rb.constraints = RigidbodyConstraints2D.FreezeRotation; }
            if (go.GetComponent<EnemyAI>() == null) go.AddComponent<EnemyAI>();
            var h = go.GetComponent<Health>();
            if (h == null) h = go.AddComponent<Health>();
            _alive++;
            h.onDeath.AddListener(() =>
            {
                _alive--;
                if (!string.IsNullOrEmpty(questObjectiveId) && QuestSystem.Instance != null)
                    QuestSystem.Instance.Report(questObjectiveId);
            });
        }
    }
}
