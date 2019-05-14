
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using MLAgents.CommunicatorObjects;
using UnityEngine;

namespace ArmMove
{
    public class ArmMoveAcademy : Academy
    {
        [HideInInspector] public Brain brain;

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

        // Set the brain here
        private void Awake()
        {
            Debug.Log("About to create brain");
            Debug.Log("Streaming assets path: " + Application.streamingAssetsPath);
            
            
           if (this.broadcastHub.IsControlled(this.broadcastHub.broadcastingBrains.First()))
           {
                var brainHandler = new BrainHandler();

                brain = brainHandler.GetBrain("Brain2");
                Debug.Log($"Brain created: {brain}\n\t{brain.brainParameters}");
                broadcastHub.broadcastingBrains = new List<Brain> { brain };
                broadcastHub.SetControlled(brain, true);

                FindObjectsOfType<ArmMoveAgent>().ToList().ForEach(agent => agent.GiveBrain(brain));
                Debug.Log("Brain implanted in all the agents.");
            }
            
            InitializeEnvironment();
        }
    }
}
