using UnityEngine;

namespace Games._1H_ArmMove.Scripts
{
    public class TargetCollisionDetector : MonoBehaviour
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
