using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script will only place the player above the ground at the starting location (0, 0)
public class InfiniteLevelSetup : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private bool PlayerLoaded;
    [SerializeField] private float SpawnCheckHeight;
    [SerializeField] private InfiniteTileGeneration TilePrefabGeneration;

    private void Start()
    {
        SpawnCheckHeight = TilePrefabGeneration.GetMapHeight() + 1f;
    }

    private void Update()
    {
        if (!PlayerLoaded)
        {
            Vector3 playerSpawnLocation = Vector3.up * SpawnCheckHeight;
            RaycastHit playerSpawnHit;
            if (Physics.Raycast(playerSpawnLocation, Vector3.down, out playerSpawnHit, SpawnCheckHeight + 1f))
            {
                Player.transform.position = playerSpawnHit.point + Vector3.up * (SpawnCheckHeight / 2f);
                PlayerLoaded = true;
            }
        }

        // respawn the player if they fall off the map
        if (PlayerLoaded && Player.transform.position.y < -1f) {
            PlayerLoaded = false;
        }
    }
}
