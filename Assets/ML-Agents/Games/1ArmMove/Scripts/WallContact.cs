using UnityEngine;

namespace ArmMove
{
    public class WallContact : MonoBehaviour
    {
        [HideInInspector] public ArmMoveAgent agent; //

        void OnCollisionEnter(Collision col)
        {
            // Touched goal.
            if (col.gameObject.CompareTag("wall"))
            {
                agent.IsWall();
            }
        }
    }
}

