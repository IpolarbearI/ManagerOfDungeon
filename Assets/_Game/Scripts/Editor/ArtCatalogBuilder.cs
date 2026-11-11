using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ManagerOfDungeon.EditorTools
{
    /// <summary>
    /// PackAssets의 "Assets/..." 경로 상수를 모두 모아 ArtCatalog에 실제 에셋 참조로 저장한다.
    /// 빌드 직전에 자동 실행되므로 평소에는 신경 쓰지 않아도 된다.
    /// </summary>
    public sealed class ArtCatalogBuilder : IPreprocessBuildWithReport
    {
        public const string FolderParent = "Assets/_Game";
        public const string FolderName = "Resources";
        public const string AssetPath = "Assets/_Game/Resources/ArtCatalog.asset";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Rebuild();
        }

        [MenuItem("Manager of Dungeon/Rebuild Art Catalog")]
        public static void Rebuild()
        {
            if (!AssetDatabase.IsValidFolder(FolderParent + "/" + FolderName))
                AssetDatabase.CreateFolder(FolderParent, FolderName);

            var catalog = AssetDatabase.LoadAssetAtPath<ArtCatalog>(AssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ArtCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }

            var list = new List<ArtCatalog.Entry>();
            var paths = CollectPaths();
            for (int i = 0; i < paths.Count; i++)
            {
                string path = paths[i];
                var main = AssetDatabase.LoadMainAssetAtPath(path);
                if (main == null)
                {
                    Debug.LogWarning("[ArtCatalog] 에셋을 찾을 수 없음: " + path);
                    continue;
                }

                var subs = new List<Object>();
                var all = AssetDatabase.LoadAllAssetsAtPath(path);
                for (int k = 0; k < all.Length; k++)
                {
                    var asset = all[k];
                    if (asset == null || asset == main || asset is Component || asset is GameObject)
                        continue;
                    subs.Add(asset);
                }

                var entry = new ArtCatalog.Entry();
                entry.path = path;
                entry.main = main;
                entry.subAssets = subs.ToArray();
                list.Add(entry);
            }

            catalog.SetEntries(list);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("[ArtCatalog] " + list.Count + "개 에셋 등록 완료");
        }

        static List<string> CollectPaths()
        {
            var result = new List<string>();
            var fields = typeof(PackAssets).GetFields(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!field.IsLiteral || field.FieldType != typeof(string))
                    continue;

                var value = field.GetRawConstantValue() as string;
                if (string.IsNullOrEmpty(value) || !value.StartsWith("Assets/"))
                    continue;
                if (!result.Contains(value))
                    result.Add(value);
            }

            return result;
        }
    }

    [InitializeOnLoad]
    static class ArtCatalogAutoBuild
    {
        static ArtCatalogAutoBuild()
        {
            EditorApplication.delayCall += () =>
            {
                if (AssetDatabase.LoadAssetAtPath<ArtCatalog>(ArtCatalogBuilder.AssetPath) == null)
                    ArtCatalogBuilder.Rebuild();
            };
        }
    }
}
