using System;
using UnityEngine;
using System.Collections;

namespace MyWalker
{
    public class BodyPart
    {

        [Header("Body Part Info")] [Space(10)] public ConfigurableJoint joint;
        public Rigidbody rigidBody;
        [HideInInspector] public Vector3 startingPos;
        [HideInInspector] public Quaternion startingRot;

        public MyGroundContact groundContact;
        public MyTargetContact targetContact;

        [HideInInspector] public float currentStrength;
        public float currentXNormalizedRot;
        public float currentYNormalizedRot;
        public float currentZNormalizedRot;

        /// <summary>
        /// Reset body part to initial configuration.
        /// </summary>
        public void Reset(BodyPart bp)
        {
            bp.rigidBody.transform.position = bp.startingPos;
            bp.rigidBody.transform.rotation = bp.startingRot;
            bp.rigidBody.velocity = Vector3.zero;
            bp.rigidBody.angularVelocity = Vector3.zero;
            if (bp.groundContact)
            {
                bp.groundContact.touchingGround = false;
            }

            if (bp.targetContact)
            {
                bp.targetContact.touchingTarget = false;
            }
        }

        /// <summary>
        /// Apply torque according to defined goal `x, y, z` angle and force `strength`.
        /// </summary>
        public void SetJointTargetRotation(float x, float y, float z)
        {
            x = (x + 1f) * 0.5f;
            y = (y + 1f) * 0.5f;
            z = (z + 1f) * 0.5f;
            var yLimit = Math.Abs(joint.angularYLimit.limit);
            var zLimit = Math.Abs(joint.angularZLimit.limit);
            var xRot = Mathf.Lerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, x);
            var yRot = Mathf.Lerp(-yLimit, yLimit, y);
            var zRot = Mathf.Lerp(-zLimit, zLimit, z);

            currentXNormalizedRot = Mathf.InverseLerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, xRot);
            currentYNormalizedRot = Mathf.InverseLerp(joint.angularYLimit.limit, joint.angularYLimit.limit, yRot);
            currentZNormalizedRot = Mathf.InverseLerp(joint.angularZLimit.limit, joint.angularZLimit.limit, zRot);

            joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);

        }

        public void SetJointStrength(float strength)
        {
            var rawVal = (strength + 1f) * 0.5f * 25000;
            var jd = new JointDrive
            {
                positionSpring = 10000,
                positionDamper = 50,
                maximumForce = rawVal
            };
            joint.slerpDrive = jd;
            currentStrength = jd.maximumForce;
        }
    }
}
   
