using System.Collections.Generic;
using UnityEngine;

namespace ManagerOfDungeon
{
    [CreateAssetMenu(menuName = "ManagerOfDungeon/Dungeon Tile", fileName = "Tile_")]
    public sealed class DungeonTileDefinition : ScriptableObject
    {
        [SerializeField] string id = "floor";
        [SerializeField] string displayName = "Floor";
        [SerializeField] Color tint = new Color(0.35f, 0.38f, 0.42f, 1f);
        [SerializeField] bool walkable = true;
        [SerializeField] List<TileEffect> effects = new List<TileEffect>();

        public string Id => id;
        public string DisplayName => displayName;
        public Color Tint => tint;
        public bool Walkable => walkable;
        public IReadOnlyList<TileEffect> Effects => effects;

        public void RaiseRaidEnter(in TileContext context)
        {
            if (effects == null)
                return;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                    effects[i].OnRaidEnter(context);
            }
        }

        public void RaiseRaidStay(in TileContext context)
        {
            if (effects == null)
                return;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                    effects[i].OnRaidStay(context);
            }
        }

        public void RaiseRaidExit(in TileContext context)
        {
            if (effects == null)
                return;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                    effects[i].OnRaidExit(context);
            }
        }
    }
}
