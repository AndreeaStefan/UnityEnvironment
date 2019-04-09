using MLAgents;
using System;
using System.Collections;
using Assets.ML_Agents.Examples._1Obstacles.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstaclesAgent : Agent
{

    ObstaclesAcademy academy;
    int configuration;
    /// <summary>
    /// The goal to reach
    /// </summary>
    public GameObject Goal;

    public Brain theBrain;

    /// <summary>
    /// Detects when the block touches the goal.
    /// </summary>
	[HideInInspector]
    public FinishDetect DetectFinish;

    [HideInInspector]
    public WallDetect DetectWall;

    [HideInInspector]
    public ObstaclesDetect DetectObstacle;

    public bool UseVectorObs;
    public GameObject ground;
    public GameObject area;

    /// <summary>
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;

    Rigidbody agentRB;  //cached on initialization
    RayPerception rayPer;
    Vector3 initalPos;
    public double distanceToTarget;
    Material groundMaterial; //cached on Awake()
    private int currentConfig = 0;
    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer groundRenderer;

    private ConfigurationController config = new ConfigurationController();


    void Awake()
    {
        academy = FindObjectOfType<ObstaclesAcademy>(); //cache the academy
        config.InitConfiguration();
    }


    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rayPer = GetComponent<RayPerception>();
        DetectFinish = GetComponent<FinishDetect>();
        DetectWall = GetComponent<WallDetect>();
        DetectObstacle = GetComponent<ObstaclesDetect>();

        DetectFinish.agent = this;
        DetectWall.agent = this;
        DetectObstacle.agent = this;

        // Cache the agent rigidbody
        agentRB = GetComponent<Rigidbody>();

        var diff = transform.position - Goal.transform.position;
        distanceToTarget =  Math.Sqrt( Vector3.Dot(diff, diff));

         areaBounds = ground.GetComponent<Collider>().bounds;
        groundRenderer = ground.GetComponent<Renderer>();
        groundMaterial = groundRenderer.material;
    }

    public override void CollectObservations()
    {
        if (UseVectorObs)
        {
            var rayDistance = 10f;
            float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
            var detectableObjects = new[] { "wall", "Finish", "obstacle" };
            AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
         
        }
    }

    public void OnAcademyDone()
    {
        var scaleF = config.GetConfigAt(academy.configStep).ScaleArea;
        area.transform.localScale *= scaleF;

        scaleF = config.GetConfigAt(academy.configStep).ScalePlayer;
        transform.localScale *= scaleF;

    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(float[] act)
    {

        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        int action = Mathf.FloorToInt(act[0]);

        // Goalies and Strikers have slightly different action spaces.
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                // AddReward(0.1f);
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
       // agentRB.AddForce(dirToGo * academy.agentRunSpeed, ForceMode.VelocityChange);

        agentRB.MovePosition(transform.position + dirToGo * academy.agentRunSpeed * Time.deltaTime);
       
        var diff = transform.position - Goal.transform.position;
        var dist = Math.Sqrt(Vector3.Dot(diff, diff));
        if (dist < distanceToTarget)
        {
            AddReward(0.1f);
            distanceToTarget = dist;
        }
        else
        {
            //  AddReward(-0.5f);
         //   distanceToTarget = dist;
        }
        
    }


    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Move the agent using the action.
        MoveAgent(vectorAction);

        // Penalty given each step to encourage agent to finish task quickly.
         AddReward(-1f / agentParameters.maxStep);
    }

    public override void AgentReset()
    {
        if (currentConfig != academy.configStep)
        {
            OnAcademyDone();
            currentConfig = academy.configStep;
        }
        transform.position = GetRandomSpawnPosition();
        agentRB.velocity = Vector3.zero;
        agentRB.angularVelocity = Vector3.zero;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-areaBounds.extents.x * academy.spawnAreaMarginMultiplier,
                areaBounds.extents.x * academy.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-areaBounds.extents.z * academy.spawnAreaMarginMultiplier,
                areaBounds.extents.z * academy.spawnAreaMarginMultiplier);

            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);

            // Checks if not colliding with anything
            if (Physics.CheckBox(randomSpawnPos, new Vector3(1f, 0.01f, 1f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    public void IsFinish()
    {
        // We use a reward of 5.
        AddReward(15f);

        // By marking an agent as done AgentReset() will be called automatically.
        Done();

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(academy.goalScoredMaterial, 0.5f));
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        groundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        groundRenderer.material = groundMaterial;
    }

    public void IsObstacle()
    {
        // When an obstacles is hit: -3 reward
        AddReward(-5f);

      //  Done();

        StartCoroutine(GoalScoredSwapGroundMaterial(academy.failMaterial, 0.5f));
    }

    public void IsWall()
    {
      //  AddReward(-1f);
    }
}
