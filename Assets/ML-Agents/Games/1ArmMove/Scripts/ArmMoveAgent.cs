using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

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
        public List<Target> Targets;

       // public List<GameObject> bodyPartsGO;

        [HideInInspector] public List<BodyPart> bodyParts;

        [Range(0, 50)] public float RotationAmount = 20f;

        Rigidbody _leftArmRb;
        Rigidbody _rightArmRb;
        Rigidbody _agentRb;
        RayPerception _rayPer;
        Renderer _groundRenderer;
        Bounds _areaBounds;
        Material _groundMaterial;
        WallContact _detectWall;
        private bool inArea;


        void Awake()
        {
            _academy = FindObjectOfType<ArmMoveAcademy>();
            var path = "E:\\Andreea\\Projects\\Git\\AvatarMaker\\AvatarMaker\\specification.json";
            var config = Helper.LoadJson(path);
            if (config != null)
            {
                _academy.constrains = config;
            }
        }


        public override void InitializeAgent()
        {
            base.InitializeAgent();
            inArea = true;
            _rayPer = GetComponent<RayPerception>();
            _agentRb = GetComponent<Rigidbody>();

            _areaBounds = Ground.GetComponent<Collider>().bounds;
            _groundRenderer = Ground.GetComponent<Renderer>();
            _groundMaterial = _groundRenderer.material;

            _detectWall = GetComponent<WallContact>();
            _detectWall.agent = this;


            bodyParts = new List<BodyPart>
            {
                new BodyPart(LeftArm, _academy.constrains[0]),
                new BodyPart(RightArm, _academy.constrains[1])
            };
        }

        public void CollectBodyPartObservation(GameObject bp, Rigidbody rb)
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
                CollectBodyPartObservation(bp.gameObject, bp.rb);
            }
         

            float[] rayAngles = {0f, 45f, 90f, 135f, 180f, 110f, 70f};
            var detectableObjects = new[] {"wall", "target"};
            AddVectorObs(_rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
            foreach (var target in Targets)
            {
                AddVectorObs(target.isTriggered);
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
            var count = 0;
            foreach (var target in Targets)
            {
                target.isTriggered = false;
                target.id = count;
                target.ResetTransform();
                target.transform.position = GetRandomSpawnPosition();
                count += 1;
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

        public void IsTarget(int id)
        {

            Debug.Log("Targets hit " + _numberOfTargetsTouched);
            _numberOfTargetsTouched += 1;
            AddReward(5);
            StartCoroutine(GoalScoredSwapGroundMaterial(_academy.successMaterial, 0.5f));

            Targets[id].isTriggered = false;
            Targets[id].ResetTransform();
            Targets[id].transform.position = GetRandomSpawnPosition();
            var waitForSeconds = new WaitForSeconds(0.2f);
            /*
            if (_numberOfTargetsTouched >= MAX_TARGETS)
            {
                AddReward(10);
                Done();
                Debug.Log("DONE!!!");
            }
            */
        }

        public void IsWall()
        {
            AddReward(-1f);
            // Done();
        }


    }
}
