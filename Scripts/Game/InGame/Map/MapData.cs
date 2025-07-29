using UnityEngine;
using System;

public class MapData : Singleton<MapData>
{
    [Header("Grid 설정")]
    public int width;
    public int height;
    public float cellSize = 1f;
    public Vector3 origin = Vector3.zero;

    [Header("스폰 가능 여부 그리드")]
    [SerializeField] public bool[] spawnableGridFlat;

    [Header("전체 맵")]
    [SerializeField] public Vector2 mapSize;
    [SerializeField] public MapTile[] mapTiles;

    [SerializeField] private Building[] _buildings;

    public void InitializeGrid()
    {
        spawnableGridFlat = new bool[width * height];
        for (int i = 0; i < spawnableGridFlat.Length; i++)
            spawnableGridFlat[i] = true; // 기본값: 모두 스폰 가능
    }

    public void CreateBuildingEntity(EntityFactory inFactory)
    {
        for(int i = 0;i< _buildings.Length;i++)
        {
            _buildings[i].CreateBulildingEntity(inFactory);
        }
    }


    private int ToIndex(int x, int z) => x + z * width;

    public bool IsInBounds(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < height;
    }

    public Vector3 GridToWorld(int x, int z)
    {
        float worldX = (x - width / 2) * cellSize + cellSize * 0.5f;
        float worldZ = (z - height / 2) * cellSize + cellSize * 0.5f;
        return new Vector3(worldX, 0f, worldZ);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - (-width / 2f * cellSize)) / cellSize);
        int z = Mathf.FloorToInt((worldPos.z - (-height / 2f * cellSize)) / cellSize);
        return new Vector2Int(x, z);
    }

    public bool CanSpawnAtWorldPosition(Vector3 worldPos)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        if (!IsInBounds(grid.x, grid.y))
            return false;

        int idx = ToIndex(grid.x, grid.y);
        return spawnableGridFlat[idx];
    }

    public bool HasObsticlesAtWorldPosition(Vector3 worldPos)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        if (!IsInBounds(grid.x, grid.y))
            return false;

        int idx = ToIndex(grid.x, grid.y);
        return mapTiles[idx].hasObstacle;
    }

    public bool CanEnemySpawn(Vector3 worldPos)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        if (!IsInBounds(grid.x, grid.y))
            return false;

        int idx = ToIndex(grid.x, grid.y);
        if (mapTiles[idx].hasObstacle)
            return false;

        return !spawnableGridFlat[idx];
    }

    public void SetCanSpawnAtWorldPosition(Vector3 worldPos, bool canSpawn)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        if (!IsInBounds(grid.x, grid.y)) return;

        spawnableGridFlat[ToIndex(grid.x, grid.y)] = canSpawn;
    }
    public bool IsOverlapWithBuilding(Vector3 worldPos, float threshold = 0.1f)
    {
        if (_buildings == null || _buildings.Length == 0)
            return false;

        foreach (var building in _buildings)
        {
            if (building == null) continue;

            float radius = building.CalculateSize();
            Vector3 buildingPos = building.transform.position;

            // 빌딩 중심과의 거리 <= 빌딩 반경 + 임계값(여유)
            if (Vector3.Distance(worldPos, buildingPos) <= (radius + threshold))
                return true;
        }

        return false;
    }


#if UNITY_EDITOR

    public bool GizmoOn;
    private void OnValidate()
    {
        if (spawnableGridFlat == null || spawnableGridFlat.Length != width * height)
            InitializeGrid();
    }
    public void ValidateGridByNavMesh()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (!spawnableGridFlat[ToIndex(x, z)])
                    continue;

                Vector3 worldPos = GridToWorld(x, z);
                bool canSpawn = UnityEngine.AI.NavMesh.SamplePosition(worldPos, out _, 0.1f, UnityEngine.AI.NavMesh.AllAreas);

                spawnableGridFlat[ToIndex(x, z)] = canSpawn;
            }
        }

    }
    private void OnDrawGizmos()
    {
        if (!GizmoOn)
            return;

        if (spawnableGridFlat == null || spawnableGridFlat.Length != width * height)
            InitializeGrid();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 worldPos = GridToWorld(x, z);
                bool canSpawn = spawnableGridFlat[ToIndex(x, z)];

                Gizmos.color = canSpawn ? Color.cyan : Color.red;
                Gizmos.DrawCube(worldPos, Vector3.one * cellSize * 0.95f);
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(worldPos, Vector3.one * cellSize * 0.95f);
            }
        }
    }
#endif

    public void Initialize()
    {
        ConfigureTileBorders((int)mapSize.x, (int)mapSize.y);
    }

    public void ShowTileMark()
    {
        for (int i = 0; i < mapTiles.Length; i++)
        {
            if (spawnableGridFlat[i] == false)
                mapTiles[i].ShowTileMark();
        }
    }

    public void HideTileMark()
    {
        for (int i = 0; i < mapTiles.Length; i++)
        {
            mapTiles[i].HideTileMark();
        }
    }


    public void ConfigureTileBorders(int width, int height)
    {
        if (mapTiles == null || mapTiles.Length != (width * height))
            return;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int index = x + z * width;
                MapTile tile = mapTiles[index];
                if (tile == null) continue;

                bool isSpawnTile = spawnableGridFlat[index];

                Renderer rend = tile.renderer;
                if (rend == null)
                {
                    continue;
                }

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();

                int checkIdx = (x - 1) + z * width;
                bool drawLeft = x == 0
                    || mapTiles[checkIdx] == null
                    || spawnableGridFlat[checkIdx].Equals(isSpawnTile) == false
                    || mapTiles[checkIdx].hasObstacle;

                checkIdx = (x + 1) + z * width;
                bool drawRight = x == width - 1
                    || mapTiles[checkIdx] == null
                    || spawnableGridFlat[checkIdx].Equals(isSpawnTile) == false
                    || mapTiles[checkIdx].hasObstacle;

                checkIdx = x + (z - 1) * width;
                bool drawBottom = z == 0
                    || mapTiles[checkIdx] == null
                    || spawnableGridFlat[checkIdx].Equals(isSpawnTile) == false
                    || mapTiles[checkIdx].hasObstacle;

                checkIdx = x + (z + 1) * width;
                bool drawTop = z == height - 1
                    || mapTiles[checkIdx] == null
                    || spawnableGridFlat[checkIdx].Equals(isSpawnTile) == false
                    || mapTiles[checkIdx].hasObstacle;

                mpb.SetFloat("_DrawLeft", drawLeft ? 1f : 0f);
                mpb.SetFloat("_DrawRight", drawRight ? 1f : 0f);
                mpb.SetFloat("_DrawBottom", drawBottom ? 1f : 0f);
                mpb.SetFloat("_DrawTop", drawTop ? 1f : 0f);

                rend.SetPropertyBlock(mpb);
            }
        }
    }

}
