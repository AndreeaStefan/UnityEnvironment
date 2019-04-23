
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmMove
{
    public class BodyPart
    {
        public string Name;

        public Transform transform;
        public Rigidbody rb;
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        public Vector3 scale;

        public CharacterJoint joint;
        public BodyPartConstraint constraints;

        public BodyPart(Transform transform, BodyPartConstraint constraints)
        {
            this.transform =  transform;
            this.constraints = constraints;
            initialPosition = this.transform.position;
            initialRotation = this.transform.rotation;

            rb = this.transform.GetComponent<Rigidbody>();
            joint = this.transform.GetComponent<CharacterJoint>();

           

            if (joint)
            {
                var connectedBody = joint.connectedBody;

                while (connectedBody.GetComponent<Collider>().bounds.Intersects(transform.GetComponent<Collider>().bounds))
                {
                    this.transform.position += new Vector3(0, 0, 0.01f * Math.Sign(this.transform.localPosition.z));
                    joint.connectedAnchor += new Vector3(0, 0.01f, 0) *  Math.Sign(this.transform.localPosition.z) * -1;
                }
              

                joint.highTwistLimit = new SoftJointLimit{limit = constraints.HighTwistLimit };
                joint.lowTwistLimit = new SoftJointLimit { limit = constraints.LowTwistLimit };
                joint.swing1Limit = new SoftJointLimit { limit = constraints.SwingLimit1 };
                joint.swing2Limit = new SoftJointLimit { limit = constraints.SwingLimit2 };
            }

            var scaleX = this.transform.localScale.x * constraints.ScaleX;
            var scaleY = this.transform.localScale.y * constraints.ScaleY;
            var scaleZ = this.transform.localScale.z * constraints.ScaleZ;

            scale = new Vector3(scaleX, scaleY, scaleZ);

            this.transform.localScale = scale;
        }

        public void ResetBodyPart()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            transform.localScale = scale;
        }

    }
}
