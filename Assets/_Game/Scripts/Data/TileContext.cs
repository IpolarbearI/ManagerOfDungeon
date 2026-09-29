using UnityEngine;

namespace ManagerOfDungeon
{
    public readonly struct TileContext
    {
        public readonly DungeonGrid Grid;
        public readonly Vector2Int Coord;

        public TileContext(DungeonGrid grid, Vector2Int coord)
        {
            Grid = grid;
            Coord = coord;
        }
    }
}
