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

    // list of all tiles, used to reapply textures with paths
    [SerializeField] private List<PathedTileGeneration> GeneratedTiles;

    private void Start()
    {
        GeneratedTiles = new List<PathedTileGeneration>();

        // generating the map
        GenerateMap();

        // fetching pixel depth and width from tile
        MeshFilter meshFilter = TilePrefab.GetComponent<MeshFilter>();
        int tileDepth = (int) Mathf.Sqrt(meshFilter.sharedMesh.vertices.Length);
        int tileWidth = tileDepth;

        // generating the path map
        PathGeneration.GenerateMaze(MapDepthInTiles, MapWidthInTiles, tileDepth, tileWidth, GeneratedTiles);
        Debug.Log(PathGeneration.WriteMaze());

        // reapplying the maze to the tile textures
        Vector3 tileSize = TilePrefab.GetComponent<MeshRenderer>().bounds.size;
        foreach (PathedTileGeneration tile in GeneratedTiles)
        {
            StartCoroutine(tile.AddPathToTexture(tileDepth, tileWidth, tileSize));
        }
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
                tile.name += $" {zTileIndex} {xTileIndex}";
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
}
