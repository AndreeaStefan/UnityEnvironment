using UnityEngine;

namespace Games._1H_ArmMove.Scripts
{
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
}
