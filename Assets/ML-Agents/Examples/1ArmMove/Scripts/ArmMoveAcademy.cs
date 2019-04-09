using System.Collections;
using System.Collections.Generic;
using Assets.ML_Agents.Examples._1ArmMove.Scripts;
using MLAgents;
using UnityEngine;

public class ArmMoveAcademy : Academy
{

    /// <summary>
    /// The "walking speed" of the agents in the scene. 
    /// </summary>
    public float agentRunSpeed;

    /// <summary>
    /// The agent rotation speed.
    /// Every agent will use this setting.
    /// </summary>
    public float agentRotationSpeed;

    /// <summary>
    /// The spawn area margin multiplier.
    /// ex: .9 means 90% of spawn area will be used. 
    /// .1 margin will be left (so players don't spawn off of the edge). 
    /// The higher this value, the longer training time required.
    /// </summary>
    public float spawnAreaMarginMultiplier;

    /// <summary>
    /// When a goal is scored the ground will switch to this 
    /// material for a few seconds.
    /// </summary>
    public Material successMaterial;

    /// <summary>
    /// When an agent fails, the ground will turn this material for a few seconds.
    /// </summary>
    public Material failMaterial;

    /// <summary>
    /// The gravity multiplier.
    /// Use ~3 to make things less floaty
    /// </summary>
    public float gravityMultiplier;

    public float armScaleFactor = 1;

    void State()
    {
        Physics.gravity *= gravityMultiplier;

    }

    public override void AcademyReset()
    {
        var path = "E:\\Andreea\\Projects\\Git\\AvatarMaker\\AvatarMaker\\specification.json";
        var config = Helper.LoadJson(path);
        if (config != null)
        {
            armScaleFactor = config.arm_scale;
        }
    }
}
