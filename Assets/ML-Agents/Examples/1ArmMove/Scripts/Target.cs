
using UnityEngine;

public class Target : MonoBehaviour
{
    public ArmMoveAgent agent;
    public bool isTriggered = false;
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
            if(!isTriggered) agent.IsTarget();
            isTriggered = true;
        }
    }

    public void ResetTransform()
    {
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;
    }
}
