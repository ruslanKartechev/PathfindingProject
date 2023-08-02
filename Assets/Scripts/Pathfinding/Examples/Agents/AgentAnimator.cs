using UnityEngine;

namespace Pathfinding.Examples.Agents
{
    public class AgentAnimator : MonoBehaviour
    {
        public Animator animator;
        private bool _running;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (animator != null)
                animator = gameObject.GetComponent<Animator>();
        }
        #endif

        public void Idle()
        {
            _running = false;
            animator.CrossFade("Idle",0.05f,0);
        }

        public void Run()
        {
            if (_running)
                return;
            _running = true;
            animator.CrossFade("Run",0.05f,0);
        }
    }
}