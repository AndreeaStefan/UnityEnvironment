using UnityEngine;

namespace Examples.A_Walker.Scripts
{
    public class GoalDetector : MonoBehaviour
    {
        [HideInInspector]
        public AWalkerAgent agent;  //

        void OnCollisionEnter(Collision col)
        {
            // Touched goal.
            if (col.gameObject.CompareTag("goal"))
            {
                agent.ReachedGoal();
            }
        }
    }
}