
using System;
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
        public BodyPartConstrain constrains;

        public BodyPart(Transform transform, BodyPartConstrain constrains)
        {

            this.transform =  transform;
            this.constrains = constrains;
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
              

                joint.highTwistLimit = new SoftJointLimit{limit = constrains.HighTwistLimit };
                joint.lowTwistLimit = new SoftJointLimit { limit = constrains.LowTwistLimit };
                joint.swing1Limit = new SoftJointLimit { limit = constrains.SwingLimit1 };
                joint.swing2Limit = new SoftJointLimit { limit = constrains.SwingLimit2 };
            }

            var scaleX = this.transform.localScale.x * constrains.ScaleX;
            var scaleY = this.transform.localScale.y * constrains.ScaleY;
            var scaleZ = this.transform.localScale.z * constrains.ScaleZ;

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
