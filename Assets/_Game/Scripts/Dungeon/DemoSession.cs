using System.Collections.Generic;
using UnityEngine;

namespace ManagerOfDungeon
{
    public enum DemoPhase
    {
        Inspect,
        EquipBow,
        PlaceTrap,
        Battle1,
        Worry,
        Strategy,
        Battle2,
        Complete
    }

    public sealed class DemoSession
    {
        public const int BondLevel2Threshold = 100;
        public const int WorryExpReward = 20;
        public const int DemoStartingBondExp = 80;

        public static DemoSession Current { get; private set; } = new DemoSession();

        public static void ResetCurrent()
        {
            Current = new DemoSession();
        }

        public DemoPhase Phase { get; set; } = DemoPhase.Inspect;
        public int BondExp { get; private set; } = DemoStartingBondExp;
        public int BondLevel { get; private set; } = 1;
        public bool DefaultStrategy { get; private set; } = true;
        public bool FocusFire { get; private set; }
        public bool WorryComplete { get; private set; }
        public bool Battle1Finished { get; set; }
        public bool Battle2Finished { get; set; }
        public string BowActorId { get; set; } = string.Empty;
        public string WorryActorId { get; set; } = "goblin_a";
        public readonly List<Vector3> SpikePositions = new List<Vector3>();
        public string StrategyWarning { get; private set; } = string.Empty;

        public string WorryMonsterName => WorryActorId == "goblin_b" ? "고블린 B" : "고블린 A";

        public void AssignWorryTarget(string actorId)
        {
            WorryActorId = actorId == "goblin_b" ? "goblin_b" : "goblin_a";
        }

        public bool FocusUnlocked => BondLevel >= 2;

        public void GrantWorryReward()
        {
            if (WorryComplete)
                return;

            WorryComplete = true;
            BondExp += WorryExpReward;
            if (BondLevel < 2 && BondExp >= BondLevel2Threshold)
            {
                BondExp -= BondLevel2Threshold;
                BondLevel = 2;
            }
        }

        public bool TrySetDefaultStrategy(bool enabled)
        {
            StrategyWarning = string.Empty;
            if (!enabled && !FocusFire)
            {
                StrategyWarning = "전략을 해제할 수 없습니다. 다른 전략을 먼저 선택하세요.";
                return false;
            }

            DefaultStrategy = enabled;
            if (!DefaultStrategy && FocusFire)
                return true;
            if (DefaultStrategy)
                FocusFire = false;
            return true;
        }

        public bool TrySetFocusFire(bool enabled)
        {
            StrategyWarning = string.Empty;
            if (!FocusUnlocked)
            {
                StrategyWarning = "유대감 Lv.2에서 점사가 해금됩니다.";
                return false;
            }

            if (!enabled && !DefaultStrategy)
            {
                StrategyWarning = "전략을 해제할 수 없습니다. 다른 전략을 먼저 선택하세요.";
                return false;
            }

            FocusFire = enabled;
            DefaultStrategy = !enabled;
            return true;
        }
    }
}
