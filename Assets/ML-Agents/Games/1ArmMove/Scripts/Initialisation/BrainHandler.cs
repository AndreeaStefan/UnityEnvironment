using System;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Resolution = MLAgents.Resolution;

namespace ArmMove
{
    public class BrainHandler
    {
        private static dynamic _configuration;
        
        public Brain GetBrain()
        {
            if (_configuration == null) _configuration = AvatarConfiguration.GetConfiguration();
            
            var brain = ScriptableObject.CreateInstance<LearningBrain>();
            var brainParameters = new BrainParameters
            {
                numStackedVectorObservations = _configuration["numStackedVectorsObservation"],
                vectorObservationSize = GetObservationSize(),
                vectorActionSpaceType = SpaceType.continuous,
                vectorActionSize = GetActionVectorSize(),
                cameraResolutions = new Resolution[]{}
            };
            brain.brainParameters = brainParameters;
            brain.SetToControlledExternally();
            
            return brain;
        }

        private int GetObservationSize()
        {
            var size = _configuration["numTargets"] * 3;
            size += ((JArray)_configuration["limbs"]).Count * 3;
            size += ((JArray)_configuration["limbs"]).Select(l =>
            {
                var tmp = 1;
                var parent = l["parent"];
                while (parent.HasValues)
                {
                    parent = parent["parent"];
                    tmp++;
                }
                // from one bone we have 4 observations
                return tmp * 4;
            }).Sum();
            return size;
        }
        
        
        private int[] GetActionVectorSize()
        {
            // 2 for moving and rotating the body
            var size = 2;
            size += ((JArray)_configuration["limbs"]).Select(l =>
            {
                var tmp = 0;

                if (l["XRotationLocked"].Value<bool>() == false)
                    tmp++;
                if (l["YRotationLocked"].Value<bool>() == false)
                    tmp++;
                if (l["ZRotationLocked"].Value<bool>() == false)
                    tmp++;
                // 1 always for strength
                tmp++;
                
                var parent = l["parent"];
                while (parent.HasValues)
                {
                    if (parent["XRotationLocked"].Value<bool>() == false)
                        tmp++;
                    if (parent["YRotationLocked"].Value<bool>() == false)
                        tmp++;
                    if (parent["ZRotationLocked"].Value<bool>() == false)
                        tmp++;
                    // 1 always for strength
                    tmp++;
                    parent = parent["parent"];
                }
                
                
                
                return tmp;
            }).Sum();
            
            return new []{size};
        }
        
    }
}