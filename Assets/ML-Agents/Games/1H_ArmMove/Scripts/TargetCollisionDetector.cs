using UnityEngine;

namespace Games._1H_ArmMove.Scripts
{
    public class TargetCollisionDetector : MonoBehaviour
    {
        public ArmMoveAgent agent; 

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("hand"))
            {
                agent.IsTarget();
                agent.ResetTarget(gameObject);
            }
        }


    }
}
