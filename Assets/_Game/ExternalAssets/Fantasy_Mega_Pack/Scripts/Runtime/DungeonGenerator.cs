// DungeonGenerator.cs - runtime procedural dungeon: rooms + corridors + enemies + chests, new every run.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Procedural dungeon generator (runtime): generates N connected rooms with corridors
    /// on the assigned Tilemaps, then spawns enemies + chests. Assign floor/wall tiles,
    /// enemy prefabs and (optionally) a chest loot table — press Play, get a new dungeon every run.
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Tilemaps")]
        public Tilemap floorMap;
        public Tilemap wallMap;

        [Header("Tiles")]
        public TileBase floorTile;
        public TileBase wallTile;

        [Header("Layout")]
        public int roomCount = 6;
        public Vector2Int roomSizeMin = new Vector2Int(6, 5);
        public Vector2Int roomSizeMax = new Vector2Int(11, 9);
        public int seed = 0;

        [Header("Population")]
        public GameObject[] enemyPrefabs;
        public int enemiesPerRoom = 2;
        public Sprite chestSprite;
        public LootTableSO chestLoot;
        [Range(0f, 1f)] public float chestChance = 0.5f;

        [Header("Player")]
        public Transform player;

        readonly List<RectInt> _rooms = new List<RectInt>();
        readonly HashSet<Vector2Int> _floor = new HashSet<Vector2Int>();

        void Start() { Generate(); }

        /// <summary>Clears the maps and generates a fresh dungeon.</summary>
        public void Generate()
        {
            var rng = seed != 0 ? new System.Random(seed) : new System.Random();
            _rooms.Clear(); _floor.Clear();
            if (floorMap != null) floorMap.ClearAllTiles();
            if (wallMap != null) wallMap.ClearAllTiles();

            // place rooms on a random walk so they always connect
            Vector2Int cursor = Vector2Int.zero;
            for (int i = 0; i < roomCount; i++)
            {
                int w = rng.Next(roomSizeMin.x, roomSizeMax.x + 1);
                int h = rng.Next(roomSizeMin.y, roomSizeMax.y + 1);
                var room = new RectInt(cursor.x - w / 2, cursor.y - h / 2, w, h);
                _rooms.Add(room);
                for (int x = room.xMin; x < room.xMax; x++)
                    for (int y = room.yMin; y < room.yMax; y++)
                        _floor.Add(new Vector2Int(x, y));
                // corridor to the next room position
                Vector2Int dir = new[] { Vector2Int.right, Vector2Int.right, Vector2Int.up, Vector2Int.down }[rng.Next(4)];
                Vector2Int next = cursor + dir * (Mathf.Max(w, h) + rng.Next(3, 6));
                foreach (var c in Line(cursor, next))
                {
                    _floor.Add(c);
                    _floor.Add(c + Vector2Int.up);
                }
                cursor = next;
            }

            // paint floor + surrounding walls
            foreach (var c in _floor)
                floorMap.SetTile(new Vector3Int(c.x, c.y, 0), floorTile);
            foreach (var c in _floor)
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var n = new Vector2Int(c.x + dx, c.y + dy);
                        if (!_floor.Contains(n))
                            wallMap.SetTile(new Vector3Int(n.x, n.y, 0), wallTile);
                    }

            // move the player into room 0
            if (player != null)
                player.position = new Vector3(_rooms[0].center.x, _rooms[0].center.y, 0);

            // populate the other rooms
            for (int i = 1; i < _rooms.Count; i++)
            {
                var room = _rooms[i];
                for (int e = 0; e < enemiesPerRoom; e++)
                {
                    if (enemyPrefabs == null || enemyPrefabs.Length == 0) break;
                    var prefab = enemyPrefabs[rng.Next(enemyPrefabs.Length)];
                    if (prefab == null) continue;
                    var pos = new Vector3(rng.Next(room.xMin + 1, room.xMax - 1) + 0.5f, rng.Next(room.yMin + 1, room.yMax - 1) + 0.5f, 0);
                    var go = Instantiate(prefab, pos, Quaternion.identity);
                    go.tag = "Enemy";
                    var rb = go.GetComponent<Rigidbody2D>();
                    if (rb) { rb.bodyType = RigidbodyType2D.Dynamic; rb.gravityScale = 0; rb.constraints = RigidbodyConstraints2D.FreezeRotation; }
                    if (go.GetComponent<EnemyAI>() == null) go.AddComponent<EnemyAI>();
                    if (go.GetComponent<Health>() == null) go.AddComponent<Health>();
                }
                if (chestSprite != null && rng.NextDouble() < chestChance)
                {
                    var c = new GameObject("Chest");
                    c.transform.position = new Vector3(room.center.x, room.center.y, 0);
                    var sr = c.AddComponent<SpriteRenderer>(); sr.sprite = chestSprite; sr.sortingOrder = 5;
                    var col = c.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = Vector2.one;
                    var ci = c.AddComponent<ChestInteractable>(); ci.loot = chestLoot;
                }
            }
        }

        static IEnumerable<Vector2Int> Line(Vector2Int a, Vector2Int b)
        {
            var c = a;
            while (c.x != b.x) { yield return c; c.x += (int)Mathf.Sign(b.x - c.x); }
            while (c.y != b.y) { yield return c; c.y += (int)Mathf.Sign(b.y - c.y); }
            yield return c;
        }
    }
}
