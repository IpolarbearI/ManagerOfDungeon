// QuestReporter.cs - reports a quest objective when this object's Health dies (serializable in scenes).
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Put on an enemy: when its Health dies, reports the given objective id to the QuestSystem.
    /// Lets level designers wire kill-quests without code.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class QuestReporter : MonoBehaviour
    {
        [Tooltip("Objective id reported on death, e.g. 'kill_room2'.")]
        public string objectiveId;

        void Awake()
        {
            GetComponent<Health>().onDeath.AddListener(() =>
            {
                if (QuestSystem.Instance != null && !string.IsNullOrEmpty(objectiveId))
                    QuestSystem.Instance.Report(objectiveId);
            });
        }
    }
}
