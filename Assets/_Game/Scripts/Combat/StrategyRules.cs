namespace ManagerOfDungeon
{
    /// <summary>
    /// 몬스터 전략 ON/OFF 규칙 (04-6, SYS-15/16).
    /// A2: 기본 공격 전략 + 근거리 점사(유대감 Lv.2). 모든 전략 OFF 금지.
    /// 전략이 늘어나면 여기에 배타 그룹·우선순위(원거리 보호 > 점사 > 전투 우선 > 기본)를 추가한다.
    /// </summary>
    public static class StrategyRules
    {
        public const string BlockedMessage = "전략을 해제할 수 없습니다. 다른 전략을 먼저 선택하세요.";

        public static bool FocusFireUnlocked(MonsterState monster)
        {
            return monster != null && StrategyUnlockTable.MeleeFocusFireUnlocked(monster.bondLevel);
        }

        /// <summary>기본 공격 전략은 다른 전략이 모두 꺼져 있을 때 자동 ON.</summary>
        public static bool DefaultStrategyOn(MonsterState monster)
        {
            return monster == null || !monster.focusFire;
        }

        public static bool TrySetFocusFire(MonsterState monster, bool enabled, out string message)
        {
            if (monster == null)
            {
                message = string.Empty;
                return false;
            }

            if (enabled && !FocusFireUnlocked(monster))
            {
                message = $"유대감 Lv.{BondRules.MeleeFocusFireLevel}에서 점사가 해금됩니다.";
                return false;
            }

            monster.focusFire = enabled;
            message = enabled
                ? $"{monster.name}: 점사 ON (같은 편 원거리 몬스터의 타겟을 함께 공격)"
                : $"{monster.name}: 기본 공격 전략";
            return true;
        }

        public static bool TrySetDefault(MonsterState monster, bool enabled, out string message)
        {
            if (monster == null)
            {
                message = string.Empty;
                return false;
            }

            if (!enabled && !monster.focusFire)
            {
                message = BlockedMessage;
                return false;
            }

            if (enabled)
                monster.focusFire = false;
            message = enabled ? $"{monster.name}: 기본 공격 전략" : string.Empty;
            return true;
        }

        public static string Describe(MonsterState monster)
        {
            return DefaultStrategyOn(monster) ? "기본 공격" : "점사";
        }
    }
}
