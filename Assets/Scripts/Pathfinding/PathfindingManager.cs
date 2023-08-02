#define Measure__
using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Agents;
using Pathfinding.Algorithms.Impl;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding
{
    public partial class PathfindingManager : MonoBehaviour, IGridBuilderListener
    {
        [SerializeField] private  WorldGrid _currentGrid;
        private AgentsConflictResolver _resolver;
        private HashSet<IPathfindingAgent> _agentsRunning 
            = new HashSet<IPathfindingAgent> (10);
        private HashSet<IPathfindingAgent> _removeMovingAgentsQueue 
            = new HashSet<IPathfindingAgent>(10);
        private HashSet<IPathfindingAgent> _addMovingAgentsQueue 
            = new HashSet<IPathfindingAgent>(10);

        private const float TimeDelta = 1 / 60f;
        public static float DeltaTime => TimeDelta * Time.timeScale;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                _resolver = new AgentsConflictResolver();
            }
            else
                Destroy(this);
        }

        private void Start()
        {
            StartCoroutine(RunningAgents());
        }

        public void SetGrid(WorldGrid grid)
        {
            _currentGrid = grid;
        }

        private IEnumerator RunningAgents()
        {
            yield return null;
            while (true)
            {
#if Measure
                var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
                foreach (var agent in _removeMovingAgentsQueue)
                    _agentsRunning.Remove(agent);
                _removeMovingAgentsQueue.Clear();
                foreach (var agent in _addMovingAgentsQueue)
                {
                    if (_agentsRunning.Contains(agent) == false)
                        _agentsRunning.Add(agent);
                }
                _addMovingAgentsQueue.Clear();
                foreach (var agent in _agentsRunning)
                    agent.NextAction();
                for (var i = 0; i < 1; i++)
                {
                    foreach (var agent in _agentsRunning)
                        agent.CorrectPosition();     
                }
                foreach (var agent in _agentsRunning)
                    agent.ApplyPosition();
#if Measure
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Debug.Log($"[Manager] time: {elapsedMs:N12} ms");
#endif
                yield return null;
            }
        }

    }
}