using System.Collections.Generic;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 침입 용사 파티 생성 (04-9 + 11-10 개정). 후보 전수 나열 → 필터 → 추첨.
    /// - 목표: 06:00 '방어 전투력 가치' ±오차. 없으면 가장 가까운 후보.
    /// - 파티 가치 = Σ(직업 기본가치 + 강화 단계 합 × 단계 가치)
    /// - 강화 후보 4종: HP · 공격력 · 공격속도 · AI. 낮은 단계부터 무작위로 올린다(동률이면 무작위).
    ///   AI 후순위 규칙은 폐지. 단 A2에서는 구현된 AI(0~2)까지만 올라간다.
    /// - 직업: 인원이 직업 수 이상이면 모든 직업 최소 1명, 이후 중복 허용.
    /// - 구성원 간 평균 강화 단계 격차 ≤ 1.
    /// - 추첨: 인원수를 먼저 균등 추첨 → 그 인원 안에서 균등 추첨('인원 증가 vs 개체 강화' 변주).
    /// </summary>
    public static class PartyGenerator
    {
        const int StatCount = BalanceConfig.HeroUpgradeStatCount;

        sealed class Choice
        {
            public string[] Jobs;
            public int[] Steps;
            public int Value;
        }

        sealed class Search
        {
            public int Target;
            public int MaxSteps;
            public GameRng Rng;
            public string[] Jobs;
            public int[] Steps;
            public Choice Picked;
            public int Count;
            public Choice Nearest;
            public int NearestDiff = int.MaxValue;
        }

        public static HeroPartyState Generate(int targetValue, GameRng rng)
        {
            return Generate(targetValue, rng, GameDefs.AlphaHeroJobs, BalanceConfig.MaxHeroAiLevelAlpha2);
        }

        public static HeroPartyState Generate(int targetValue, GameRng rng, string[] jobPool, int maxAiLevel)
        {
            if (maxAiLevel < 0)
                maxAiLevel = 0;
            int maxSteps = 3 * BalanceConfig.MaxHeroStatLevel + maxAiLevel;
            int maxSize = BalanceConfig.MaxPartySize;
            var pickedBySize = new Choice[maxSize + 1];
            var countBySize = new int[maxSize + 1];
            Choice nearest = null;
            int nearestDiff = int.MaxValue;

            for (int n = 1; n <= maxSize; n++)
            {
                var search = new Search();
                search.Target = targetValue;
                search.MaxSteps = maxSteps;
                search.Rng = rng;
                search.Steps = new int[n];

                var multisets = JobMultisets(jobPool, n);
                for (int m = 0; m < multisets.Count; m++)
                {
                    search.Jobs = multisets[m];
                    Enumerate(search, 0);
                }

                pickedBySize[n] = search.Picked;
                countBySize[n] = search.Count;
                if (search.Nearest != null && search.NearestDiff < nearestDiff)
                {
                    nearest = search.Nearest;
                    nearestDiff = search.NearestDiff;
                }
            }

            var sizes = new List<int>();
            for (int n = 1; n <= maxSize; n++)
            {
                if (countBySize[n] > 0)
                    sizes.Add(n);
            }

            Choice chosen = sizes.Count > 0 ? pickedBySize[sizes[rng.Range(0, sizes.Count)]] : nearest;
            return Build(chosen, targetValue, rng, maxAiLevel);
        }

        static void Enumerate(Search s, int index)
        {
            int n = s.Steps.Length;
            if (index == n)
            {
                Evaluate(s);
                return;
            }

            // 같은 직업끼리는 순서만 다른 중복 조합을 건너뛴다.
            bool sameAsPrev = index > 0 && s.Jobs[index] == s.Jobs[index - 1];
            int minSteps = sameAsPrev ? s.Steps[index - 1] : 0;
            for (int t = minSteps; t <= s.MaxSteps; t++)
            {
                s.Steps[index] = t;
                Enumerate(s, index + 1);
            }
        }

        static void Evaluate(Search s)
        {
            int n = s.Steps.Length;
            int minT = int.MaxValue;
            int maxT = int.MinValue;
            int value = 0;
            for (int i = 0; i < n; i++)
            {
                int t = s.Steps[i];
                if (t < minT)
                    minT = t;
                if (t > maxT)
                    maxT = t;
                var def = GameDefs.GetHero(s.Jobs[i]);
                value += (def != null ? def.BaseValue : 0) + t * BalanceConfig.HeroStatStepValue;
            }

            // 평균 강화 단계(= 단계 합 / 4) 격차 ≤ 1
            if (maxT - minT > StatCount)
                return;

            int diff = value > s.Target ? value - s.Target : s.Target - value;
            if (diff < s.NearestDiff)
            {
                s.NearestDiff = diff;
                s.Nearest = Snapshot(s, value);
            }

            if (diff > BalanceConfig.PartyValueTolerance)
                return;

            s.Count++;
            if (s.Rng.Range(0, s.Count) == 0)
                s.Picked = Snapshot(s, value);
        }

        static Choice Snapshot(Search s, int value)
        {
            var c = new Choice();
            c.Jobs = (string[])s.Jobs.Clone();
            c.Steps = (int[])s.Steps.Clone();
            c.Value = value;
            return c;
        }

        static List<string[]> JobMultisets(string[] pool, int size)
        {
            var result = new List<string[]>();
            if (pool == null || pool.Length == 0)
                return result;

            var indices = new int[size];
            Fill(pool, indices, 0, 0, result);
            return result;
        }

        static void Fill(string[] pool, int[] indices, int position, int start, List<string[]> result)
        {
            if (position == indices.Length)
            {
                var jobs = new string[indices.Length];
                for (int i = 0; i < indices.Length; i++)
                    jobs[i] = pool[indices[i]];

                if (indices.Length >= pool.Length)
                {
                    for (int p = 0; p < pool.Length; p++)
                    {
                        if (System.Array.IndexOf(jobs, pool[p]) < 0)
                            return;
                    }
                }

                result.Add(jobs);
                return;
            }

            for (int i = start; i < pool.Length; i++)
            {
                indices[position] = i;
                Fill(pool, indices, position + 1, i, result);
            }
        }

        static HeroPartyState Build(Choice choice, int target, GameRng rng, int maxAiLevel)
        {
            var party = new HeroPartyState();
            party.targetValue = target;
            if (choice == null)
                return party;

            for (int i = 0; i < choice.Jobs.Length; i++)
            {
                // levels: 0 HP / 1 공격력 / 2 공격속도 / 3 AI
                var levels = new int[StatCount];
                var caps = new[] { BalanceConfig.MaxHeroStatLevel, BalanceConfig.MaxHeroStatLevel, BalanceConfig.MaxHeroStatLevel, maxAiLevel };
                var candidates = new List<int>(StatCount);
                for (int step = 0; step < choice.Steps[i]; step++)
                {
                    // 낮은 단계부터, 동률이면 무작위 (상한에 닿은 후보 제외)
                    int lowest = int.MaxValue;
                    for (int k = 0; k < StatCount; k++)
                    {
                        if (levels[k] < caps[k] && levels[k] < lowest)
                            lowest = levels[k];
                    }

                    candidates.Clear();
                    for (int k = 0; k < StatCount; k++)
                    {
                        if (levels[k] < caps[k] && levels[k] == lowest)
                            candidates.Add(k);
                    }

                    if (candidates.Count == 0)
                        break;
                    levels[candidates[rng.Range(0, candidates.Count)]]++;
                }

                var hero = new HeroState();
                hero.jobId = choice.Jobs[i];
                hero.hpLevel = levels[0];
                hero.atkLevel = levels[1];
                hero.asLevel = levels[2];
                hero.aiLevel = levels[3];
                party.heroes.Add(hero);
                party.totalValue += hero.Value();
            }

            return party;
        }
    }
}
