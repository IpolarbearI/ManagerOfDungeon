using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManagerOfDungeon.EditorTools
{
    /// <summary>
    /// 메뉴: Manager of Dungeon/Combat Test Scene
    /// 전투 테스트 씬이 없으면 만들고(카메라 + CombatTestBench), 있으면 연다. 열린 뒤 Play를 누르면 된다.
    /// </summary>
    public static class CombatTestSceneMenu
    {
        const string Folder = "Assets/_Game/Scenes/Test";
        const string ScenePath = Folder + "/CombatTest.unity";

        [MenuItem("Manager of Dungeon/Combat Test Scene")]
        public static void OpenOrCreate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/_Game/Scenes"))
                AssetDatabase.CreateFolder("Assets/_Game", "Scenes");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Game/Scenes", "Test");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.625f; // Dungeon 씬과 같은 크기
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.10f, 0.12f, 1f);
            camGo.AddComponent<AudioListener>();

            var bench = new GameObject("CombatTestBench");
            bench.AddComponent<CombatTestBench>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[CombatTest] 씬 생성: " + ScenePath + " — Play를 누르면 테스트를 시작합니다.");
        }
    }
}
