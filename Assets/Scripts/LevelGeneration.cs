using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code based on example from class (https://github.com/greggddqu/ProceduralGenExs/blob/main/Assets/Scripts/MyLevelGen.cs)
// Also based on tutorial provided in class example (https://gamedevacademy.org/complete-guide-to-procedural-level-generation-in-unity-part-1/)

public class LevelGeneration : MonoBehaviour
{
    [SerializeField] private int MapWidthInTiles, MapDepthInTiles;
    [SerializeField] private GameObject TilePrefab;

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        Vector3 tileSize = TilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        for (int xTileIndex = 0; xTileIndex < MapWidthInTiles; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < MapDepthInTiles; zTileIndex++)
            {
                Vector3 tilePosition = new Vector3(transform.position.x + xTileIndex * tileWidth, transform.position.y, transform.position.z + zTileIndex * tileDepth);
                GameObject tile = Instantiate(TilePrefab, tilePosition, Quaternion.identity);
            }
        }
    }
}
