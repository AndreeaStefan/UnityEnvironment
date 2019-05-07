using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ArmMove
{
    public class ArmMoveAgent : Agent
    {

        [FormerlySerializedAs("Ground")] public GameObject Floor;
        public GameObject LimbsContainer;
        public GameObject TargetsContainer;
        public GameObject Root;
        private ArmMoveAcademy _academy;
        
        private List<Transform> _targets;
        private List<Transform> _limbsTransform;
        private List<AnhaBodyPart> _limbsBodyParts;
        private List<Transform> _hands;
        private JointDriveController _jdController;
        
        private Rigidbody _agentRb;
        private Renderer _groundRenderer;
        private Bounds _areaBounds;
        private Material _groundMaterial;

        private float _direction;
        private float _rotation;

        private int decisionCounter;
        
        [FormerlySerializedAs("constrains")] public dynamic limbsConfig;

        void Awake()
        {
            limbsConfig = AvatarConfiguration.GetConfiguration();
            _academy = FindObjectOfType<ArmMoveAcademy>();
        }

        public override void InitializeAgent()
        {
            base.InitializeAgent();
            decisionCounter = 0;
            _limbsTransform = new List<Transform>();
            _limbsBodyParts = new List<AnhaBodyPart>();
            
            _agentRb = Root.GetComponent<Rigidbody>();
            _areaBounds = Floor.GetComponent<Collider>().bounds;
            _groundRenderer = Floor.GetComponent<Renderer>();
            _groundMaterial = _groundRenderer.material;

            _targets = TargetsContainer.GetComponentsInChildren<Transform>().Skip(1).ToList();

            // Automatically adding all the bones of limbs;
            // Each limb ought to end with a child object with tag "hand"
            _hands = Finder.ChildrenWithTag(LimbsContainer.transform, "hand");
            SetLimbs();
            
            // SETTING UP THE joints CONTROLLER
            _jdController = GetComponent<JointDriveController>();
            _jdController.SetupBodyPart(Root.transform);
            _limbsTransform.ForEach(bp => _jdController.SetupBodyPart(bp));
        }

        private void SetLimbs()
        {
            var limbsConfigs = limbsConfig["limbs"];
            var i = 0;
            
            _hands.ForEach(h =>
            {
                var limbConfig = limbsConfigs[i];

                var bone = h.parent;
                var joint = bone.GetComponent<Joint>();

                while (joint)
                {
                    _limbsBodyParts.Add( new AnhaBodyPart(bone, new BodyPartConstraint(limbConfig)));
                    _limbsTransform.Add(bone);
                    
                    limbConfig = limbConfig["parent"];
                    bone = joint.connectedBody.transform;
                    joint = bone.GetComponent<Joint>();
                }
                i++;
            });
            
        }
        
        

        public override void CollectObservations()
        {
            // skip body
             foreach (var bodyPart in _jdController.bodyPartsDict.Values.Skip(1))
            {
                CollectObservationBodyPart(bodyPart);
            }

            // Adding position of hands 
            _hands.ForEach(h => AddVectorObs(Helper.getRelativePosition(Floor.transform, h.position)));
            // Adding position of targets 
            _targets.ForEach(t => AddVectorObs(t.transform.localPosition));

            // Adding position of hands relative to the targets
          //   _hands.ForEach(h =>  _targets.ForEach(t => AddVectorObs(Helper.getRelativePosition(t.transform, h.position))));

        }
        
        private void CollectObservationBodyPart(BodyPart bp)
        {
            if(bp.rb.transform != Root.transform)
            {
                AddVectorObs(bp.currentXNormalizedRot);
                AddVectorObs(bp.currentYNormalizedRot);
                AddVectorObs(bp.currentZNormalizedRot);
                AddVectorObs(bp.currentStrength/_jdController.maxJointForceLimit);
            }
        }

        
        
        // the action space depends on the joints setting. If an axis is locked, no action is needed
        public override void AgentAction(float[] vectorAction, string textAction)
        {
            var bpDict = _jdController.bodyPartsDict;
            var iterator = vectorAction.GetEnumerator();
            iterator.MoveNext();
            
            _limbsBodyParts.ForEach(bp =>
            {
                float x;
                float y;
                float z;
                if(bp.constraints.XRotationLocked)
                    x = 0f;
                else
                {
                    x = (float) iterator.Current;
                    iterator.MoveNext();
                }
                if(bp.constraints.YRotationLocked)
                    y = 0f;
                else
                {
                    y = (float) iterator.Current;
                    iterator.MoveNext();
                }
                if(bp.constraints.ZRotationLocked)
                    z = 0f;
                else
                {
                    z = (float) iterator.Current;
                    iterator.MoveNext();
                }

                bpDict[bp.transform].SetJointStrength((float)iterator.Current);
                bpDict[bp.transform].SetJointTargetRotation(x, y, z);
                
                iterator.MoveNext();

            });
            
            // moving the agent part
            _direction = (float) iterator.Current;
            iterator.MoveNext();
            _rotation = (float)iterator.Current;
        }
        
        void FixedUpdate()
        {
            if (decisionCounter == 0)
            {
                decisionCounter = 3;
                RequestDecision();
            }
            else
            {
                decisionCounter--;
            }

            var dir = Mathf.Clamp(_direction, -1, 1);
            var rot = Mathf.Clamp(_rotation, -1, 1);


            Root.transform.Rotate(Root.transform.up, Time.fixedDeltaTime * 200 * rot);
            _agentRb.MovePosition(Root.transform.position + Root.transform.forward * dir * _academy.agentRunSpeed * Time.fixedDeltaTime);

            // Penalty for movement of the body
            var bodyRotationPenalty = -0.001f * Math.Abs(_rotation);
            var bodMovementPenalty = -0.001f * Math.Abs(_direction);
            AddReward(bodyRotationPenalty);
            AddReward(bodMovementPenalty);

            // Penalty for time
            RewardFunctionTimePenalty();
            AssessState();
        }


        /// <summary>
        /// Time penalty
        /// gets a pentalty each step so that it tries to finish as quickly as possible.
        /// </summary>
        void RewardFunctionTimePenalty()
        {
            AddReward(-0.001f);  //-0.001f chosen by experimentation.
        }

        private void AssessState()
        {
            
        }

        public override void AgentReset()
        {
            Root.transform.position = GetRandomSpawnPosition();
            _agentRb.velocity = Vector3.zero;
            _agentRb.angularVelocity = Vector3.zero;
            foreach (var target in _targets)
            {
                target.transform.position = GetRandomSpawnTargetPosition();
            }

            foreach (var bp in _limbsBodyParts)
            {
                bp.ResetBodyPart();
            }
        }

        IEnumerator SwapGroundMaterial(Material mat, float time)
        {
            _groundRenderer.material = mat;
            yield return new WaitForSeconds(time); // Wait for 2 sec
            _groundRenderer.material = _groundMaterial;
        }

        
        
        private Vector3 GetRandomSpawnPosition()
        {
            var foundNewSpawnLocation = false;
            var randomSpawnPos = Vector3.zero;
            while (foundNewSpawnLocation == false)
            {
                var randomPosX = Random.Range(-_areaBounds.extents.x * _academy.spawnAreaMarginMultiplier,
                    _areaBounds.extents.x * _academy.spawnAreaMarginMultiplier);

                var randomPosZ = Random.Range(-_areaBounds.extents.z * _academy.spawnAreaMarginMultiplier,
                    _areaBounds.extents.z * _academy.spawnAreaMarginMultiplier);

                randomSpawnPos = Floor.transform.position + new Vector3(randomPosX, 1f, randomPosZ);

                // Checks if not colliding with anything
                if (Physics.CheckBox(randomSpawnPos, new Vector3(1f, 0.01f, 1f)) == false)
                {
                    foundNewSpawnLocation = true;
                }
            }

            return randomSpawnPos;
        }

        private Vector3 GetRandomSpawnTargetPosition()
        {
            var position = GetRandomSpawnPosition();
            position.y = Random.Range(0.5f, 2.5f);
            return position;
        }

        public void IsTarget()
        {
            AddReward(5);
          
           
           // _hands.ForEach(h => Debug.Log("Hand-Global" +h.position));
          //  _targets.ForEach(t => Debug.Log("Target-Global" + t.transform.position));

          //  _hands.ForEach(h => Debug.Log("Hand2" + Helper.getRelativePosition(Floor.transform, h.position)));
          //  _targets.ForEach(t => Debug.Log("Target2" + t.transform.localPosition));
          //     _targets.ForEach(t => Debug.Log("Target - local " +  t.transform.localPosition));
           //    _targets.ForEach(t => Debug.Log("Target - relative" + Helper.getRelativePosition(Floor.transform, t.transform.position)));

            StartCoroutine(SwapGroundMaterial(_academy.successMaterial, 0.5f));
        }
        
        public void ResetTarget(GameObject target)
        {
            target.transform.position = GetRandomSpawnTargetPosition();
        }

        public void IsWall()
        {
            AddReward(-5f);
            StartCoroutine(SwapGroundMaterial(_academy.failMaterial, 0.5f));
            Done();
        }


    }
}
