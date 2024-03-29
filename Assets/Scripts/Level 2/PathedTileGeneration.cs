using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code based on example from class (https://github.com/greggddqu/ProceduralGenExs/blob/main/Assets/Scripts/MyTileGeneration.cs)
// Also based on tutorial provided in class example (https://gamedevacademy.org/complete-guide-to-procedural-level-generation-in-unity-part-1/)

public class PathedTileGeneration : MonoBehaviour
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
    [SerializeField] private PathGeneration PathGeneration;
    [SerializeField] private Color PathColor;
    private Texture2D TileTexture;
    private float[,] HeightMap;

    private void Start()
    {
        // path generator is shared and needs to be found in the scene
        PathGeneration = FindObjectOfType<PathGeneration>();
        GenerateTile();
    }

    private void GenerateTile()
    {
        Vector3[] meshVertices = this.MeshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        float offsetX = -transform.position.x;
        float offsetZ = -transform.position.z;

        HeightMap = NoiseMapGeneration.GenerateNoiseMap(tileDepth, tileWidth, MapScale, offsetX, offsetZ, Waves);

        BuildTexture(HeightMap);
        MeshRenderer.material.mainTexture = TileTexture;

        UpdateMeshVertices(HeightMap);
    }

    private void BuildTexture(float[,] heightMap)
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

        // Debug.LogWarning($"Inital Build:\n\tFirst: {colorMap[0]}, Height = {heightMap[0, 0]}\n\tLast: {colorMap[colorMap.Length - 1]}, Height = {heightMap[10, 10]}");

        TileTexture = new Texture2D(tileWidth, tileDepth);
        TileTexture.wrapMode = TextureWrapMode.Clamp;
        TileTexture.SetPixels(colorMap);
        TileTexture.Apply();
    }

    public IEnumerator AddPathToTexture(int tilePixelDepth, int tilePixelWidth, Vector3 tileWorldSize)
    {
        // wait until the colored texture is built
        yield return new WaitUntil(() => TileTexture != null);

        Color[] colorMap = TileTexture.GetPixels();

        int offsetX = (int)(transform.position.x / tileWorldSize.x * tilePixelWidth);
        int offsetZ = (int)(transform.position.z / tileWorldSize.z * tilePixelDepth);

        for (int zIndex = 0; zIndex < tilePixelDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tilePixelWidth; xIndex++)
            {
                int colorIndex = zIndex * tilePixelWidth + xIndex;
                
                if (PathGeneration.GetTile(new Point(offsetZ + zIndex, offsetX + xIndex)) == MazeTile.Path)
                {
                    colorMap[colorMap.Length - colorIndex - 1] = PathColor;
                }
            }
        }

        // Debug.LogWarning($"Path Build:\n\tFirst: {colorMap[0]}, Height = {HeightMap[0, 0]}\n\tLast: {colorMap[colorMap.Length - 1]}, Height = {HeightMap[10, 10]}");

        Texture2D pathedTileTexture = new Texture2D(tilePixelWidth, tilePixelDepth);
        pathedTileTexture.wrapMode = TextureWrapMode.Clamp;
        pathedTileTexture.SetPixels(colorMap);
        pathedTileTexture.Apply();

        MeshRenderer.material.mainTexture = pathedTileTexture;
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

    public Texture2D GetTexture()
    {
        return TileTexture;
    }

    public float[,] GetHeightMap() 
    {
        return HeightMap;
    }
}
