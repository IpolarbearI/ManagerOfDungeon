using System;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 저장 가능한 난수(xorshift32). 상태가 GameState에 들어가므로
    /// 같은 세이브에서 시작하면 침입·파티·고민 결과가 똑같이 재현된다(QA 10-12 '난수 시드').
    /// </summary>
    [Serializable]
    public sealed class GameRng
    {
        public int state;

        public GameRng()
            : this(1)
        {
        }

        public GameRng(int seed)
        {
            state = seed == 0 ? unchecked((int)0x9E3779B9u) : seed;
        }

        public uint NextUInt()
        {
            uint x = unchecked((uint)state);
            if (x == 0)
                x = 0x9E3779B9u;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = unchecked((int)x);
            return x;
        }

        /// <summary>[min, max) 정수.</summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                return minInclusive;
            uint span = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % span);
        }

        /// <summary>[min, max) 실수.</summary>
        public float Range(float min, float max)
        {
            return min + (max - min) * Next01();
        }

        public float Next01()
        {
            return (NextUInt() & 0xFFFFFF) / 16777216f;
        }

        /// <summary>percent% 확률로 true.</summary>
        public bool Percent(int percent)
        {
            if (percent <= 0)
                return false;
            if (percent >= 100)
                return true;
            return Range(0, 100) < percent;
        }
    }
}
