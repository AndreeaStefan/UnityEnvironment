
using UnityEngine;

namespace ArmMove
{
    public class BodyPart
    {
        public string Name;

        public GameObject gameObject;
        public Rigidbody rb;
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        public Vector3 scale;

        public CharacterJoint joint;
        public BodyPartConstrain constrains;

        public BodyPart(GameObject gameObject, BodyPartConstrain constrains)
        {

            this.gameObject = gameObject;
            this.constrains = constrains;
            initialPosition = gameObject.transform.position;
            initialRotation = gameObject.transform.rotation;

            rb = gameObject.GetComponent<Rigidbody>();
            joint = gameObject.GetComponent<CharacterJoint>();
            if (joint)
            {
                joint.highTwistLimit = new SoftJointLimit{limit = constrains.HighTwistLimit };
                joint.lowTwistLimit = new SoftJointLimit { limit = constrains.LowTwistLimit };
                joint.swing1Limit = new SoftJointLimit { limit = constrains.SwingLimit1 };
                joint.swing2Limit = new SoftJointLimit { limit = constrains.SwingLimit2 };
            }

            var scaleX = gameObject.transform.localScale.x * constrains.ScaleX;
            var scaleY = gameObject.transform.localScale.y * constrains.ScaleY;
            var scaleZ = gameObject.transform.localScale.z * constrains.ScaleZ;

            scale = new Vector3(scaleX, scaleY, scaleZ);

            gameObject.transform.localScale = scale;
        }

        public void ResetBodyPart()
        {
            gameObject.transform.position = initialPosition;
            gameObject.transform.rotation = initialRotation;
            gameObject.transform.localScale = scale;
        }

    }
}
