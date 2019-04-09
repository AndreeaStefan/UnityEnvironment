using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class TargetContact1 : MonoBehaviour
{
    [HideInInspector]
    public ArmMoveAgent agent; 

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("target"))
        {
            agent.IsTarget();
        }
    }
}



