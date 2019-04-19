
using UnityEngine;

namespace ArmMove
{
    public class Target : MonoBehaviour
    {
        public ArmMoveAgent agent;
        public int id;
        public Vector3 initialPosition;
        public Quaternion initialRotation;

        void OnAwake()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.CompareTag("arm"))
            {
               agent.IsTarget(id);
            }
        }

        public void ResetTransform()
        {
            //  transform.localPosition = initialPosition;
            transform.localRotation = initialRotation;
        }
    }
}
