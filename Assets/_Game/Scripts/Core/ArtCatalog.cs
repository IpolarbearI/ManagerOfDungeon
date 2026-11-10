using System.Collections.Generic;
using UnityEngine;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 빌드(.exe)에서 AssetDatabase 대신 쓰는 에셋 참조 목록.
    /// 에디터 메뉴 "Manager of Dungeon/Rebuild Art Catalog" 또는 빌드 직전에 자동으로 갱신된다.
    /// 위치: Assets/_Game/Resources/ArtCatalog.asset
    /// </summary>
    public sealed class ArtCatalog : ScriptableObject
    {
        public const string ResourcePath = "ArtCatalog";

        [System.Serializable]
        public sealed class Entry
        {
            public string path;
            public Object main;
            public Object[] subAssets;
        }

        [SerializeField] List<Entry> entries = new List<Entry>();

        static ArtCatalog _instance;
        static bool _loaded;
        Dictionary<string, Entry> _byPath;

        public List<Entry> Entries => entries;

        public static ArtCatalog Instance
        {
            get
            {
                if (!_loaded || _instance == null)
                {
                    _loaded = true;
                    _instance = Resources.Load<ArtCatalog>(ResourcePath);
                }

                return _instance;
            }
        }

        public void SetEntries(List<Entry> list)
        {
            entries = list ?? new List<Entry>();
            _byPath = null;
        }

        public static Entry Find(string path)
        {
            var catalog = Instance;
            if (catalog == null || string.IsNullOrEmpty(path))
                return null;
            return catalog.Lookup(path);
        }

        public static T Main<T>(string path) where T : Object
        {
            var entry = Find(path);
            return entry != null ? entry.main as T : null;
        }

        public static Sprite FirstSprite(string path)
        {
            var entry = Find(path);
            if (entry == null)
                return null;

            var main = entry.main as Sprite;
            if (main != null)
                return main;

            if (entry.subAssets == null)
                return null;

            for (int i = 0; i < entry.subAssets.Length; i++)
            {
                var sprite = entry.subAssets[i] as Sprite;
                if (sprite != null)
                    return sprite;
            }

            return null;
        }

        public static Sprite NamedSprite(string path, string spriteName)
        {
            var entry = Find(path);
            if (entry == null || entry.subAssets == null)
                return null;

            for (int i = 0; i < entry.subAssets.Length; i++)
            {
                var sprite = entry.subAssets[i] as Sprite;
                if (sprite != null && sprite.name == spriteName)
                    return sprite;
            }

            return null;
        }

        public static Texture2D Texture(string path)
        {
            var entry = Find(path);
            if (entry == null)
                return null;

            var tex = entry.main as Texture2D;
            if (tex != null)
                return tex;

            var sprite = FirstSprite(path);
            return sprite != null ? sprite.texture : null;
        }

        Entry Lookup(string path)
        {
            if (_byPath == null)
            {
                _byPath = new Dictionary<string, Entry>();
                for (int i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    if (e != null && !string.IsNullOrEmpty(e.path))
                        _byPath[e.path] = e;
                }
            }

            _byPath.TryGetValue(path, out var entry);
            return entry;
        }
    }
}
