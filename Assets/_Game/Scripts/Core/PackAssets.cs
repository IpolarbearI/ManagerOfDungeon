using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ManagerOfDungeon
{
    public static class PackAssets
    {
        public const string GoblinPrefab =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/Prefabs/Characters/Goblin.prefab";
        public const string ArcherPrefab =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/Prefabs/Characters/Villager.prefab";
        public const string SpikeIdle =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/Sprites/Tiles/Traps/spikes_0.png";
        public const string SpikeActive =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/Sprites/Tiles/Traps/spikes_1.png";
        public const string OldWoodBow =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/UI/Icons/icons_ranged_1.png";
        public const string HeldBow =
            "Assets/_Game/ExternalAssets/Pixel Art Icon Pack - RPG/Texture/Weapon & Tool/Bow.png";
        public const string GoblinIdle =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/Sprites/Characters/goblin/idle/down_0.png";
        public const string ArcherIdle =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/Sprites/Characters/villager/idle/down_0.png";
        public const string UiFrame61 =
            "Assets/_Game/InternalAssets/Frame 61.png";
        public const string UiFrame64 =
            "Assets/_Game/InternalAssets/Frame 64.png";
        public const string UiFrame7 =
            "Assets/_Game/InternalAssets/Frame 7.png";
        public const string UiStorageTitle =
            "Assets/_Game/InternalAssets/보관함/보관함.png";
        public const string UiStorageGrid =
            "Assets/_Game/InternalAssets/보관함/Frame 6.png";
        public const string UiStorageTabs =
            "Assets/_Game/InternalAssets/보관함/Frame 11.png";
        public const string UiStorageDetail =
            "Assets/_Game/InternalAssets/보관함/Frame 29.png";
        public const string UiStorageScroll =
            "Assets/_Game/InternalAssets/보관함/image 83.png";
        public const string UiStorageThumb =
            "Assets/_Game/InternalAssets/보관함/image 84.png";
        public const string UiStorageChest =
            "Assets/_Game/InternalAssets/보관함/image 101.png";
        public const string UiStorageClose =
            "Assets/_Game/InternalAssets/보관함/_레이어_1.png";
        public const string UiLogTitle =
            "Assets/_Game/InternalAssets/운영기록/운영 기록.png";
        public const string UiLogTable =
            "Assets/_Game/InternalAssets/운영기록/Frame 79.png";
        public const string UiLogScroll =
            "Assets/_Game/InternalAssets/운영기록/image 83.png";
        public const string UiLogThumb =
            "Assets/_Game/InternalAssets/운영기록/image 84.png";
        public const string UiLogIcon =
            "Assets/_Game/InternalAssets/운영기록/image 118.png";
        public const string UiLogClose =
            "Assets/_Game/InternalAssets/운영기록/_레이어_1.png";

        static Material _spriteMaterial;
        static Sprite _frame61;
        static Sprite _frame64;
        static Sprite _frame64Box;
        static Sprite[] _frame7Buttons;
        static Sprite _storageTitle;
        static Sprite _storageGrid;
        static Sprite _storageTabs;
        static Sprite _storageDetail;
        static Sprite _storageScroll;
        static Sprite _storageThumb;
        static Sprite _storageChest;
        static Sprite _storageClose;
        static Sprite _logTitle;
        static Sprite _logTable;
        static Sprite _logScroll;
        static Sprite _logThumb;
        static Sprite _logIcon;
        static Sprite _logClose;

        public static Material SpriteMaterial()
        {
            if (_spriteMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                    _spriteMaterial = new Material(shader);
            }

            return _spriteMaterial;
        }

        public static void ApplySpriteMaterial(SpriteRenderer renderer)
        {
            if (renderer == null)
                return;

            var material = SpriteMaterial();
            if (material != null)
                renderer.sharedMaterial = material;
            renderer.enabled = true;
        }

        public static GameObject LoadPrefab(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
            return null;
#endif
        }

        public static Sprite LoadSprite(string path)
        {
#if UNITY_EDITOR
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                return sprite;

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                sprite = assets[i] as Sprite;
                if (sprite != null)
                    return sprite;
            }
#endif
            return null;
        }

        public static Sprite UiTopBar()
        {
            if (_frame61 == null)
                _frame61 = LoadFullSprite(UiFrame61);
            return _frame61;
        }

        public static Sprite UiButton()
        {
            if (_frame64 == null)
                _frame64 = LoadFullSprite(UiFrame64);
            return _frame64;
        }

        public static Sprite UiButtonBox()
        {
            if (_frame64Box == null)
            {
                _frame64Box = LoadNamedSprite(UiFrame64, "Frame 64_0");
                if (_frame64Box == null)
                    _frame64Box = UiButton();
            }

            return _frame64Box;
        }

        public static Sprite StorageTitle() => CacheFull(ref _storageTitle, UiStorageTitle);
        public static Sprite StorageGrid() => CacheFull(ref _storageGrid, UiStorageGrid);
        public static Sprite StorageTabs() => CacheFull(ref _storageTabs, UiStorageTabs);
        public static Sprite StorageDetail() => CacheFull(ref _storageDetail, UiStorageDetail);
        public static Sprite StorageScroll() => CacheFull(ref _storageScroll, UiStorageScroll);
        public static Sprite StorageThumb() => CacheFull(ref _storageThumb, UiStorageThumb);
        public static Sprite StorageChest() => CacheFull(ref _storageChest, UiStorageChest);
        public static Sprite StorageClose() => CacheFull(ref _storageClose, UiStorageClose);
        public static Sprite LogTitle() => CacheFull(ref _logTitle, UiLogTitle);
        public static Sprite LogTable() => CacheFull(ref _logTable, UiLogTable);
        public static Sprite LogScroll() => CacheFull(ref _logScroll, UiLogScroll);
        public static Sprite LogThumb() => CacheFull(ref _logThumb, UiLogThumb);
        public static Sprite LogIcon() => CacheFull(ref _logIcon, UiLogIcon);
        public static Sprite LogClose() => CacheFull(ref _logClose, UiLogClose);

        static Sprite CacheFull(ref Sprite cache, string path)
        {
            if (cache == null)
                cache = LoadFullSprite(path);
            return cache;
        }

        public static Sprite UiDesignIcon(int index)
        {
            EnsureFrame7Buttons();
            if (_frame7Buttons == null || _frame7Buttons.Length == 0)
                return null;

            if (index < 0)
                index = 0;
            if (index >= _frame7Buttons.Length)
                index = _frame7Buttons.Length - 1;
            return _frame7Buttons[index];
        }

        static void EnsureFrame7Buttons()
        {
            if (_frame7Buttons != null)
                return;

            _frame7Buttons = new Sprite[7];
#if UNITY_EDITOR
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(UiFrame7);
            if (tex == null)
                return;

            float[] x = { 0f, 81f, 164f, 242f, 330f, 412f, 495f };
            for (int i = 0; i < 7; i++)
            {
                float width = i < 6 ? x[i + 1] - x[i] : tex.width - x[i];
                if (width < 1f)
                    width = 60f;
                var sprite = Sprite.Create(
                    tex,
                    new Rect(x[i], 0f, width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                sprite.name = "Frame7_" + i;
                sprite.hideFlags = HideFlags.DontSave;
                _frame7Buttons[i] = sprite;
            }
#endif
        }

        public static Sprite LoadNamedSprite(string path, string spriteName)
        {
#if UNITY_EDITOR
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                var sprite = assets[i] as Sprite;
                if (sprite != null && sprite.name == spriteName)
                    return sprite;
            }
#endif
            return LoadSprite(path);
        }

        public static Sprite LoadFullSprite(string path)
        {
#if UNITY_EDITOR
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
                return null;

            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = System.IO.Path.GetFileNameWithoutExtension(path) + "_Full";
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
#else
            return LoadSprite(path);
#endif
        }
    }
}
