using System.CodeDom;
using MLAgents;
using System.Collections.Generic;
using UnityEngine;

namespace MyWalker
{
    public class MyWalkerAgent : Agent
    {
        public Vector3 dirToTarget;
        public Transform target;

        public Transform hips;
        public Transform leftUpperLeg;
        public Transform rightUpperLeg;
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform leftFoot;
        public Transform rightFoot;

        private List<BodyPart> bodyParts = new List<BodyPart>();
        private Dictionary<Transform, BodyPart> bodyPartsDict = new Dictionary<Transform, BodyPart>();

        bool isNewDecisionStep;
        int currentDecisionStep;
        public int groundContactCount = 0;
        public float lastRw = 0;
        public int bellowGroundCount = 0;

        public override void InitializeAgent()
        {
            AddBodyPart(hips);
            AddBodyPart(leftUpperLeg);
            AddBodyPart(rightUpperLeg);
            AddBodyPart(leftLeg);
            AddBodyPart(rightLeg);
            AddBodyPart(leftFoot);
            AddBodyPart(rightFoot);

        }


        /// <summary>
        /// Add relevant information on each body part to observations.
        /// </summary>
        public void CollectObservationBodyPart(BodyPart bp)
        {
            var rb = bp.rigidBody;
            AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Is this bp touching the ground
            AddVectorObs(bp.targetContact.touchingTarget ? 1 : 0);
            AddVectorObs(rb.velocity);
            AddVectorObs(rb.angularVelocity);
            Vector3 localPosRelToHips = hips.InverseTransformPoint(rb.position);
            AddVectorObs(localPosRelToHips);

            if (bp.rigidBody.transform != hips )
            {
                AddVectorObs(bp.currentXNormalizedRot);
                AddVectorObs(bp.currentYNormalizedRot);
                AddVectorObs(bp.currentZNormalizedRot);
                AddVectorObs(bp.currentStrength / 25000);
            }
        }

        /// Loop over body parts to add them to observation.
        public override void CollectObservations()
        {
            AddVectorObs(dirToTarget.normalized);
            AddVectorObs(hips.forward);
            AddVectorObs(hips.up);

            foreach (var bodyPart in bodyParts)
            {
                CollectObservationBodyPart(bodyPart);
            }
        }


        public override void AgentAction(float[] vectorAction, string textAction)
        {
            if (isNewDecisionStep)
            {
                int i = -1;
                dirToTarget = target.position - bodyPartsDict[hips].rigidBody.position;
                bodyPartsDict[leftUpperLeg].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
                bodyPartsDict[leftLeg].SetJointTargetRotation(vectorAction[++i], 0, 0);
                bodyPartsDict[leftFoot].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
                bodyPartsDict[rightUpperLeg].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
                bodyPartsDict[rightLeg].SetJointTargetRotation(vectorAction[++i], 0, 0);
                bodyPartsDict[rightFoot].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);

                bodyPartsDict[leftUpperLeg].SetJointStrength(vectorAction[++i]);
                bodyPartsDict[leftLeg].SetJointStrength(vectorAction[++i]);
                bodyPartsDict[leftFoot].SetJointStrength(vectorAction[++i]);
                bodyPartsDict[rightUpperLeg].SetJointStrength(vectorAction[++i]);
                bodyPartsDict[rightLeg].SetJointStrength(vectorAction[++i]);
                bodyPartsDict[rightFoot].SetJointStrength(vectorAction[++i]);
                // Set reward for this step according to mixture of the following elements.
                // a. Velocity alignment with goal direction.
                // b. Rotation alignment with goal direction.
            }

            IncrementDecisionTimer();

            AddReward(
                    +0.03f * Vector3.Dot(dirToTarget.normalized, bodyPartsDict[hips].rigidBody.velocity)
                    + 0.01f * Vector3.Dot(dirToTarget.normalized, hips.forward)
                );

            // feet should be above ground 
            if (leftFoot.transform.position.y < 0 || rightFoot.transform.position.y < 0 
                                                  || rightLeg.transform.position.y < 0 || leftLeg.transform.position.y < 0
                                                  || rightUpperLeg.transform.position.y < 0 || leftUpperLeg.transform.position.y < 0)
            {
                AddReward(-1);
                bellowGroundCount++;
                Done();
            }
            
        }

        /// <summary>
        /// Loop over body parts and reset them to initial conditions.
        /// </summary>
        public override void AgentReset()
        {
            if (dirToTarget != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dirToTarget);
            }

            foreach (var bodyPart in bodyPartsDict.Values)
            {
                bodyPart.Reset(bodyPart);
            }
        }

        private void AddBodyPart(Transform t)
        {
            var bodyPart = new BodyPart
            {
                startingPos = transform.position,
                startingRot = transform.rotation,
                rigidBody = t.GetComponent<Rigidbody>(),
                joint = t.GetComponent<ConfigurableJoint>(),
            };

            bodyPart.rigidBody.maxAngularVelocity = 100;
            // Add & setup the ground contact script
            bodyPart.groundContact = t.GetComponent<MyGroundContact>();
            if (!bodyPart.groundContact)
            {
                bodyPart.groundContact = t.gameObject.AddComponent<MyGroundContact>();
                bodyPart.groundContact.agent = gameObject.GetComponent<MyWalkerAgent>();
            }
            else
            {
                bodyPart.groundContact.agent = gameObject.GetComponent<MyWalkerAgent>();
            }

            // Add & setup the target contact script
            bodyPart.targetContact = t.GetComponent<MyTargetContact>();
            if (!bodyPart.targetContact)
            {
                bodyPart.targetContact = t.gameObject.AddComponent<MyTargetContact>();
            }

            bodyParts.Add(bodyPart);
            bodyPartsDict.Add(t, bodyPart);
        }

        /// <summary>
        /// Only change the joint settings based on decision frequency.
        /// </summary>
        public void IncrementDecisionTimer()
        {
            if (currentDecisionStep == agentParameters.numberOfActionsBetweenDecisions ||
                agentParameters.numberOfActionsBetweenDecisions == 1)
            {
                currentDecisionStep = 1;
                isNewDecisionStep = true;
            }
            else
            {
                currentDecisionStep++;
                isNewDecisionStep = false;
            }
        }
    }
}


