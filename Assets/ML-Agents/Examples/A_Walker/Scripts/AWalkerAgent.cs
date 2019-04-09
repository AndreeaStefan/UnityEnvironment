using System.Collections;
using System.Collections.Generic;
using Examples.A_Walker.Scripts;
using MLAgents;
using UnityEngine;

public class AWalkerAgent : Agent
{
[Header("Specific to MyWalker")]
    public GameObject ground;
    public GameObject arena;
    public GameObject goal;
    [HideInInspector]
    public GoalDetector goalDetector;
    
    private Renderer groundRenderer;
    private Material groundMaterial;
    private Rigidbody playerRB;
    private Bounds arenaBounds;
    private RayPerception perception;
    private AWalkerAcademy academy;

    
    
    private void Awake()
    {
        academy = FindObjectOfType<AWalkerAcademy>();
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        goalDetector = GetComponent<GoalDetector>();
        goalDetector.agent = this;
        playerRB = GetComponent<Rigidbody>();
        arenaBounds = ground.GetComponent<Collider>().bounds;
        groundRenderer = ground.GetComponent<Renderer>();
        groundMaterial = groundRenderer.material;
        perception = GetComponent<RayPerception>();
    }

    public override void CollectObservations()
    {
        const float rayDistance = 10f;
        float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
        var detectableObjects = new[] { "goal", "wall" };
        // Size of the observation vector: 28; angles * (objects + 2)
        AddVectorObs(perception.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
    }
  
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Move the agent using the action.
        MoveAgent(vectorAction);

        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / agentParameters.maxStep);
    }

    // brain set on continuous; two floats, 1# X; 2# Rotation
    private void MoveAgent(float[] act)
    {
        var x = Mathf.Clamp(act[0], -1, 1);
        var r = Mathf.Clamp(act[1], -1, 1);
        var rotationDirection = r >= 0 ? 1 : -1;
        var movingDirection = x >= 0 ? 1 : -1;

        transform.Rotate(transform.up * rotationDirection, Time.fixedDeltaTime * 2000f * r );
        playerRB.AddForce(transform.forward * x * movingDirection * academy.agentRunSpeed , ForceMode.VelocityChange);
    }
    
    
    public override void AgentReset()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        arena.transform.Rotate(new Vector3(0, rotationAngle, 0f));

        transform.position = GetRandomSpawnPosition();
        playerRB.velocity = Vector3.zero;
        playerRB.angularVelocity = Vector3.zero;
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-arenaBounds.extents.x * academy.spawnAreaMarginMultiplier,
                arenaBounds.extents.x * academy.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-arenaBounds.extents.z * academy.spawnAreaMarginMultiplier,
                arenaBounds.extents.z * academy.spawnAreaMarginMultiplier);
            
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
            
            // Checks if not colliding with anything
            if (Physics.CheckBox(randomSpawnPos, new Vector3(1f, 0.01f, 1f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }


    public void ReachedGoal()
    {
        AddReward(5f);
        Done();
        StartCoroutine(HighlightGround(academy.goalScoredMaterial, 0.5f));
    }

    IEnumerator HighlightGround(Material mat, float time)
    {
        groundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        groundRenderer.material = groundMaterial;
    }
}
