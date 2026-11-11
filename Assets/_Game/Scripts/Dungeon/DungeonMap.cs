using UnityEngine;

namespace ManagerOfDungeon
{
    [ExecuteAlways]
    public sealed class DungeonMap : MonoBehaviour
    {
        void OnEnable()
        {
            var oldLayout = transform.Find("Layout");
            if (oldLayout != null)
            {
                if (Application.isPlaying)
                    Destroy(oldLayout.gameObject);
                else
                    DestroyImmediate(oldLayout.gameObject);
            }

            var legacyCombat = GetComponent<DungeonCombat>();
            if (legacyCombat != null)
                legacyCombat.enabled = false;

            if (Application.isPlaying && GetComponent<CombatDirector>() == null)
                gameObject.AddComponent<CombatDirector>();

            // 화면(DungeonRoomScreen)은 하나. 움직이는 쪽만 모드에 따라 다르다.
            //  - 본 게임(A2): 로비 '새 게임'/'이어하기' → GameDirector
            //  - A1.01 쇼케이스: 로비 '튜토리얼' 또는 Dungeon 씬 직접 실행 → AlphaDemoDirector
            if (Application.isPlaying)
            {
                if (GameSession.IsGame)
                {
                    if (GetComponent<GameDirector>() == null)
                        gameObject.AddComponent<GameDirector>();
                }
                else if (GetComponent<AlphaDemoDirector>() == null)
                {
                    gameObject.AddComponent<AlphaDemoDirector>();
                }
            }

            if (GetComponent<DungeonRoomScreen>() == null)
                gameObject.AddComponent<DungeonRoomScreen>();
        }
    }
}
