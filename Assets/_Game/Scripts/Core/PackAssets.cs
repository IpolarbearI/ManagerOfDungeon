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
        public const string ScrapSwordIcon =
            "Assets/_Game/ExternalAssets/Pixel Art Icon Pack - RPG/Texture/Weapon & Tool/Iron Sword.png";
        public const string GoblinIdle =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/Sprites/Characters/goblin/idle/down_0.png";
        public const string ArcherIdle =
            "Assets/_Game/ExternalAssets/Fantasy_Mega_Pack/Sprites/Characters/villager/idle/down_0.png";
        public const string UiFrame61 =
            "Assets/_Game/InternalAssets/Frame 61.png";
        /// <summary>상단 바 아트에서 Day·시간·솔·자산 숫자를 지운 버전(숫자는 코드가 실시간으로 그림).</summary>
        public const string UiFrame61Hud =
            "Assets/_Game/InternalAssets/Frame 61_hud.png";
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
        public const string UiLobbyLogo =
            "Assets/_Game/InternalAssets/_레이어_1.png";
        public const string UiDungeonMap =
            "Assets/_Game/InternalAssets/dungeon_1920x1080.png";
        public const string UiGoblinFront =
            "Assets/_Game/InternalAssets/goblin_front.png";
        public const string UiDesk =
            "Assets/_Game/InternalAssets/desk.png";
        public const string UiInteractionBackground =
            "Assets/_Game/InternalAssets/interaction_background_1920x1080.png";
        public const string AudioDungeon =
            "Assets/_Game/Audio/Music/dungeon.mp3";
        public const string AudioBattle =
            "Assets/_Game/Audio/Music/battle.mp3";
        public const string WarriorPrefab =
            "Assets/_Game/ExternalAssets/Hero Knight - Pixel Art/Demo/HeroKnight.prefab";
        public const string KoreanFontPath =
            "Assets/_Game/Fonts/MalgunGothic.ttf";

        static Font _koreanFont;

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
        static Sprite _lobbyLogo;
        static Sprite _dungeonMap;
        static Sprite _goblinFront;
        static Sprite _desk;
        static Sprite _interactionBackground;

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
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                return prefab;
#endif
            return ArtCatalog.Main<GameObject>(path);
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
            return ArtCatalog.FirstSprite(path);
        }

        public static Sprite UiTopBar()
        {
            if (_frame61 == null)
                _frame61 = LoadFullSprite(UiFrame61Hud);
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
        public static Sprite LobbyLogo() => CacheFull(ref _lobbyLogo, UiLobbyLogo);
        public static Sprite DungeonMap() => CacheFull(ref _dungeonMap, UiDungeonMap);
        public static Sprite GoblinFront() => CacheFull(ref _goblinFront, UiGoblinFront);
        public static Sprite Desk() => CacheFull(ref _desk, UiDesk);
        public static Sprite InteractionBackground() => CacheFull(ref _interactionBackground, UiInteractionBackground);

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
            var tex = LoadTexture(UiFrame7);
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
        }

        public static Texture2D LoadTexture(string path)
        {
#if UNITY_EDITOR
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
                return tex;
#endif
            return ArtCatalog.Texture(path);
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
#else
            var named = ArtCatalog.NamedSprite(path, spriteName);
            if (named != null)
                return named;
#endif
            return LoadSprite(path);
        }

        public static Sprite LoadFullSprite(string path)
        {
            var tex = LoadTexture(path);
            if (tex == null)
                return LoadSprite(path);

            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = System.IO.Path.GetFileNameWithoutExtension(path) + "_Full";
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        public static Font KoreanFont()
        {
            if (_koreanFont != null)
                return _koreanFont;
#if UNITY_EDITOR
            _koreanFont = AssetDatabase.LoadAssetAtPath<Font>(KoreanFontPath);
#endif
            if (_koreanFont == null)
                _koreanFont = ArtCatalog.Main<Font>(KoreanFontPath);
            if (_koreanFont == null)
            {
                _koreanFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Malgun Gothic", "맑은 고딕", "Gulim", "Dotum", "Arial Unicode MS", "Arial" },
                    40);
            }

            return _koreanFont;
        }

        public static AudioClip DungeonBgm() => LoadAudio(AudioDungeon);
        public static AudioClip BattleBgm() => LoadAudio(AudioBattle);

        public static AudioClip LoadAudio(string path)
        {
#if UNITY_EDITOR
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
                return clip;
#endif
            return ArtCatalog.Main<AudioClip>(path);
        }
    }
}
