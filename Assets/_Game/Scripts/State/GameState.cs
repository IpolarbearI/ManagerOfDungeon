using System;
using System.Collections.Generic;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 한 판의 모든 런타임 데이터. 저장 단위이며 JsonUtility로 직렬화된다.
    /// 규칙: 게임의 진실은 여기에만 있다. MonoBehaviour는 이 값을 읽어 그리고, 서비스를 통해서만 바꾼다.
    /// (JsonUtility 제약: public 필드, List, [Serializable] 클래스만 사용. Dictionary·프로퍼티 금지)
    /// </summary>
    [Serializable]
    public sealed class GameState
    {
        public const int CurrentSaveVersion = 1;

        public int saveVersion = CurrentSaveVersion;
        public int day = 1;
        public float clockMinutes = BalanceConfig.DayStartMinute;
        public bool dayStarted;
        public int sol;
        public int coins;
        public bool endless;
        public bool gameOver;
        public GameRng rng = new GameRng(1);
        public int nextId = 1;
        public int assetSnapshot;      // 06:00 총자산가치
        public int defenseSnapshot;    // 06:00 방어 전투력 가치 → 침입 강도 기준
        public List<MonsterState> monsters = new List<MonsterState>();
        public List<TrapState> traps = new List<TrapState>();
        public List<string> inventory = new List<string>();
        public InvasionState invasion = new InvasionState();
        public List<string> log = new List<string>();

        public string NewId(string prefix)
        {
            string id = prefix + "_" + nextId;
            nextId++;
            return id;
        }

        public MonsterState FindMonster(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            for (int i = 0; i < monsters.Count; i++)
            {
                if (monsters[i].id == id)
                    return monsters[i];
            }

            return null;
        }

        public TrapState FindTrap(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            for (int i = 0; i < traps.Count; i++)
            {
                if (traps[i].id == id)
                    return traps[i];
            }

            return null;
        }

        public void AddLog(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;
            log.Add("[D" + day + " " + GameClock.Format(clockMinutes) + "] " + line);
            while (log.Count > BalanceConfig.MaxLogLines)
                log.RemoveAt(0);
        }
    }

    [Serializable]
    public sealed class MonsterState
    {
        public string id;
        public string speciesId;
        public string name;
        public string personalityId;
        public string weaponId;
        public string armorId;

        // 유대감 (04-5)
        public int bondLevel = BondRules.MinLevel;
        public int bondExp;

        // 전략 (04-6). A2: 기본 공격 전략 / 근거리 점사
        public bool focusFire;

        // 전투 시작 위치: 화면 비율 좌표(0~1). 카메라 크기가 달라도 같은 자리.
        public float posX;
        public float posY;

        // 교감 장부
        public int talkDay;
        public int talkCount;
        public int giftBonus;
        public int worryChance;
        public bool hasWorry;
        public string worryTopicId;
        public List<MemoryState> memories = new List<MemoryState>();

        public BondProgress CreateBond()
        {
            return new BondProgress(bondLevel, bondExp);
        }

        public void StoreBond(BondProgress bond)
        {
            if (bond == null)
                return;
            bondLevel = bond.Level;
            bondExp = bond.Exp;
        }
    }

    [Serializable]
    public sealed class MemoryState
    {
        public int day;
        public string topicId;
        public string playerChoice;
        public string summary;
    }

    [Serializable]
    public sealed class TrapState
    {
        public string id;
        public string trapId;
        public float posX;
        public float posY;
        public bool used;
        public bool autoReinstall;
    }

    [Serializable]
    public sealed class InvasionState
    {
        public int lastInvasionDay = BalanceConfig.InitialLastInvasionDay;
        public bool todayHasInvasion;
        public float todayMinute;
        public bool todayTriggered;
        public HeroPartyState party = new HeroPartyState();
    }

    [Serializable]
    public sealed class HeroPartyState
    {
        public int totalValue;
        public int targetValue;
        public List<HeroState> heroes = new List<HeroState>();
    }

    [Serializable]
    public sealed class HeroState
    {
        public string jobId;
        // 강화 후보 4종: HP · 공격력 · 공격속도 · AI (11-10 개정: AI도 같은 후보, 후순위 규칙 폐지)
        public int hpLevel;
        public int atkLevel;
        public int asLevel;
        public int aiLevel; // 0 = 기본공격만

        public int StatSteps => hpLevel + atkLevel + asLevel + aiLevel;

        public int Value()
        {
            var def = GameDefs.GetHero(jobId);
            int baseValue = def != null ? def.BaseValue : 0;
            return baseValue + StatSteps * BalanceConfig.HeroStatStepValue;
        }

        public float MaxHealth()
        {
            var def = GameDefs.GetHero(jobId);
            return def == null ? 1f : def.MaxHealth + GameDefs.HeroHpBonus[Clamp(hpLevel)];
        }

        public float AttackPower()
        {
            var def = GameDefs.GetHero(jobId);
            return def == null ? 0f : def.AttackPower + GameDefs.HeroAtkBonus[Clamp(atkLevel)];
        }

        public float AttackSpeed()
        {
            var def = GameDefs.GetHero(jobId);
            return def == null ? 1f : def.AttackSpeed + GameDefs.HeroAsBonus[Clamp(asLevel)];
        }

        static int Clamp(int level)
        {
            if (level < 0)
                return 0;
            return level > BalanceConfig.MaxHeroStatLevel ? BalanceConfig.MaxHeroStatLevel : level;
        }
    }
}
