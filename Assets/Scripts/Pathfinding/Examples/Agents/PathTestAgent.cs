using System.Collections;
using System.Collections.Generic;
using Pathfinding.Agents;
using UnityEngine;

namespace Pathfinding.Examples.Agents
{
    public class PathTestAgent : PathAgent, IPathAgentListener
    {
        [Space(10)]
        public bool autoStart;
        public Transform targetPoint;
        public AgentAnimator animator;
        public Renderer renderer;
        public List<Material> _materials;
        public bool RandomizeMaterial;
        private Coroutine _moving;
                
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(Application.isPlaying == false)
                animator = GetComponentInChildren<AgentAnimator>();
        }
#endif

        public void MoveAgentToTarget(Transform target)
        {
            InitAgent();
            SetListener(this);
            if (RandomizeMaterial)
            {
                var mat = _materials.GetRandom();
                renderer.sharedMaterial = mat;       
            }
            if (autoStart)
            {
                if (targetPoint == null)
                    targetPoint = target;
                MoveTo(targetPoint.position);
            }
        }

        public void MoveToTarget()
        {
            MoveTo(targetPoint.position);
        }

        public void OnBeganRotation()
        {
        }

        public void OnEndedRotation()
        {
        }

        public void OnBeganMovement()
        {
            if(_moving != null)
                StopCoroutine(_moving);
            // _moving = StartCoroutine(CheckingPosition());
        }

        public void OnStopped()
        {
            animator.Idle();
        }

        public void OnReachedFinalPoint()
        {
        }

        public void OnStateChanged(AgentState newState, AgentState prevState)
        {
            switch (newState)
            {
                case AgentState.Idle:
                    animator.Idle();
                    break;
                case AgentState.Running:
                    animator.Run();
                    break;
                case AgentState.Blocked:
                    animator.Idle();
                    break;
                case AgentState.PushedBack:
                    animator.Idle();
                    break;
            }
        }
        
        private IEnumerator CheckingPosition()
        {
            yield return new WaitForSeconds(0.25f);
            var oldPos = targetPoint.position;
            while (true)
            {
                if (targetPoint.position != oldPos)
                   MoveTo(targetPoint.position);    
                oldPos = targetPoint.position;
                yield return null;
            }
        }
        
        
    }
}