using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLevelSetup : MonoBehaviour
{
    [SerializeField] private LevelGeneration LevelGeneration;
    [SerializeField] private GameObject Player, Goal, Enemy;
    [SerializeField] private BoxCollider GoalCollider;
    [SerializeField] private SphereCollider EnemyCollider;

    [SerializeField] private bool PlayerLoaded, GoalLoaded, EnemyLoaded;
    [SerializeField] private float SpawnCheckHeight;
    [SerializeField] private TileGeneration TilePrefabGeneration;

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
                // for some reason, the player is extremely like to fall through this level
                // thus, the default spawn is a bit higher up
                Player.transform.position = playerSpawnHit.point + Vector3.up * (SpawnCheckHeight / 2f); 
                PlayerLoaded = true;
            }
        }

        // respawn the player if they fall off the map
        if (PlayerLoaded && Player.transform.position.y < -1f) {
            PlayerLoaded = false;
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
            }
        }

        // as the game runs continue to raycast to check for the goal to prevent it from getting embedded in the ground
        if (GoalLoaded) 
        {
            Vector3 goalSpawnLocation = new Vector3((LevelGeneration.GetMapWidth() - 1) * LevelGeneration.GetTileWidth(), SpawnCheckHeight, (LevelGeneration.GetMapDepth() - 1) * LevelGeneration.GetTileDepth());
            RaycastHit goalSpawnHit;
            if (Physics.Raycast(goalSpawnLocation, Vector3.down, out goalSpawnHit, SpawnCheckHeight + 1f))
            {
                GoalLoaded = goalSpawnHit.collider.gameObject.name == Goal.name;
            }
        }

        if (!EnemyLoaded)
        {
            Vector3 enemySpawnLocation = new Vector3(LevelGeneration.GetMapWidth() * LevelGeneration.GetTileWidth() / 2f, SpawnCheckHeight, LevelGeneration.GetMapDepth() * LevelGeneration.GetTileDepth() / 2f);
            RaycastHit enemySpawnHit;
            if (Physics.Raycast(enemySpawnLocation, Vector3.down, out enemySpawnHit, SpawnCheckHeight + 1f))
            {
                Enemy.transform.position = enemySpawnHit.point + Vector3.up * 3f;
                EnemyLoaded = true;
                EnemyCollider.enabled = true;
            }
        }
    }
}
