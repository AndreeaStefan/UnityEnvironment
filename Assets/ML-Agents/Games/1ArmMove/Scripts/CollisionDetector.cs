using UnityEngine;

namespace ArmMove
{
    public class CollisionDetector : MonoBehaviour
    {
            public ArmMoveAgent agent; 

            private void OnTriggerEnter(Collider other)
            {
                if (other.gameObject.CompareTag("target"))
                {
                    agent.IsTarget();
                    agent.ResetTarget(other.gameObject);
                }
                else if (other.gameObject.CompareTag("wall"))
                {
                    agent.IsWall();
                }
            }
    }
}