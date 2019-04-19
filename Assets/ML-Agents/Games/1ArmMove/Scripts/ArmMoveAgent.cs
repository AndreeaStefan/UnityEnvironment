﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArmMove
{
    public class ArmMoveAgent : Agent
    {

        private ArmMoveAcademy _academy;
        private const int MAX_TARGETS = 3;
        private int _numberOfTargetsTouched = 0;

        public GameObject Ground;
        public GameObject Area;
        public GameObject LeftArm;
        public GameObject RightArm;
        public GameObject TargetsContainer;
        private List<Transform> _targets;

       // public List<GameObject> bodyPartsGO;

        [HideInInspector] public List<BodyPart> bodyParts;

        [Range(0, 50)] public float RotationAmount = 20f;

        Rigidbody _agentRb;
        RayPerception _rayPer;
        Renderer _groundRenderer;
        Bounds _areaBounds;
        Material _groundMaterial;
        WallContact _detectWall;
        private ArmSpawner _armSpawner;

        public Dictionary<string, BodyPartConstrain> constrains;

        void Awake()
        {
            _academy = FindObjectOfType<ArmMoveAcademy>();
            var path = "../../../../config/specification.json";
            var config = Helper.LoadJson(path);
            if (config != null)
            {
                constrains = config;
            }
        }


        public override void InitializeAgent()
        {
            base.InitializeAgent();

            _rayPer = GetComponent<RayPerception>();
            _agentRb = GetComponent<Rigidbody>();
            _armSpawner = GetComponent<ArmSpawner>();

            _areaBounds = Ground.GetComponent<Collider>().bounds;
            _groundRenderer = Ground.GetComponent<Renderer>();
            _groundMaterial = _groundRenderer.material;

            _detectWall = GetComponent<WallContact>();
            _detectWall.agent = this;

            _targets = TargetsContainer.GetComponentsInChildren<Transform>().ToList();

            bodyParts = new List<BodyPart>();

            AddBodyParts(LeftArm.transform);
            AddBodyParts(RightArm.transform);

        }

        private void AddBodyParts(Transform obj)
        {

            for (var i = 0; i < obj.childCount; i++)
            {
                bodyParts.Add(constrains.ContainsKey(obj.GetChild(i).name)
                    ? new BodyPart(obj.GetChild(i), constrains[obj.GetChild(i).name])
                    : new BodyPart(obj.GetChild(i), BodyPartConstrain.GetDefault()));
                AddBodyParts(obj.GetChild(i));
            }
        }

        public void CollectBodyPartObservation(Transform bp, Rigidbody rb)
        {
            AddVectorObs(bp.transform.localPosition);
            AddVectorObs(bp.transform.rotation);
            AddVectorObs(rb.angularVelocity);
            AddVectorObs(rb.velocity);
        }

        public override void CollectObservations()
        {

            var rayDistance = 10f;
            AddVectorObs(_areaBounds.min);
            AddVectorObs(_areaBounds.max);
            foreach (var bp in bodyParts)
            {
                CollectBodyPartObservation(bp.transform, bp.rb);
            }
         

            float[] rayAngles = {0f, 45f, 90f, 135f, 180f, 110f, 70f};
            var detectableObjects = new[] {"wall", "target"};
            AddVectorObs(_rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
            foreach (var target in _targets)
            {
                AddVectorObs(target.transform.localPosition);
            }

        }

        public override void AgentAction(float[] vectorAction, string textAction)
        {
            // Move the agent using the action.
            MoveAgent(vectorAction);

            // Penalty given each step to encourage agent to finish task quickly.
            AddReward(-1f / agentParameters.maxStep);
        }

        /// <summary>
        /// Moves the agent according to the selected action.
        /// </summary>
        public void MoveAgent(float[] act)
        {

            var dirToGo = Vector3.zero;
            var rotateDir = Vector3.zero;

            var moveBody = (int) act[0];
           
            switch (moveBody)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    rotateDir = transform.up * -1f;
                    break;
                case 3:
                    rotateDir = transform.up * 1f;
                    break;
                case 4:
                    dirToGo = transform.right * -0.75f;
                    break;
                case 5:
                    dirToGo = transform.right * 0.75f;
                    break;
                case 6:
                    dirToGo = transform.forward * -0.5f;
                    break;
            }

            for (int i = 1; i < act.Length; i++)
            {
                var moveBodyPart = (int)act[i]; // the actions for the i'th body part
                var rotation = Vector3.zero;
                switch (moveBodyPart)
                {
                    case 1:
                        rotation = RightArm.transform.right * 0.75f * RotationAmount;
                        break;
                    case 2:
                        rotation = RightArm.transform.right * -0.75f * RotationAmount;
                        break;
                    case 3:
                        rotation = RightArm.transform.forward * -0.75f * RotationAmount;
                        break;
                    case 4:
                        rotation = RightArm.transform.forward * 0.75f * RotationAmount;
                        break;
                }

                //ToDo: improve  indexing 
                bodyParts[i-1].rb.AddTorque(rotation, ForceMode.VelocityChange);
            }

            transform.Rotate(rotateDir, Time.fixedDeltaTime * 100f);
            _agentRb.MovePosition(transform.position + dirToGo * _academy.agentRunSpeed * Time.deltaTime);

            var posX = Ground.transform.position.x + transform.localPosition.x;
            var posZ = Ground.transform.position.z + transform.localPosition.z;
            if (posX < _areaBounds.min.x || posX > _areaBounds.max.x ||
                posZ < _areaBounds.min.z || posZ > _areaBounds.max.z)
            {
                AddReward(-5);
                Debug.Log("Outside game area");
                Done();

            }
        }

        public override void AgentReset()
        {

            transform.position = GetRandomSpawnPosition();
            _agentRb.velocity = Vector3.zero;
            _agentRb.angularVelocity = Vector3.zero;
            _numberOfTargetsTouched = 0;
            foreach (var target in _targets)
            {
                target.transform.position = GetRandomSpawnPosition();
            }

            foreach (var bp in bodyParts)
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

                randomSpawnPos = Ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);

                // Checks if not colliding with anything
                if (Physics.CheckBox(randomSpawnPos, new Vector3(1f, 0.01f, 1f)) == false)
                {
                    foundNewSpawnLocation = true;
                }
            }

            return randomSpawnPos;
        }

        public void IsTarget()
        {
            Debug.Log("Targets hit " + _numberOfTargetsTouched);
            _numberOfTargetsTouched += 1;
            AddReward(5);
            StartCoroutine(GoalScoredSwapGroundMaterial(_academy.successMaterial, 0.5f));
        }
        
        public void ResetTarget(GameObject target)
        {
            target.transform.position = GetRandomSpawnPosition();
        }

        public void IsWall()
        {
            AddReward(-1f);
        }


    }
}
