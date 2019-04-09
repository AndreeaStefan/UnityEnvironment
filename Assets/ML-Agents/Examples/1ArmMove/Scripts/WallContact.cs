using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class WallContact : MonoBehaviour
{
    [HideInInspector]
    public ArmMoveAgent agent; //

    void OnCollisionEnter(Collision col)
    {
        // Touched goal.
        if (col.gameObject.CompareTag("wall"))
        {
            agent.IsWall();
        }
    }
}

