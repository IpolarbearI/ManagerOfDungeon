using UnityEngine;

namespace ManagerOfDungeon
{
    [ExecuteAlways]
    public sealed class DungeonBoard : MonoBehaviour
    {
        [System.Serializable]
        public struct TilePlacement
        {
            public Vector2Int coord;
            public DungeonTileDefinition tile;
        }

        [SerializeField] int width = 5;
        [SerializeField] int height = 5;
        [SerializeField] float cellSize = 1f;
        [SerializeField] float gap = 0.08f;
        [SerializeField] DungeonTileDefinition defaultTile;
        [SerializeField] TilePlacement[] specialTiles;
        [SerializeField] bool frameMainCamera = true;

        DungeonGrid _grid;
        Transform _tilesRoot;
        Sprite _cellSprite;
        DungeonTileDefinition _runtimeFloor;

        public DungeonGrid Grid => _grid;
        public int Width => width;
        public int Height => height;

        void OnEnable()
        {
#if UNITY_EDITOR
            SetSceneViewTo2D();
#endif
            Rebuild();
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= RebuildDelayed;
#endif
            ClearTiles();
        }

        void OnValidate()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            cellSize = Mathf.Max(0.1f, cellSize);
            gap = Mathf.Max(0f, gap);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= RebuildDelayed;
            UnityEditor.EditorApplication.delayCall += RebuildDelayed;
#endif
        }

        public void Rebuild()
        {
            _grid = new DungeonGrid(width, height, ResolveDefaultTile());
            ApplySpecialTiles();
            BuildViews();
            FrameCameraIfNeeded();
        }

        public bool SetSpecialTile(int x, int y, DungeonTileDefinition tile)
        {
            if (_grid == null)
                Rebuild();

            if (!_grid.SetTile(x, y, tile))
                return false;

            RefreshCellView(x, y);
            return true;
        }

        public bool TryWorldToCell(Vector3 world, out Vector2Int cell)
        {
            cell = default;
            if (_grid == null)
                return false;

            float stride = cellSize + gap;
            float originX = (width - 1) * stride * 0.5f;
            float originY = (height - 1) * stride * 0.5f;
            Vector3 local = transform.InverseTransformPoint(world);
            int x = Mathf.RoundToInt((local.x + originX) / stride);
            int y = Mathf.RoundToInt((local.y + originY) / stride);
            if (!_grid.InBounds(x, y))
                return false;

            cell = new Vector2Int(x, y);
            return true;
        }

        void RebuildDelayed()
        {
            if (this == null || !isActiveAndEnabled)
                return;

            Rebuild();
        }

        DungeonTileDefinition ResolveDefaultTile()
        {
            if (defaultTile != null)
                return defaultTile;

            if (_runtimeFloor == null)
            {
                _runtimeFloor = ScriptableObject.CreateInstance<DungeonTileDefinition>();
                _runtimeFloor.name = "RuntimeFloor";
            }

            return _runtimeFloor;
        }

        void ApplySpecialTiles()
        {
            if (specialTiles == null)
                return;

            for (int i = 0; i < specialTiles.Length; i++)
            {
                var placement = specialTiles[i];
                if (placement.tile == null)
                    continue;

                _grid.SetTile(placement.coord, placement.tile);
            }
        }

        void BuildViews()
        {
            EnsureSprite();
            _tilesRoot = EnsureTilesRoot();
            ClearTiles();

            float stride = cellSize + gap;
            float originX = (width - 1) * stride * 0.5f;
            float originY = (height - 1) * stride * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    CreateCellView(x, y, stride, originX, originY);
            }
        }

        void CreateCellView(int x, int y, float stride, float originX, float originY)
        {
            var cell = new GameObject($"Tile_{x}_{y}");
            cell.transform.SetParent(_tilesRoot, false);
            cell.transform.SetLocalPositionAndRotation(
                new Vector3(x * stride - originX, y * stride - originY, 0f),
                Quaternion.identity);
            cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);

            var renderer = cell.AddComponent<SpriteRenderer>();
            renderer.sprite = _cellSprite;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.color = ResolveTint(x, y);
            renderer.sortingOrder = 0;
        }

        void RefreshCellView(int x, int y)
        {
            if (_tilesRoot == null)
                return;

            var child = _tilesRoot.Find($"Tile_{x}_{y}");
            if (child == null)
                return;

            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.color = ResolveTint(x, y);
        }

        Color ResolveTint(int x, int y)
        {
            var tile = _grid != null ? _grid.GetTile(x, y) : null;
            return tile != null ? tile.Tint : Color.magenta;
        }

        Transform EnsureTilesRoot()
        {
            var existing = transform.Find("Tiles");
            if (existing != null)
                return existing;

            var root = new GameObject("Tiles");
            root.transform.SetParent(transform, false);
            return root.transform;
        }

        void ClearTiles()
        {
            if (_tilesRoot == null)
                _tilesRoot = transform.Find("Tiles");

            if (_tilesRoot == null)
                return;

            for (int i = _tilesRoot.childCount - 1; i >= 0; i--)
            {
                var child = _tilesRoot.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        void EnsureSprite()
        {
            if (_cellSprite != null)
                return;

            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels32(pixels);
            tex.Apply();

            _cellSprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            _cellSprite.name = "DungeonCell";
        }

        void FrameCameraIfNeeded()
        {
            if (!frameMainCamera)
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            float stride = cellSize + gap;
            float padding = 0.75f;
            float halfWidth = width * stride * 0.5f + padding;
            float halfHeight = height * stride * 0.5f + padding;
            float aspect = Mathf.Max(0.01f, cam.aspect);

            cam.orthographic = true;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.orthographicSize = Mathf.Max(halfHeight, halfWidth / aspect);
            cam.transparencySortMode = TransparencySortMode.CustomAxis;
            cam.transparencySortAxis = Vector3.forward;
            cam.transform.SetPositionAndRotation(
                new Vector3(transform.position.x, transform.position.y, -10f),
                Quaternion.identity);
        }

#if UNITY_EDITOR
        static void SetSceneViewTo2D()
        {
            var views = UnityEditor.SceneView.sceneViews;
            for (int i = 0; i < views.Count; i++)
            {
                var view = views[i] as UnityEditor.SceneView;
                if (view == null)
                    continue;

                view.in2DMode = true;
                view.orthographic = true;
            }
        }
#endif

        void OnDrawGizmosSelected()
        {
            if (_grid == null)
                return;

            Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
            float stride = cellSize + gap;
            float originX = (width - 1) * stride * 0.5f;
            float originY = (height - 1) * stride * 0.5f;
            var size = new Vector3(cellSize, cellSize, 0.02f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var local = new Vector3(x * stride - originX, y * stride - originY, 0f);
                    Gizmos.DrawWireCube(transform.TransformPoint(local), size);
                }
            }
        }
    }
}
