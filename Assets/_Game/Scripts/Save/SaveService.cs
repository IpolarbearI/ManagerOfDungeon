using System.IO;
using UnityEngine;

namespace ManagerOfDungeon
{
    /// <summary>
    /// GameState를 JSON으로 저장/불러오기 (04-14).
    /// 슬롯 0 = 자동저장(매일 06:00), 슬롯 1 = 수동저장(ESC).
    /// 위치: Application.persistentDataPath/slot_N.json
    /// </summary>
    public static class SaveService
    {
        public const int AutoSlot = 0;
        public const int ManualSlot = 1;
        public const int SlotCount = 2;

        public static string PathFor(int slot)
        {
            return Path.Combine(Application.persistentDataPath, "slot_" + slot + ".json");
        }

        public static bool Exists(int slot)
        {
            return File.Exists(PathFor(slot));
        }

        public static bool HasAnySave()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (Exists(i))
                    return true;
            }

            return false;
        }

        public static bool Save(GameState state, int slot)
        {
            if (state == null)
                return false;

            try
            {
                state.saveVersion = GameState.CurrentSaveVersion;
                string json = JsonUtility.ToJson(state, true);
                string path = PathFor(slot);
                string temp = path + ".tmp";
                File.WriteAllText(temp, json);
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(temp, path);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Save] 저장 실패: " + e.Message);
                return false;
            }
        }

        public static GameState Load(int slot)
        {
            try
            {
                string path = PathFor(slot);
                if (!File.Exists(path))
                    return null;

                var state = JsonUtility.FromJson<GameState>(File.ReadAllText(path));
                return Migrate(state);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Save] 불러오기 실패: " + e.Message);
                return null;
            }
        }

        /// <summary>가장 최근에 쓴 슬롯을 불러온다(이어하기).</summary>
        public static GameState LoadLatest()
        {
            int best = -1;
            System.DateTime bestTime = System.DateTime.MinValue;
            for (int i = 0; i < SlotCount; i++)
            {
                if (!Exists(i))
                    continue;
                var time = File.GetLastWriteTimeUtc(PathFor(i));
                if (time > bestTime)
                {
                    bestTime = time;
                    best = i;
                }
            }

            return best >= 0 ? Load(best) : null;
        }

        public static void DeleteAll()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (Exists(i))
                    File.Delete(PathFor(i));
            }
        }

        static GameState Migrate(GameState state)
        {
            if (state == null)
                return null;

            // saveVersion이 오르면 여기서 이전 형식을 변환한다.
            if (state.rng == null)
                state.rng = new GameRng(System.Environment.TickCount);
            if (state.invasion == null)
                state.invasion = new InvasionState();
            if (state.invasion.party == null)
                state.invasion.party = new HeroPartyState();
            state.saveVersion = GameState.CurrentSaveVersion;
            return state;
        }
    }
}
