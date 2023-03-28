using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code based on example from class (https://github.com/greggddqu/ProceduralGenExs/blob/main/Assets/Scripts/MyLevelGen.cs)
// Also based on tutorial provided in class example (https://gamedevacademy.org/complete-guide-to-procedural-level-generation-in-unity-part-1/)

public class InfiniteLevelGeneration : MonoBehaviour
{
    [SerializeField, Range(1, 10)] private int RenderDistance; // distance from the player's current tile to the edge
    [SerializeField] private GameObject Player; // reference to the player, necessary to get transform
    [SerializeField] private GameObject TilePrefab; // prefab for an infinite tile that can generate itself if placed in correct spot

    [SerializeField] private InfiniteTileGeneration[,] ActiveTiles; // grid of all tiles currently being rendered, center is the current one
    [SerializeField] private Vector3Int CurrentOffset; // vector representing the current center in terms of x and z

    private float TileWorldDepth, TileWorldWidth; // variables to represent the size of tiles in world space

    [SerializeField] private bool RandomSeed; // boolean used to determine if the map should have a randomized seed

    private void Start()
    {
        // initializing data structures
        ActiveTiles = new InfiniteTileGeneration[RenderDistance * 2 + 1, RenderDistance * 2 + 1];

        // randomizing the tile seeds
        if (RandomSeed)
        {
            InfiniteTileGeneration tg = TilePrefab.GetComponent<InfiniteTileGeneration>();
            tg.RandomizeLevelSeeds();
        }

        // get the dimensions of the tile in world space
        Vector3 tileSize = TilePrefab.GetComponent<MeshRenderer>().bounds.size;
        TileWorldDepth = (int)tileSize.z;
        TileWorldWidth = (int)tileSize.x;

        // generate all the remaining tiles in the starting render distance
        for (int zDist = -RenderDistance; zDist <= RenderDistance; zDist++)
        {
            for (int xDist = -RenderDistance; xDist <= RenderDistance; xDist++)
            {
                ActiveTiles[zDist + RenderDistance, xDist + RenderDistance] = CreateTile(zDist, xDist);
            }
        }
    }

    private void Update()
    {
        InfiniteTileGeneration currentTile = ActiveTiles[RenderDistance, RenderDistance];
        // check that the player is in the same tile as last frame
        Vector3 tileMin = currentTile.transform.position - new Vector3(TileWorldWidth / 2, 0f, TileWorldDepth / 2);
        Vector3 tileMax = currentTile.transform.position + new Vector3(TileWorldWidth / 2, 0f, TileWorldDepth / 2);
        Vector3 playerPosition = new Vector3(Player.transform.position.x, 0f, Player.transform.position.z);

        if (playerPosition.z > tileMax.z)
        {
            CurrentOffset += new Vector3Int(0, 0, 1);
            // need to shift the entire array left one unit
            ShiftArray(Vector3.left);
        }
        else if (playerPosition.z < tileMin.z)
        {
            CurrentOffset += new Vector3Int(0, 0, -1);
            // need to shift the entire array right one unit
            ShiftArray(Vector3.right);
        }
        else if (playerPosition.x > tileMax.x)
        {
            CurrentOffset += new Vector3Int(1, 0, 0);
            // need to shift the entire array down one unit
            ShiftArray(Vector3.down); 
        }
        else if (playerPosition.x < tileMin.x)
        {
            CurrentOffset += new Vector3Int(-1, 0, 0);
            // need to shift the entire array up one unit
            ShiftArray(Vector3.up);
        }
    }

    private InfiniteTileGeneration CreateTile(int z, int x)
    {
        // Debug.Log($"Creating tile at ({z}, {x})");
        Vector3 tilePosition = new Vector3(x * TileWorldWidth, 0f, z * TileWorldDepth);
        GameObject tile = Instantiate(TilePrefab, tilePosition, Quaternion.identity);
        return tile.GetComponent<InfiniteTileGeneration>();
    }

    private void ShiftArray(Vector3 direction)
    {
        if (direction == Vector3.left)
        {
            // need to delete the leftmost tiles
            for (int i = 0; i < ActiveTiles.GetLength(1); i++)
            {
                Destroy(ActiveTiles[0, i].gameObject);
            }

            // moving over the columns, starting from the left
            for (int z = 1; z < ActiveTiles.GetLength(0); z++)
            {
                for (int x = 0; x < ActiveTiles.GetLength(1); x++)
                {
                    ActiveTiles[z - 1, x] = ActiveTiles[z, x];
                }
            }

            // also need to generate new tiles to put on the left side
            for (int i = 0; i < ActiveTiles.GetLength(1); i++)
            {
                ActiveTiles[ActiveTiles.GetLength(0) - 1, i] = CreateTile(CurrentOffset.z + RenderDistance, CurrentOffset.x - RenderDistance + i);
            }
        }

        if (direction == Vector3.right)
        {
            // need to delete the rightmost tiles
            for (int i = 0; i < ActiveTiles.GetLength(1); i++)
            {
                Destroy(ActiveTiles[ActiveTiles.GetLength(0) - 1, i].gameObject);
            }

            // moving over the columns, starting from the right
            for (int z = ActiveTiles.GetLength(0) - 1; z > 0; z--)
            {
                for (int x = 0; x < ActiveTiles.GetLength(1); x++)
                {
                    ActiveTiles[z, x] = ActiveTiles[z - 1, x];
                }
            }

            // also need to generate new tiles to put on the right side
            for (int i = 0; i < ActiveTiles.GetLength(1); i++)
            {
                ActiveTiles[0, i] = CreateTile(CurrentOffset.z - RenderDistance, CurrentOffset.x - RenderDistance + i);
            }
        }

        if (direction == Vector3.down)
        {
            // need to delete the bottommost tiles
            for (int i = 0; i < ActiveTiles.GetLength(0); i++)
            {
                Destroy(ActiveTiles[i, 0].gameObject);
            }

            // moving over the rows, starting from the top
            for (int x = 1; x < ActiveTiles.GetLength(1); x++)
            {
                for (int z = 0; z < ActiveTiles.GetLength(0); z++)
                {
                    ActiveTiles[z, x - 1] = ActiveTiles[z, x];
                }
            }

            // also need to generate new tiles to put on the bottom side
            for (int i = 0; i < ActiveTiles.GetLength(0); i++)
            {
                ActiveTiles[i, ActiveTiles.GetLength(0) - 1] = CreateTile(CurrentOffset.z - RenderDistance + i, CurrentOffset.x + RenderDistance);
            }
        }

        if (direction == Vector3.up)
        {
            // need to delete the topmost tiles
            for (int i = 0; i < ActiveTiles.GetLength(0); i++)
            {
                Destroy(ActiveTiles[i, ActiveTiles.GetLength(1) - 1].gameObject);
            }

            // moving over the rows, starting from the bottom
            for (int x = ActiveTiles.GetLength(1) - 1; x > 0; x--)
            {
                for (int z = 0; z < ActiveTiles.GetLength(0); z++)
                {
                    ActiveTiles[z, x] = ActiveTiles[z, x - 1];
                }
            }

            // also need to generate new tiles to put on the top side
            for (int i = 0; i < ActiveTiles.GetLength(0); i++)
            {
                ActiveTiles[i, 0] = CreateTile(CurrentOffset.z - RenderDistance + i, CurrentOffset.x - RenderDistance);
            }
        }
    }
}
