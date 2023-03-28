using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartParticleSwarm : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField, Range(0f, 20f)] private float MoveSpeed;

    public bool Fleeing; // used to take the swarm out of the safe area

    private void Update()
    {
        
        Vector3 directionToPlayer = Vector3.Normalize(Player.transform.position - transform.position);
        float distanceToPlayer = Mathf.Abs(Vector3.Distance(Player.transform.position, transform.position));

        // move in direction of the player
        if (!Fleeing)
        {
            RaycastHit playerHit;
            if (Physics.Raycast(transform.position, directionToPlayer, out playerHit, distanceToPlayer + 1f))
            {
                // if blocked, fly up, unless the hit was on a trigger (meaning it hit the safe area)
                if (playerHit.collider.gameObject.name.Contains("Tile") && !playerHit.collider.isTrigger)
                {
                    transform.position += Vector3.Normalize(directionToPlayer + Vector3.up) * Time.deltaTime * MoveSpeed;
                }
                // if nothing blocking, fly directly to the player
                else
                {
                    transform.position += directionToPlayer * Time.deltaTime * MoveSpeed;
                }
            }
        }
        // run away from player to get out of safe area
        else
        {
            transform.position += -directionToPlayer * Time.deltaTime * MoveSpeed;
        }
    }
}
