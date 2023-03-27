using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathedLevelSetup : MonoBehaviour
{
    [SerializeField] private PathedLevelGeneration LevelGeneration;
    [SerializeField] private GameObject Player, Goal;
    [SerializeField] private BoxCollider GoalCollider;

    [SerializeField] private bool PlayerLoaded, GoalLoaded;
    [SerializeField] private float SpawnCheckHeight;
    [SerializeField] private PathedTileGeneration TilePrefabGeneration;

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
                Player.transform.position = playerSpawnHit.point + Vector3.up;
                PlayerLoaded = true;
                // Debug.Log($"Placed player at { Player.transform.position}");
            }
        }

        if (!GoalLoaded)
        {
            Vector3 goalSpawnLocation = new Vector3((LevelGeneration.GetMapWidth() - 1) * LevelGeneration.GetTileWidth(), SpawnCheckHeight, (LevelGeneration.GetMapDepth() - 1) * LevelGeneration.GetTileDepth());
            RaycastHit goalSpawnHit;
            if (Physics.Raycast(goalSpawnLocation, Vector3.down, out goalSpawnHit, SpawnCheckHeight + 1f))
            {
                Goal.transform.position = goalSpawnHit.point + Vector3.up;
                GoalLoaded = true;
                GoalCollider.enabled = true;
                // Debug.Log($"Placed goal at {Goal.transform.position}");
            }
        }
    }
}
