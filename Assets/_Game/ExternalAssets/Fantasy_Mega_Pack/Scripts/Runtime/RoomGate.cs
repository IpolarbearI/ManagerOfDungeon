// RoomGate.cs - a gate/door that blocks the way until a quest objective is complete.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Blocking gate that opens (disappears or swaps sprite) when the given quest objective is done.
    /// Put it on a sprite with a (non-trigger) Collider2D across a corridor.
    /// </summary>
    public class RoomGate : MonoBehaviour
    {
        [Tooltip("Quest objective id that opens this gate.")]
        public string requiredObjective;
        [Tooltip("Optional open sprite; if empty the gate object is disabled when opened.")]
        public Sprite openSprite;

        bool _open;

        void Update()
        {
            if (_open || QuestSystem.Instance == null) return;
            foreach (var o in QuestSystem.Instance.objectives)
            {
                if (o.id == requiredObjective && o.IsDone) { Open(); return; }
            }
        }

        void Open()
        {
            _open = true;
            var col = GetComponent<Collider2D>();
            AudioManager.PlaySFX("gate");
            if (col) col.enabled = false;
            var sr = GetComponent<SpriteRenderer>();
            if (openSprite != null && sr != null) sr.sprite = openSprite;
            else if (sr != null) { var c = sr.color; c.a = 0.25f; sr.color = c; }
        }
    }
}
