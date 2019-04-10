using UnityEngine;

namespace Examples._1ArmMove.Scripts
{
    public class CollisionDetector : MonoBehaviour
    {
        public ArmMoveAgent agent; //


        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.CompareTag("wall"))
            {
                agent.IsWall();
            }
            else if (col.gameObject.CompareTag("target"))
            {
                agent.IsTarget();
                agent.ResetTarget(col.gameObject);
            }
        }
    }
}

