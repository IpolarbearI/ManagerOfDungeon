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

        static Material _spriteMaterial;

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
    }
}
