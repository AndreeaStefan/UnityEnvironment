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

        [FormerlySerializedAs("constrains")] public dynamic limbsConfig;

        void Awake()
        {
            limbsConfig = AvatarConfiguration.GetConfiguration();
            _academy = FindObjectOfType<ArmMoveAcademy>();
        }


        public override void InitializeAgent()
        {
            base.InitializeAgent();
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
        
        public void CollectObservationBodyPart(BodyPart bp)
        {
            if(bp.rb.transform != Root.transform)
            {
                AddVectorObs(bp.currentXNormalizedRot);
                AddVectorObs(bp.currentYNormalizedRot);
                AddVectorObs(bp.currentZNormalizedRot);
                AddVectorObs(bp.currentStrength/_jdController.maxJointForceLimit);
            }
        }

        public override void CollectObservations()
        {
            foreach (var bodyPart in _jdController.bodyPartsDict.Values)
            {
                CollectObservationBodyPart(bodyPart);
            }
            // Adding position of hands relative to the floor
            _hands.ForEach(h => AddVectorObs(Helper.getRelativePosition(Floor.transform, h.position)));
           // Local position of targets is already relative to the floor
           _targets.ForEach(t => AddVectorObs(t.transform.localPosition));
         
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
                
                bpDict[bp.transform].SetJointTargetRotation(x, y, z);
                bpDict[bp.transform].SetJointStrength((float) iterator.Current);
                iterator.MoveNext();

            });
            
            // moving the agent part
            var direction = Mathf.Clamp((float)iterator.Current, -1, 1);
            iterator.MoveNext();
            var rotation = Mathf.Clamp((float)iterator.Current, -1, 1);
            transform.Rotate(Root.transform.up, Time.fixedDeltaTime * 20 * rotation);
            _agentRb.MovePosition(Root.transform.position + Root.transform.forward * direction * _academy.agentRunSpeed * Time.fixedDeltaTime);
        }

        public override void AgentReset()
        {
            transform.position = GetRandomSpawnPosition();
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

        IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
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
            position.y = Random.Range(0.5f, 3.5f);
            return position;
        }

        public void IsTarget()
        {
            AddReward(5);
            StartCoroutine(GoalScoredSwapGroundMaterial(_academy.successMaterial, 0.5f));
        }
        
        public void ResetTarget(GameObject target)
        {
            target.transform.position = GetRandomSpawnTargetPosition();
        }

        public void IsWall()
        {
            AddReward(-5f);
            StartCoroutine(GoalScoredSwapGroundMaterial(_academy.failMaterial, 0.5f));
            Done();
        }


    }
}
