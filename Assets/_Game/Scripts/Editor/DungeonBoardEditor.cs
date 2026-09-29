using UnityEditor;
using UnityEngine;

namespace ManagerOfDungeon
{
    [CustomEditor(typeof(DungeonBoard))]
    public sealed class DungeonBoardEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Rebuild Grid"))
                ((DungeonBoard)target).Rebuild();
        }
    }
}
