using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSwarmPositionChecker : MonoBehaviour
{
    [SerializeField] private SmartParticleSwarm Swarm;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Tile"))
        {
            Swarm.Fleeing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Tile"))
        {
            Swarm.Fleeing = false;
        }
    }
}
