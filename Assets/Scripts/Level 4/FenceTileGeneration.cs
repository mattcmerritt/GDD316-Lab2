using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code based on example from class (https://github.com/greggddqu/ProceduralGenExs/blob/main/Assets/Scripts/MyTileGeneration.cs)
// Also based on tutorial provided in class example (https://gamedevacademy.org/complete-guide-to-procedural-level-generation-in-unity-part-1/)

public class FenceTileGeneration : MonoBehaviour
{
    [SerializeField] private NoiseMapGeneration NoiseMapGeneration;
    [SerializeField] private MeshRenderer MeshRenderer;
    [SerializeField] private MeshFilter MeshFilter;
    [SerializeField] private MeshCollider MeshCollider;
    [SerializeField] private float MapScale;
    [SerializeField] private TerrainType[] TerrainTypes;
    [SerializeField] private float HeightMultiplier;
    [SerializeField] private AnimationCurve HeightCurve;
    [SerializeField] private Wave[] Waves;
    
    // fence info
    public bool FencedIn;
    [SerializeField] private GameObject FencePostPrefab;

    private void Start()
    {
        GenerateTile();
    }

    private void GenerateTile()
    {
        Vector3[] meshVertices = this.MeshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        float offsetX = -transform.position.x;
        float offsetZ = -transform.position.z;

        float[,] heightMap = NoiseMapGeneration.GenerateNoiseMap(tileDepth, tileWidth, MapScale, offsetX, offsetZ, Waves);

        Texture2D tileTexture = BuildTexture(heightMap);
        MeshRenderer.material.mainTexture = tileTexture;

        UpdateMeshVertices(heightMap);

        // adding fence posts
        if (FencedIn)
        {
            // fence post vertex locations in clockwise order
            Vector2Int[] edgeLocations =
            {
                new Vector2Int (0, 0),
                new Vector2Int (0, 2),
                new Vector2Int (0, 4),
                new Vector2Int (0, 6),
                new Vector2Int (0, 8),
                new Vector2Int (0, 10),
                new Vector2Int (2, 10),
                new Vector2Int (4, 10),
                new Vector2Int (6, 10),
                new Vector2Int (8, 10),
                new Vector2Int (10, 10),
                new Vector2Int (10, 8),
                new Vector2Int (10, 6),
                new Vector2Int (10, 4),
                new Vector2Int (10, 2),
                new Vector2Int (10, 0),
                new Vector2Int (8, 0),
                new Vector2Int (6, 0),
                new Vector2Int (4, 0),
                new Vector2Int (2, 0),
            };

            // get the dimensions of the tile in world space
            Vector3 tileSize = MeshRenderer.bounds.size;
            int tileWorldWidth = (int) tileSize.x;
            int tileWorldDepth = (int) tileSize.z;

            // update vertices list
            meshVertices = MeshFilter.mesh.vertices;

            foreach (Vector2Int vertex in edgeLocations)
            {
                Vector3 FencePostLocation = (transform.position) + (meshVertices[vertex.x + vertex.y * tileDepth]);
                // Debug.Log($"Point: {vertex}, Location: {FencePostLocation}");
                Instantiate(FencePostPrefab, FencePostLocation, Quaternion.identity);
            }
        }
    }

    private Texture2D BuildTexture(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];
                TerrainType terrainType = ChooseTerrainType(height);
                colorMap[colorIndex] = terrainType.color;
            }
        }

        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    private TerrainType ChooseTerrainType(float height)
    {
        foreach (TerrainType terrainType in TerrainTypes)
        {
            if (height < terrainType.height)
            {
                return terrainType;
            }
        }
        return TerrainTypes[TerrainTypes.Length - 1];
    }

    private void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Vector3[] meshVertices = MeshFilter.mesh.vertices;

        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                float height = heightMap[zIndex, xIndex];
                Vector3 vertex = meshVertices[vertexIndex];
                meshVertices[vertexIndex] = new Vector3(vertex.x, HeightCurve.Evaluate(height) * HeightMultiplier, vertex.z);
                vertexIndex++;
            }
        }

        MeshFilter.mesh.vertices = meshVertices;
        MeshFilter.mesh.RecalculateBounds();
        MeshFilter.mesh.RecalculateNormals();
        MeshCollider.sharedMesh = MeshFilter.mesh;
    }

    public void RandomizeLevelSeeds()
    {
        foreach (Wave wave in Waves)
        {
            wave.seed = Random.Range(0, 10000);
        }
    }

    public float GetMapHeight()
    {
        return HeightMultiplier;
    }
}
