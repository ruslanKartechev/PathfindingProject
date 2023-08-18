using System.Collections;
using System.Collections.Generic;
using Pathfinding.Agents;
using UnityEngine;

namespace Pathfinding.Examples.Agents
{
    public class SimpleAnimatedAgent : PathAgent, IPathAgentListener
    {
        [Space(10)]
        public bool autoStart;
        [SerializeField] private  Transform targetPoint;
        [SerializeField] private  AgentAnimator animator;
        [SerializeField] private  Renderer renderer;
        [SerializeField] private  List<Material> _materials;
        [SerializeField] private bool _randomizeMaterial;
        private Coroutine _moving;
        private List<PathRecalculateCondition> _recalculateConditions = new List<PathRecalculateCondition>();

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
            if (_randomizeMaterial)
                renderer.sharedMaterial = _materials.GetRandom();       
            if (autoStart)
            {
                if (targetPoint == null)
                    targetPoint = target;
                /// /// / /// / / //!!!!!!!!!!!!!!!!!! 
                MoveTo(targetPoint.position, false);
            }
        }

        public void OnBeganRotation()
        {
        }

        public void OnEndedRotation()
        {
        }

        public void OnBeganMovement()
        {
            // animator.Run();
            AddRecalcConditions();
            if(_moving != null)
                StopCoroutine(_moving);
            _moving = StartCoroutine(CheckingPosition());
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
        
        private void AddRecalcConditions()
        {
            _recalculateConditions.Clear();
            _recalculateConditions = new List<PathRecalculateCondition>(4);
            // for (var i = .25f; i <= .75f; i += .25f)
                // _recalculateConditions.Add(new PathRecalculateCondition(i));   
            _recalculateConditions.Add(new PathRecalculateCondition(0.5f));
        }

        private bool CheckConditions(double percent)
        {
            foreach (var condition in _recalculateConditions)
            {
                if (condition.Check(percent))
                    return true;
            }
            return false;
        }
        
        private IEnumerator CheckingPosition()
        {
            yield return null;
            var oldTargetPos = targetPoint.position;
            while (true)
            {
                if (targetPoint.position != oldTargetPos)
                   MoveTo(targetPoint.position);    
                oldTargetPos = targetPoint.position;
                if (CheckConditions(Percent))
                {
                    // MoveTo(targetPoint.position, true);
                    // Dbg.Blue($"Recalculated");
                }
                yield return null;
            }
        }
        
        
        private class PathRecalculateCondition
        {
            private bool _activated;
            private double _percent;
            public PathRecalculateCondition(double percent)
            {
                _percent = percent;
            }
            
            public bool Check(double percent)
            {
                if (percent >= _percent)
                {
                    if (!_activated)
                    {
                        _activated = true;
                        return true;
                    }
                }
                return false;
            }
        }
        
    }
}