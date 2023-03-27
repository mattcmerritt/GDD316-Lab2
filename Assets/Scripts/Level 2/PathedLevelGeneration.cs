using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code based on example from class (https://github.com/greggddqu/ProceduralGenExs/blob/main/Assets/Scripts/MyLevelGen.cs)
// Also based on tutorial provided in class example (https://gamedevacademy.org/complete-guide-to-procedural-level-generation-in-unity-part-1/)

public class PathedLevelGeneration : MonoBehaviour
{
    [SerializeField] private int MapWidthInTiles, MapDepthInTiles;
    [SerializeField] private GameObject TilePrefab;
    [SerializeField] private bool RandomSeed;
    [SerializeField] private PathGeneration PathGeneration;
    [SerializeField] private NoiseMapGeneration NoiseMapGeneration;

    private bool AggregatePrepared;

    // list of all tiles, used to reapply textures with paths
    private List<PathedTileGeneration> GeneratedTiles;

    private void Start()
    {
        GeneratedTiles = new List<PathedTileGeneration>();

        // generating the map
        GenerateMap();

        // fetching pixel depth and width from tile
        MeshFilter meshFilter = TilePrefab.GetComponent<MeshFilter>();
        int tileDepth = (int) Mathf.Sqrt(meshFilter.sharedMesh.vertices.Length);
        int tileWidth = tileDepth;

        // aggregating the heightMaps
        float[,] aggregatedHeightMap = new float[tileDepth * MapDepthInTiles, tileWidth * MapWidthInTiles];
        for (int i = 0; i < GeneratedTiles.Count; i++)
        {
            // calculate offsets
            int zOff = i % GeneratedTiles.Count;
            int xOff = i / GeneratedTiles.Count;

            // adding the tiles into the aggregate map, need to wait for map to load
            StartCoroutine(AddToAggregateMap(i, aggregatedHeightMap, zOff, xOff));
        }

        // generating a height map for the full map
        PathedTileGeneration prefabTileGeneration = TilePrefab.GetComponent<PathedTileGeneration>();
        float mapScale = prefabTileGeneration.GetMapScale();
        Wave[] waves = prefabTileGeneration.GetWaves();
        float[,] completeHeightMap = NoiseMapGeneration.GenerateNoiseMap(tileDepth * MapDepthInTiles, tileWidth * MapWidthInTiles, mapScale, 0f, 0f, waves);

        // fetching the height for mountains
        float mountainHeight = 0;
        TerrainType[] terrainTypes = prefabTileGeneration.GetTerrainTypes();
        foreach (TerrainType t in terrainTypes)
        {
            if (t.name == "highlands")
            {
                mountainHeight = t.height;
            }
        }

        // generating the path map
        PathGeneration.GenerateMaze(MapDepthInTiles, MapWidthInTiles, tileDepth, tileWidth, completeHeightMap, mountainHeight);
        Debug.Log(PathGeneration.WriteMaze());

        // reapplying the maze to the tile textures
        Vector3 tileSize = TilePrefab.GetComponent<MeshRenderer>().bounds.size;
        foreach (PathedTileGeneration tile in GeneratedTiles)
        {
            StartCoroutine(tile.AddPathToTexture(tileDepth, tileWidth, tileSize));
        }

        // printing maps
        StartCoroutine(PrintMaps(aggregatedHeightMap, completeHeightMap));
    }

    private void GenerateMap()
    {
        Vector3 tileSize = TilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        // Randomizing the tile seeds
        if (RandomSeed)
        {
            PathedTileGeneration tg = TilePrefab.GetComponent<PathedTileGeneration>();
            tg.RandomizeLevelSeeds();
        }

        for (int xTileIndex = 0; xTileIndex < MapWidthInTiles; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < MapDepthInTiles; zTileIndex++)
            {
                Vector3 tilePosition = new Vector3(transform.position.x + xTileIndex * tileWidth, transform.position.y, transform.position.z + zTileIndex * tileDepth);
                GameObject tile = Instantiate(TilePrefab, tilePosition, Quaternion.identity);
                GeneratedTiles.Add(tile.GetComponent<PathedTileGeneration>());
            }
        }
    }

    // Accessor methods to determine where to spawn the player and goal
    public int GetMapWidth()
    {
        return MapWidthInTiles;
    }

    public int GetMapDepth()
    {
        return MapDepthInTiles;
    }

    public int GetTileWidth()
    {
        return (int) TilePrefab.GetComponent<MeshRenderer>().bounds.size.x;
    }

    public int GetTileDepth()
    {
        return (int) TilePrefab.GetComponent<MeshRenderer>().bounds.size.z;
    }

    private void PrintHeightMap(float[,] map)
    {
        int depth = map.GetLength(0);
        int width = map.GetLength(1);
        string output = $"Dimensions: {depth} x {width}\n";

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                output += (int) (map[z, x] * 10f);
            }
            output += "\n";
        }

        Debug.Log(output);
    }

    public IEnumerator AddToAggregateMap(int i, float[,] aggregatedHeightMap, int zOff, int xOff)
    {
        yield return new WaitUntil(() => {
            return GeneratedTiles[i].GetHeightMap() != null && GeneratedTiles[i].GetHeightMap()[GeneratedTiles[i].GetHeightMap().GetLength(0) - 1, GeneratedTiles[i].GetHeightMap().GetLength(1) - 1] != 0f;
        });
        // copy map over to designated spot
        float[,] currentMap = GeneratedTiles[i].GetHeightMap();
        for (int z = 0; z < currentMap.GetLength(0); z++)
        {
            for (int x = 0; x < currentMap.GetLength(1); x++)
            {
                aggregatedHeightMap[zOff + z, xOff + x] = currentMap[z, x];
            }
        }

        AggregatePrepared = true;
    }

    public IEnumerator PrintMaps(float[,] aggregatedHeightMap, float[,] completeHeightMap)
    {
        yield return new WaitUntil(() => AggregatePrepared = true);
        // comparing
        PrintHeightMap(aggregatedHeightMap);
        PrintHeightMap(completeHeightMap);
    }
}
