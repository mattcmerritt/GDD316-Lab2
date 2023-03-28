using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSwarm : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField, Range(0f, 20f)] private float MoveSpeed;

    private void Update()
    {
        // move in direction of the player
        Vector3 directionToPlayer = Vector3.Normalize(Player.transform.position - transform.position);
        float distanceToPlayer = Mathf.Abs(Vector3.Distance(Player.transform.position, transform.position));

        RaycastHit playerHit;
        if (Physics.Raycast(transform.position, directionToPlayer, out playerHit, distanceToPlayer + 1f))
        {
            // if blocked, fly up
            if (playerHit.collider.gameObject.name.Contains("Tile"))
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
}
