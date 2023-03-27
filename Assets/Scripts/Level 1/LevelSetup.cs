using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSetup : MonoBehaviour
{
    [SerializeField] private LevelGeneration LevelGeneration;
    [SerializeField] private GameObject Player, Goal;

    private bool PlayerLoaded, GoalLoaded;
    [SerializeField] private float SpawnCheckHeight = 10f;

    private void Update()
    {
        if (!PlayerLoaded)
        {
            Vector3 playerSpawnLocation = Vector3.up * SpawnCheckHeight;
            RaycastHit playerSpawnHit;
            if (Physics.Raycast(playerSpawnLocation, Vector3.down, out playerSpawnHit, SpawnCheckHeight))
            {
                Player.transform.position = playerSpawnHit.point + Vector3.up;
                PlayerLoaded = true;
            }
        }

        if (!GoalLoaded)
        {
            Vector3 goalSpawnLocation = new Vector3((LevelGeneration.GetMapWidth() - 1) * LevelGeneration.GetTileWidth(), SpawnCheckHeight, (LevelGeneration.GetMapDepth() - 1) * LevelGeneration.GetTileDepth());
            RaycastHit goalSpawnHit;
            if (Physics.Raycast(goalSpawnLocation, Vector3.down, out goalSpawnHit, SpawnCheckHeight))
            {
                Goal.transform.position = goalSpawnHit.point + Vector3.up;
                GoalLoaded = true;
            }
        }
    }
}
