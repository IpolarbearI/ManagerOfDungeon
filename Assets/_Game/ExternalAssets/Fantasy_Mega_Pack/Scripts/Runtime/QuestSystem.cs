// QuestSystem.cs - lightweight mission/objective tracker with on-screen HUD (OnGUI, zero dependencies).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Lightweight mission tracker. Define objectives in code or inspector, report progress
    /// with QuestSystem.Instance.Report("kill_skeleton"), and the HUD updates automatically.
    /// Fires onQuestComplete when an objective finishes and onAllComplete at the end.
    /// </summary>
    public class QuestSystem : MonoBehaviour
    {
        [Serializable]
        public class Objective
        {
            public string id;
            [Tooltip("Shown in the HUD, e.g. 'Defeat 3 skeletons'")] public string title;
            public int required = 1;
            [NonSerialized] public int current;
            public bool IsDone => current >= required;
        }

        public static QuestSystem Instance;

        public List<Objective> objectives = new List<Objective>();
        public event Action<Objective> onQuestComplete;
        public event Action onAllComplete;

        bool _allDone;

        void Awake() { Instance = this; }

        /// <summary>Adds an objective at runtime.</summary>
        public Objective AddObjective(string id, string title, int required = 1)
        {
            var o = new Objective { id = id, title = title, required = required };
            objectives.Add(o);
            return o;
        }

        /// <summary>Reports progress for an objective id (e.g. when an enemy dies).</summary>
        public void Report(string id, int amount = 1)
        {
            bool changed = false;
            foreach (var o in objectives)
            {
                if (o.id != id || o.IsDone) continue;
                o.current = Mathf.Min(o.required, o.current + amount);
                changed = true;
                if (o.IsDone) onQuestComplete?.Invoke(o);
            }
            if (!changed || _allDone) return;
            foreach (var o in objectives) if (!o.IsDone) return;
            _allDone = true;
            onAllComplete?.Invoke();
        }

        public bool AllDone => _allDone;

        void OnGUI()
        {
            if (objectives.Count == 0) return;
            var box = new Rect(12, 12, 320, 30 + objectives.Count * 26);
            GUI.Box(box, "");
            var head = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(24, 16, 300, 22), "OBJECTIVES", head);
            var st = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            for (int i = 0; i < objectives.Count; i++)
            {
                var o = objectives[i];
                string check = o.IsDone ? "[x] " : "[  ] ";
                GUI.Label(new Rect(24, 40 + i * 26, 300, 22), check + o.title + "  (" + o.current + "/" + o.required + ")", st);
            }
        }
    }
}
