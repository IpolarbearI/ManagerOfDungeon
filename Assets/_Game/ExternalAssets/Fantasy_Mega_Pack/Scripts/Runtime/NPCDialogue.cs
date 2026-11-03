// NPCDialogue.cs - talkable NPC: E to open a story dialogue (DialogueBoxUI), ambient speech bubbles,
// optional quest hook on first conversation, separate repeat lines for later visits.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Makes an NPC talkable via the InteractionSystem (E key). First conversation can start a quest
    /// objective; later talks show shorter repeat lines (classic JRPG pattern). Also emits ambient
    /// speech bubbles when the player is nearby.
    /// </summary>
    public class NPCDialogue : MonoBehaviour, IInteractable
    {
        [Header("Identity")]
        public string speakerName = "Villager";
        public Sprite portrait;

        [Header("Dialogue")]
        [TextArea] public string[] firstLines = { "Hello traveler!" };
        [TextArea] public string[] repeatLines = { "Good luck out there." };

        [Header("Quest hook (optional)")]
        [Tooltip("Objective started in the QuestSystem after the FIRST conversation. Leave empty for none.")]
        public string startObjectiveId = "";
        public string startObjectiveTitle = "";
        public int startObjectiveRequired = 1;
        [Tooltip("Objective REPORTED (progress +1) on first conversation - e.g. 'find the prisoner'.")]
        public string reportObjectiveId = "";
        [Tooltip("Lines shown only once the given objective is done (e.g. quest turn-in).")]
        public string doneObjectiveId = "";
        [TextArea] public string[] doneLines;

        [Header("Ambient bubble (optional)")]
        [TextArea] public string[] bubbleLines;
        public float bubbleInterval = 6f;
        public float bubbleRange = 5f;

        bool _talked;
        float _bubbleTimer;
        Transform _player;

        public string Prompt => "Talk to " + speakerName;

        public void Interact(GameObject interactor)
        {
            if (DialogueBoxUI.IsOpen) return;
            string[] lines = firstLines;
            System.Action onDone = null;

            var qs = QuestSystem.Instance;
            bool questDone = qs != null && !string.IsNullOrEmpty(doneObjectiveId) &&
                             qs.objectives.Exists(o => o.id == doneObjectiveId && o.IsDone);
            if (questDone && doneLines != null && doneLines.Length > 0)
            {
                lines = doneLines;
            }
            else if (_talked)
            {
                lines = repeatLines;
            }
            else
            {
                _talked = true;
                if (!string.IsNullOrEmpty(reportObjectiveId) && qs != null)
                    qs.Report(reportObjectiveId);
                if (!string.IsNullOrEmpty(startObjectiveId) && qs != null)
                {
                    string id = startObjectiveId, title = startObjectiveTitle;
                    int req = startObjectiveRequired;
                    onDone = () =>
                    {
                        if (!qs.objectives.Exists(o => o.id == id))
                        {
                            qs.AddObjective(id, title, req);
                            AudioManager.PlaySFX("quest");
                        }
                    };
                }
            }
            DialogueBoxUI.Show(speakerName, lines, portrait, onDone);
        }

        void Update()
        {
            if (bubbleLines == null || bubbleLines.Length == 0) return;
            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _player = p.transform;
                return;
            }
            if (DialogueBoxUI.IsOpen) { _bubbleTimer = 2f; return; }
            if (Vector2.Distance(_player.position, transform.position) > bubbleRange) return;
            _bubbleTimer -= Time.deltaTime;
            if (_bubbleTimer <= 0f)
            {
                _bubbleTimer = bubbleInterval + Random.Range(0f, 2.5f);
                SpeechBubble.Say(transform, bubbleLines[Random.Range(0, bubbleLines.Length)], 2.4f);
            }
        }
    }
}
