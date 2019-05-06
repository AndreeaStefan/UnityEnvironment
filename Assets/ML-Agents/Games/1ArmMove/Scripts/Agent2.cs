using MLAgents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArmMove
{
    public class Agent2 : Agent
    {
        public GameObject Floor;
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

        private float _direction;
        private float _rotation;

        private int decisionCounter;

        public dynamic limbsConfig;
    }
}
