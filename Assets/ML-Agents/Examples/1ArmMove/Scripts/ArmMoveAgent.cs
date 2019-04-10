using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Examples._1ArmMove.Scripts;
using MLAgents;
using UnityEngine;
using UnityEngine.Serialization;

public class ArmMoveAgent : Agent
{
    private ArmMoveAcademy _academy;

    public GameObject Ground;
    public GameObject Area;
    public GameObject LeftArm;
    public GameObject RightArm;
    public GameObject Targets;
    private List<Transform> _targets;
    
    Rigidbody _leftArmRb;
    Rigidbody _rightArmRb;
    Rigidbody _agentRb;
    RayPerception _rayPer;
    Renderer _groundRenderer;
    Bounds _areaBounds;
    Material _groundMaterial;

    private Vector3 _leftInitialPosition;
    private Vector3 _rightInitialPosition;

    private Quaternion _leftInitialRotation;
    private Quaternion _rightInitialRotation;

    private CollisionDetector _collider;

    private Transform _leftHand;
    private Transform _rightHand;
    

    void Awake()
    {
        _academy = FindObjectOfType<ArmMoveAcademy>();
    }


    public override void InitializeAgent()
    {
        base.InitializeAgent();
        _rayPer = GetComponent<RayPerception>();
        _agentRb = GetComponent<Rigidbody>();

        _leftInitialPosition = LeftArm.transform.localPosition;
        _leftInitialRotation = LeftArm.transform.localRotation;

        _rightInitialPosition = RightArm.transform.localPosition;
        _rightInitialRotation = RightArm.transform.localRotation;

        _areaBounds = Ground.GetComponent<Collider>().bounds;
        _groundRenderer = Ground.GetComponent<Renderer>();
        _groundMaterial = _groundRenderer.material;

        _leftArmRb = LeftArm.GetComponent<Rigidbody>();
        _rightArmRb = RightArm.GetComponent<Rigidbody>();

        _leftHand = LeftArm.GetComponentInChildren<Transform>();
        _rightHand = RightArm.GetComponentInChildren<Transform>();

        _targets = Targets.GetComponentsInChildren<Transform>().ToList();

    }

    public void CollectBodyPartObservation(Transform bodyPartTransform, Rigidbody rb)
    {
        var localPosition = bodyPartTransform.localPosition;
        AddVectorObs(localPosition);
        AddVectorObs(bodyPartTransform.rotation);
        AddVectorObs(rb.angularVelocity);
        AddVectorObs(rb.velocity);
    }

    public override void CollectObservations()
    {
        var rayDistance = 10f;

        CollectBodyPartObservation(LeftArm.transform, _leftArmRb);
        CollectBodyPartObservation(RightArm.transform, _rightArmRb);
        
        AddVectorObs(_rightHand.position);
        AddVectorObs(_leftHand.position);
        _targets.ForEach(t => AddVectorObs(t.localPosition));
        
        float[] rayAngles = {0f, 45f, 90f, 135f, 180f, 110f, 70f};
        var detectableObjects = new[] {"wall"};
        AddVectorObs(_rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Move the agent using the action.
        MoveAgent(vectorAction);
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    private void MoveAgent(float[] act)
    {
        // moving the agent part
        var x = Mathf.Clamp(act[0], -1, 1);
        var r = Mathf.Clamp(act[1], -1, 1);

        transform.Rotate(transform.up, Time.fixedDeltaTime * 20 * r);
        _agentRb.MovePosition(transform.position + transform.forward * x * _academy.agentRunSpeed * Time.fixedDeltaTime);
        
        //moving the arms
        var torqueX = Mathf.Clamp(act[2], -1f, 1f) * 20;
        var torqueZ = Mathf.Clamp(act[3], -1f, 1f) * 20;
        _leftArmRb.AddTorque(new Vector3(torqueX, 0f, torqueZ));

        torqueX = Mathf.Clamp(act[4], -1f, 1f) * 20;
        torqueZ = Mathf.Clamp(act[5], -1f, 1f) * 20;
        _rightArmRb.AddTorque(new Vector3(torqueX, 0f, torqueZ));
        
        
    }


    public override void AgentReset()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        Area.transform.Rotate(new Vector3(0, rotationAngle, 0f));

        transform.position = GetRandomSpawnPosition();
        LeftArm.transform.localPosition = _leftInitialPosition;
        RightArm.transform.localPosition = _rightInitialPosition;
        LeftArm.transform.localRotation = _leftInitialRotation;
        RightArm.transform.localRotation = _rightInitialRotation;

        _agentRb.velocity = Vector3.zero;
        _agentRb.angularVelocity = Vector3.zero;
        _leftArmRb.velocity = Vector3.zero;
        _rightArmRb.velocity = Vector3.zero;
        _leftArmRb.angularVelocity = Vector3.zero;
        _rightArmRb.angularVelocity = Vector3.zero;
        
        
        ResetConfig();
    }

    public void ResetConfig()
    {
//        LeftArm.transform.localScale = new Vector3(LeftArm.transform.localScale.x, _academy.armScaleFactor,
//            LeftArm.transform.localScale.z);
//        RightArm.transform.localScale = new Vector3(LeftArm.transform.localScale.x, _academy.armScaleFactor,
//            LeftArm.transform.localScale.z);
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
        AddReward(1f);
        StartCoroutine(GoalScoredSwapGroundMaterial(_academy.successMaterial, 0.5f));
    }

    public void IsWall()
    {
        AddReward(-5f);
        StartCoroutine(GoalScoredSwapGroundMaterial(_academy.failMaterial, 0.5f));
        Done();
    }


    public void ResetTarget(GameObject target)
    {
        target.transform.position = GetRandomSpawnPosition();
    }
}