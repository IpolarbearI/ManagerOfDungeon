// InteractionSystem.cs - unified interaction (E key) for chests, NPCs, doors, levers.
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    public interface IInteractable
    {
        string Prompt { get; }
        void Interact(GameObject interactor);
    }

    // Put on the player. Finds the nearest IInteractable in range and triggers it on E.
    /// <summary>
    /// Player-side interaction: finds the nearest IInteractable in radius and triggers it on E. Shows a [E] prompt via OnGUI.
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        public float radius = 1.3f;
        Component _nearest;
        string _prompt = "";

        void Update()
        {
            FindNearest();
            if (_nearest != null && InteractPressed())
                ((IInteractable)_nearest).Interact(gameObject);
        }

        void FindNearest()
        {
            _nearest = null; _prompt = "";
            var hits = Physics2D.OverlapCircleAll(transform.position, radius);
            float best = float.MaxValue;
            foreach (var h in hits)
            {
                var it = h.GetComponent<IInteractable>();
                if (it == null) continue;
                float d = Vector2.Distance(transform.position, h.transform.position);
                if (d < best) { best = d; _nearest = (Component)it; _prompt = it.Prompt; }
            }
        }

        void OnGUI()
        {
            if (_nearest != null)
                GUI.Label(new Rect(Screen.width / 2 - 80, Screen.height - 60, 200, 24), "[E] " + _prompt);
        }

        bool InteractPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
    }

    // Sample: a chest that drops loot once when interacted.
    [RequireComponent(typeof(Collider2D))]
    /// <summary>
    /// Sample interactable chest: opens once, swaps sprite and gives loot from a LootTableSO straight to the interactor's inventory.
    /// </summary>
    public class ChestInteractable : MonoBehaviour, IInteractable
    {
        public LootTableSO loot;
        public Sprite openedSprite;
        bool _opened;

        public string Prompt => _opened ? "Empty" : "Open chest";

        public void Interact(GameObject interactor)
        {
            AudioManager.PlaySFX("chest");
            if (_opened) return;
            _opened = true;
            if (openedSprite != null) { var sr = GetComponent<SpriteRenderer>(); if (sr) sr.sprite = openedSprite; }
            if (loot != null)
            {
                foreach (var kv in loot.Roll())
                {
                    var inv = interactor.GetComponent<InventorySystem>();
                    if (inv != null) inv.Add(kv.Key, kv.Value);
                }
            }
        }
    }
}
