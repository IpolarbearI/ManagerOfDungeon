using System.Collections.Generic;

namespace ManagerOfDungeon
{
    public sealed class StrategyInfo
    {
        public string Id;
        public string Name;
        public string Description;
        public int UnlockLevel;
        public bool Implemented;
    }

    /// <summary>
    /// 유대감 레벨로 해금되는 전략 목록 (04-6). 해금 순간 이름·설명 팝업에 쓴다.
    /// 11-10 개정: 최대 레벨 제한이 없어졌고 Lv.4 이후에도 특정 레벨마다 전략이 추가될 예정 →
    /// 새 전략은 여기에 한 줄씩 추가한다. Implemented=false인 전략은 팝업을 띄우지 않는다.
    /// </summary>
    public static class StrategyCatalog
    {
        public static readonly StrategyInfo[] All =
        {
            new StrategyInfo
            {
                Id = "focus_fire",
                Name = "점사",
                Description = "같은 편 원거리 몬스터가 노리는 용사를 함께 공격합니다.\n근거리 몬스터일 때 적용됩니다.",
                UnlockLevel = BondRules.MeleeFocusFireLevel,
                Implemented = true
            },
            new StrategyInfo
            {
                Id = "trap_lure",
                Name = "함정 유도",
                Description = "용사가 함정을 밟도록 길을 막아서며 이동합니다.",
                UnlockLevel = 3,
                Implemented = false
            },
            new StrategyInfo
            {
                Id = "tactical_retreat",
                Name = "전략적 후퇴",
                Description = "원거리 아군의 후퇴를 따라가며 대형과 엄호를 유지합니다.",
                UnlockLevel = 4,
                Implemented = false
            },
        };

        /// <summary>fromLevel 초과 ~ toLevel 이하에서 새로 열린 (구현된) 전략.</summary>
        public static List<StrategyInfo> UnlockedBetween(int fromLevel, int toLevel)
        {
            var list = new List<StrategyInfo>();
            for (int i = 0; i < All.Length; i++)
            {
                var info = All[i];
                if (info.Implemented && info.UnlockLevel > fromLevel && info.UnlockLevel <= toLevel)
                    list.Add(info);
            }

            return list;
        }
    }
}
