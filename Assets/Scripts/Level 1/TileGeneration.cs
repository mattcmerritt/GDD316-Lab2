using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code based on example from class (https://github.com/greggddqu/ProceduralGenExs/blob/main/Assets/Scripts/MyTileGeneration.cs)
// Also based on tutorial provided in class example (https://gamedevacademy.org/complete-guide-to-procedural-level-generation-in-unity-part-1/)

[System.Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public class TileGeneration : MonoBehaviour
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
}
