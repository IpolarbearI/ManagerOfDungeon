// StoryTrigger.cs - one-shot trigger zone: boss intro, BGM change, dialogue or speech bubble when the player enters.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Invisible trigger area. When the player enters (once): optionally plays a boss intro,
    /// switches BGM, shows a dialogue or a speech bubble. Build with a BoxCollider2D (isTrigger).
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class StoryTrigger : MonoBehaviour
    {
        [Header("Boss intro (optional)")]
        public string bossIntroName = "";
        public Transform bossTarget;

        [Header("Music (optional)")]
        public string bgmName = "";

        [Header("Dialogue (optional)")]
        public string speaker = "";
        public Sprite portrait;
        [TextArea] public string[] lines;

        [Header("Bubble (optional)")]
        [TextArea] public string bubbleText = "";
        public Transform bubbleTarget;

        bool _fired;

        void Reset() { GetComponent<BoxCollider2D>().isTrigger = true; }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_fired || !other.CompareTag("Player")) return;
            _fired = true;

            if (!string.IsNullOrEmpty(bgmName)) AudioManager.PlayBGM(bgmName);
            if (!string.IsNullOrEmpty(bossIntroName) && bossTarget != null)
                BossIntro.Play(bossIntroName, bossTarget);
            if (lines != null && lines.Length > 0 && !DialogueBoxUI.IsOpen)
                DialogueBoxUI.Show(string.IsNullOrEmpty(speaker) ? "???" : speaker, lines, portrait);
            if (!string.IsNullOrEmpty(bubbleText))
                SpeechBubble.Say(bubbleTarget != null ? bubbleTarget : other.transform, bubbleText, 2.6f);
        }
    }
}
