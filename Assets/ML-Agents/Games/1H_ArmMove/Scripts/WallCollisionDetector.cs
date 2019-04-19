using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollisionDetector : MonoBehaviour
{
    public ArmMoveAgent agent; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("arm") || other.gameObject.CompareTag("Player"))
        {
            agent.IsWall();
        }
    }
}
