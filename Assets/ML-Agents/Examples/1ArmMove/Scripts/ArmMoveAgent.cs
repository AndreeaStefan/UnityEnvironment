using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class ArmMoveAgent  : Agent
{

    private ArmMoveAcademy _academy;
    private const int MAX_TARGETS = 3;
    private int _numberOfTargetsTouched = 0;

    public GameObject Ground;
    public GameObject Area;
    public GameObject LeftArm;
    public GameObject RightArm;
    public List<Target> Targets;

    [Range(0, 50)]
    public float RotationAmount = 20f;

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
        if (LeftArm != null)
        {
            _leftArmRb = LeftArm.GetComponent<Rigidbody>();
        }

        if (RightArm != null)
        {
            _rightArmRb = RightArm.GetComponent<Rigidbody>();
        }
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

        CollectBodyPartObservation(LeftArm, _leftArmRb);
        CollectBodyPartObservation(RightArm, _rightArmRb);

        float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
        var detectableObjects = new[] { "wall", "target" };
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
        var armRotR = Vector3.zero;
        var armRotL = Vector3.zero;

        var moveBody = (int)act[0];
        var moveLeftArm = (int)act[1];
        var moveRightArm = (int)act[2];

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
          //  case 6:
           //     dirToGo = transform.forward * -0.5f;
                break;
        }

        switch (moveLeftArm)
        {
            case 1:
                armRotR = RightArm.transform.right * 0.75f * RotationAmount;
                break;
            case 2:
                armRotR = RightArm.transform.right * -0.75f * RotationAmount;
                break;
            case 3:
                armRotR = RightArm.transform.forward * -0.75f * RotationAmount;
                break;
            case 4:
                armRotR = RightArm.transform.forward * 0.75f * RotationAmount;
                break;
        }

        switch (moveRightArm)
        {
            case 1:
                armRotL = LeftArm.transform.right * 0.75f * RotationAmount;
                break;
            case 2:
                armRotL = LeftArm.transform.right * -0.75f * RotationAmount;
                break;
            case 3:
                armRotL = LeftArm.transform.forward * -0.75f * RotationAmount;
                break;
            case 4:
                armRotL = LeftArm.transform.forward * 0.75f * RotationAmount;
                break;
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 100f);
        _agentRb.MovePosition(transform.position + dirToGo * _academy.agentRunSpeed * Time.deltaTime);
        _rightArmRb.AddTorque(armRotR, ForceMode.VelocityChange);
        _leftArmRb.AddTorque(armRotL, ForceMode.VelocityChange);

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

    public void MoveAgent2(float[] act)
    {
        var x = Mathf.Clamp(act[0], -1, 1);
        var r = Mathf.Clamp(act[1], -1, 1);
        var rotationDirection = r >= 0 ? 1 : -1;
        var movingDirection = x >= 0 ? 1 : -1;

        transform.Rotate(transform.up * rotationDirection, Time.fixedDeltaTime * 2000f * r);
        _agentRb.AddForce(transform.forward * x * movingDirection * _academy.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void AgentReset()
    {
       
        transform.position = GetRandomSpawnPosition();
        _agentRb.velocity = Vector3.zero;
        _agentRb.angularVelocity = Vector3.zero;
        _numberOfTargetsTouched = 0;
        ResetConfig();
        foreach (var target in Targets)
        {
            target.isTriggered = false;
            target.ResetTransform();
        }
    }

    public void ResetConfig()
    {
        LeftArm.transform.localScale = new Vector3(LeftArm.transform.localScale.x, _academy.armScaleFactor, LeftArm.transform.localScale.z);
        RightArm.transform.localScale = new Vector3(LeftArm.transform.localScale.x, _academy.armScaleFactor, LeftArm.transform.localScale.z);
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
        AddReward(1);
        StartCoroutine(GoalScoredSwapGroundMaterial(_academy.successMaterial, 0.5f));
        if (_numberOfTargetsTouched >= MAX_TARGETS)
        {
            AddReward(10);
            Done();
            Debug.Log("DONE!!!");
        }
    }

    public void IsWall()
    {
          AddReward(-1f);
         // Done();
    }


}
