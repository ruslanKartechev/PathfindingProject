using System;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding.Agents
{
    [DefaultExecutionOrder(100)]
    public class PathfindingAgentsManager : MonoBehaviour, IGridBuilderListener
    {
        [SerializeField] private WorldGrid _grid;

        public void SetGrid(WorldGrid grid)
        {
            _grid = grid;
        }
        
        private void Start()
        {
            var agentsList = PathfindingAgentsContainer.Agents;
        }
    }
}