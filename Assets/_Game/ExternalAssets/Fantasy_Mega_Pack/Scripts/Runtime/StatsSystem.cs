// StatsSystem.cs - HP/MP/XP/Level + derived stats. Drives the pack's HP/MP/XP bars + level-up VFX.
using System;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// RPG progression: HP/MP/XP/Level with auto level-up (AddXP), derived AttackPower/Defense, and normalized HPPercent/MPPercent/XPPercent for UI bars.
    /// </summary>
    public class StatsSystem : MonoBehaviour
    {
        [Header("Progression")]
        public int level = 1;
        public int xp = 0;
        public int xpToNext = 100;

        [Header("Attributes")]
        public int strength = 5;
        public int dexterity = 5;
        public int intelligence = 5;

        [Header("Resources")]
        public int maxMP = 50;
        public int currentMP;

        public event Action OnStatsChanged;
        public event Action<int> OnLevelUp;

        Health _health;

        void Awake()
        {
            _health = GetComponent<Health>();
            currentMP = maxMP;
        }

        public int AttackPower => strength * 2 + level;
        public int Defense => dexterity + level / 2;

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            xp += amount;
            while (xp >= xpToNext) { xp -= xpToNext; LevelUp(); }
            OnStatsChanged?.Invoke();
        }

        void LevelUp()
        {
            level++;
            xpToNext = Mathf.RoundToInt(xpToNext * 1.4f);
            strength++; dexterity++; intelligence++;
            maxMP += 5; currentMP = maxMP;
            if (_health != null) { _health.maxHealth += 10; }
            OnLevelUp?.Invoke(level);
            OnStatsChanged?.Invoke();
        }

        public bool SpendMP(int amount)
        {
            if (currentMP < amount) return false;
            currentMP -= amount;
            OnStatsChanged?.Invoke();
            return true;
        }

        public float HPPercent => (_health != null && _health.maxHealth > 0) ? (float)_health.CurrentHealth / _health.maxHealth : 1f;
        public float MPPercent => maxMP > 0 ? (float)currentMP / maxMP : 0f;
        public float XPPercent => xpToNext > 0 ? (float)xp / xpToNext : 0f;
    }
}
