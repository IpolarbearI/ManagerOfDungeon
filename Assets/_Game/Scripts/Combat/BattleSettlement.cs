using System.Collections.Generic;

namespace ManagerOfDungeon
{
    /// <summary>전투가 끝났을 때 CombatDirector가 넘겨주는 결과.</summary>
    public sealed class BattleResult
    {
        public bool DefendersWon;
        public float DurationSeconds;
        /// <summary>실제로 전투에 참여한 몬스터(사망 포함).</summary>
        public readonly List<string> ParticipantMonsterIds = new List<string>();
        public readonly List<string> DeadMonsterIds = new List<string>();
        public readonly List<string> TriggeredTrapIds = new List<string>();
    }

    /// <summary>유대감 레벨 상승 1건. 머리 위 '♥ Lv.N ↑'와 전략 해금 팝업에 쓴다.</summary>
    public sealed class BondLevelUp
    {
        public string MonsterId;
        public string MonsterName;
        public int FromLevel;
        public int ToLevel;
    }

    /// <summary>정산 결과(결과 패널 표시용).</summary>
    public sealed class SettlementReport
    {
        public bool Won;
        public bool CoinLost;
        public bool GameOver;
        public int SolReward;
        public int ConsumedTrapValue;
        public int DeployedValue;
        public int TrapsReinstalled;
        public int TrapsReinstallFailed;
        public readonly List<string> Lines = new List<string>();
        public readonly List<BondLevelUp> LevelUps = new List<BondLevelUp>();
    }

    /// <summary>
    /// 전투 후 정산 (04-3, 04-12, 11-10 개정).
    /// 순서: 패배 시 코인 −1 먼저 → (승리) 참여 몬스터 각각 유대감 +30 → 사망 몬스터 −10
    /// → 승리 보상 솔 → 함정 소모·자동 재설치(솔 부족 시 실패 알림).
    /// 승리 보상 = 발동·소모된 함정 가치 × 10% + 그 외 배치 자산 × 15%.
    /// </summary>
    public static class BattleSettlement
    {
        public static SettlementReport Apply(GameState state, BattleResult result)
        {
            var report = new SettlementReport();
            report.Won = result.DefendersWon;

            // 보상 계산은 함정 소모 처리 전에 한다(발동 함정은 '소모분'으로 따로 계산).
            int consumedValue = 0;
            for (int i = 0; i < result.TriggeredTrapIds.Count; i++)
            {
                var trap = state.FindTrap(result.TriggeredTrapIds[i]);
                var def = trap != null ? GameDefs.GetTrap(trap.trapId) : null;
                if (def != null && !trap.used)
                    consumedValue += def.AssetValue;
            }

            int deployed = AssetCalculator.Deployed(state) - consumedValue;
            report.ConsumedTrapValue = consumedValue;
            report.DeployedValue = deployed;

            // 1) 패배: 코인 먼저
            if (!result.DefendersWon)
            {
                state.coins--;
                report.CoinLost = true;
                report.Lines.Add($"방어 실패 · 코인 -1 (남은 코인 {state.coins})");
                if (state.coins <= 0)
                {
                    state.coins = 0;
                    state.gameOver = true;
                    report.GameOver = true;
                }
            }

            // 2) 방어 성공: 전투에 참여한 몬스터 각각 유대감 +30
            //    [결정 필요] 전투 중 쓰러진 몬스터도 '참여'로 보고 +30 후 −10을 적용(순 +20).
            if (result.DefendersWon)
            {
                for (int i = 0; i < result.ParticipantMonsterIds.Count; i++)
                {
                    var monster = state.FindMonster(result.ParticipantMonsterIds[i]);
                    if (monster == null)
                        continue;
                    var bond = monster.CreateBond();
                    int before = bond.Level;
                    bond.AddExp(BondRules.DefenseWinExp);
                    monster.StoreBond(bond);
                    RecordLevelUp(report, monster, before);
                }

                report.Lines.Add($"전투 참여 몬스터 유대감 +{BondRules.DefenseWinExp}");
            }

            // 3) 사망 몬스터 유대감 −10 (레벨은 내려가지 않음, 현재 레벨 EXP 최소 0)
            for (int i = 0; i < result.DeadMonsterIds.Count; i++)
            {
                var monster = state.FindMonster(result.DeadMonsterIds[i]);
                if (monster == null)
                    continue;
                var bond = monster.CreateBond();
                int lost = bond.ApplyDeathPenalty();
                monster.StoreBond(bond);
                report.Lines.Add($"{monster.name} 쓰러짐 · 유대감 EXP -{lost}");
            }

            for (int i = 0; i < report.LevelUps.Count; i++)
            {
                var up = report.LevelUps[i];
                report.Lines.Add($"{up.MonsterName} 유대감 Lv.{up.ToLevel} 달성!");
            }

            // 4) 승리 보상
            if (result.DefendersWon)
            {
                float reward = consumedValue * BalanceConfig.RewardConsumedTrapRate
                    + deployed * BalanceConfig.RewardDeployedRate;
                report.SolReward = (int)reward;
                state.sol += report.SolReward;
                report.Lines.Add($"보상 +{report.SolReward}솔 (소모 함정 {consumedValue}×10% + 배치 자산 {deployed}×15%)");
            }

            // 5) 발동 함정 소모 → 실루엣, 자동 재설치
            for (int i = 0; i < result.TriggeredTrapIds.Count; i++)
            {
                var trap = state.FindTrap(result.TriggeredTrapIds[i]);
                if (trap != null)
                    trap.used = true;
            }

            report.TrapsReinstalled = TrapService.AutoReinstall(state, out int failed);
            report.TrapsReinstallFailed = failed;
            if (report.TrapsReinstalled > 0)
                report.Lines.Add($"함정 자동 재설치 {report.TrapsReinstalled}개");
            if (failed > 0)
                report.Lines.Add($"자동 재설치 실패 {failed}개 (솔 부족)");

            state.invasion.lastInvasionDay = state.day;
            state.invasion.todayTriggered = true;

            for (int i = 0; i < report.Lines.Count; i++)
                state.AddLog(report.Lines[i]);
            return report;
        }

        static void RecordLevelUp(SettlementReport report, MonsterState monster, int fromLevel)
        {
            if (monster.bondLevel <= fromLevel)
                return;

            var up = new BondLevelUp();
            up.MonsterId = monster.id;
            up.MonsterName = monster.name;
            up.FromLevel = fromLevel;
            up.ToLevel = monster.bondLevel;
            report.LevelUps.Add(up);
        }
    }
}
