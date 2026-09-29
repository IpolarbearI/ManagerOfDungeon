using UnityEngine;

namespace ManagerOfDungeon
{
    public sealed class DungeonGrid
    {
        public int Width { get; }
        public int Height { get; }

        readonly DungeonTileDefinition[,] _tiles;

        public DungeonGrid(int width, int height, DungeonTileDefinition fill)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            _tiles = new DungeonTileDefinition[Width, Height];
            Fill(fill);
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public bool InBounds(Vector2Int coord) => InBounds(coord.x, coord.y);

        public DungeonTileDefinition GetTile(int x, int y)
        {
            return InBounds(x, y) ? _tiles[x, y] : null;
        }

        public DungeonTileDefinition GetTile(Vector2Int coord) => GetTile(coord.x, coord.y);

        public bool SetTile(int x, int y, DungeonTileDefinition tile)
        {
            if (!InBounds(x, y))
                return false;

            _tiles[x, y] = tile;
            return true;
        }

        public bool SetTile(Vector2Int coord, DungeonTileDefinition tile) => SetTile(coord.x, coord.y, tile);

        public void Fill(DungeonTileDefinition tile)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                    _tiles[x, y] = tile;
            }
        }

        public void ApplyEnterEffects(Vector2Int coord)
        {
            var tile = GetTile(coord);
            if (tile == null)
                return;

            tile.RaiseRaidEnter(new TileContext(this, coord));
        }
    }
}
