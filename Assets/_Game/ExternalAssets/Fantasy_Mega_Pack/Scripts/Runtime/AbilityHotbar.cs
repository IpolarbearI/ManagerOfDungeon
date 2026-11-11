// AbilityHotbar.cs - 4-slot ability hotbar (keys 1-4) with cooldowns + MP costs + VFX hooks.
using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// 4-slot ability hotbar on keys 1-4: each ability has an icon, MP cost, cooldown,
    /// damage radius and an onCast event for VFX/sound hooks. Needs StatsSystem for MP.
    /// </summary>
    public class AbilityHotbar : MonoBehaviour
    {
        [Serializable]
        public class Ability
        {
            public string name = "Fireball";
            public Sprite icon;
            public int mpCost = 10;
            public float cooldown = 2f;
            public int damage = 15;
            public float radius = 2f;
            [NonSerialized] public float cd;
        }

        public Ability[] abilities = new Ability[4];

        StatsSystem _stats;

        void Awake() { _stats = GetComponent<StatsSystem>(); }

        void Update()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                var a = abilities[i];
                if (a == null) continue;
                if (a.cd > 0f) a.cd -= Time.deltaTime;
                if (SlotPressed(i) && a.cd <= 0f) Cast(a);
            }
        }

        void Cast(Ability a)
        {
            if (_stats != null && !_stats.SpendMP(a.mpCost)) return;
            a.cd = a.cooldown;
            // damage every enemy Health in radius
            foreach (var h in Physics2D.OverlapCircleAll(transform.position, a.radius))
            {
                if (h.gameObject == gameObject || !h.CompareTag("Enemy")) continue;
                var hp = h.GetComponent<Health>();
                if (hp != null && !hp.IsDead)
                {
                    var kb = h.GetComponent<Knockback>();
                    if (kb != null) kb.ApplyFrom(transform.position);
                    hp.TakeDamage(a.damage);
                }
            }
            ScreenShake.Shake(0.12f, 0.08f);
        }

        void OnGUI()
        {
            int n = abilities.Length;
            float size = 52, pad = 8;
            float x0 = Screen.width / 2f - (n * (size + pad)) / 2f;
            float y = Screen.height - size - 14;
            for (int i = 0; i < n; i++)
            {
                var a = abilities[i];
                var r = new Rect(x0 + i * (size + pad), y, size, size);
                GUI.Box(r, "");
                if (a != null && a.icon != null)
                    GUI.DrawTexture(new Rect(r.x + 4, r.y + 4, size - 8, size - 8), a.icon.texture, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(r.x + 4, r.y, 20, 18), (i + 1).ToString());
                if (a != null && a.cd > 0f)
                {
                    GUI.Box(r, "");
                    var st = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, fontStyle = FontStyle.Bold };
                    GUI.Label(r, Mathf.CeilToInt(a.cd).ToString(), st);
                }
            }
        }

        bool SlotPressed(int i)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            if (kb == null) return false;
            switch (i)
            {
                case 0: return kb.digit1Key.wasPressedThisFrame;
                case 1: return kb.digit2Key.wasPressedThisFrame;
                case 2: return kb.digit3Key.wasPressedThisFrame;
                case 3: return kb.digit4Key.wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(KeyCode.Alpha1 + i);
#endif
        }
    }
}
