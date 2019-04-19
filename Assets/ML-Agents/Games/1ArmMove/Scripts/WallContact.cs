using UnityEngine;

namespace Games._1ArmMove.Scripts
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

