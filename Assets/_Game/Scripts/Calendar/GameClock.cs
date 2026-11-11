using UnityEngine;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 게임 시계 (04-1). 06:00~22:00, 실제 약 5분.
    /// 시간을 진행시키고 지나간 정시 수와 22:00 도달 여부를 알려준다.
    /// </summary>
    public static class GameClock
    {
        public struct TickResult
        {
            public int HoursPassed;
            public bool ReachedDayEnd;
        }

        public static TickResult Advance(GameState state, float realSeconds)
        {
            var result = new TickResult();
            if (state == null || realSeconds <= 0f)
                return result;

            float before = state.clockMinutes;
            float after = Mathf.Min(BalanceConfig.DayEndMinute, before + realSeconds * BalanceConfig.GameMinutesPerRealSecond);
            state.clockMinutes = after;
            result.HoursPassed = Mathf.FloorToInt(after / 60f) - Mathf.FloorToInt(before / 60f);
            result.ReachedDayEnd = after >= BalanceConfig.DayEndMinute;
            return result;
        }

        public static void StartNewDay(GameState state)
        {
            state.day++;
            state.clockMinutes = BalanceConfig.DayStartMinute;
            state.dayStarted = false;
        }

        public static string Format(float minutes)
        {
            int total = Mathf.FloorToInt(minutes);
            int h = total / 60;
            int m = (total % 60) / 10 * 10; // 10분 단위 표시
            return h.ToString("00") + ":" + m.ToString("00");
        }
    }
}
