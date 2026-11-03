// WaveConfigSO.cs - wave list asset for EnemySpawner.
using System.Collections.Generic;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Wave list asset for EnemySpawner (Create > FantasyDungeon > Wave Config): prefabs, count and spawn interval per wave.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "FantasyDungeon/Wave Config")]
    public class WaveConfigSO : ScriptableObject
    {
        [System.Serializable]
        public class Wave
        {
            public GameObject[] enemyPrefabs;
            public int count = 5;
            public float spawnInterval = 0.6f;
        }
        public List<Wave> waves = new List<Wave>();
        public float timeBetweenWaves = 4f;
    }
}
