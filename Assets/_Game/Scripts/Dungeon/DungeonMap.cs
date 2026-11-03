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
            if (Application.isPlaying && GetComponent<AlphaDemoDirector>() == null)
                gameObject.AddComponent<AlphaDemoDirector>();

            if (GetComponent<DungeonRoomScreen>() == null)
                gameObject.AddComponent<DungeonRoomScreen>();
        }
    }
}
