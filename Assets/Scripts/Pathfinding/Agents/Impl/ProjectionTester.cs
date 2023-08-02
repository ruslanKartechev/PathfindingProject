using Pathfinding.Algorithms;
using Pathfinding.Algorithms.Impl;
using Pathfinding.Data;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding.Agents
{
    public class ProjectionTester : MonoBehaviour
    {
        public Transform p1;
        public Transform p2;
        public Transform projectedPoint;
        public WorldGridBuilder gridBuilder;
        public float agentRadius = 0.5f;
        public bool doCalculate;
        [Space(10)]
        public float gizmosDuration = 1f;
        public Color gizmosColor = Color.red;
        private AStart _pathfinding;
        private MovementCalculator _movementCalculator;
        private IPathCornerSmoother _smoother;
        private IPathfindingGrid _grid;

        private GridCoord2 StartCoord => _grid.GetGridCoordinate(p1.position);
        private GridCoord2 EndCoord => _grid.GetGridCoordinate(p2.position);
        
        private void Start()
        {
            gridBuilder = FindObjectOfType<WorldGridBuilder>();
            _grid = gridBuilder.GetGrid();
            _pathfinding = new AStart(_grid, new DistanceHeuristic());
        }
        
        public void Calculate()
        {
            var path = _pathfinding.FindPath(StartCoord, EndCoord);
            _movementCalculator = new MovementCalculator(path, 
                p1.position, p2.position, 
                _grid, new PathCornerSmoother(agentRadius));
            var percent  =_movementCalculator.GetPercent(projectedPoint.position);
            Debug.Log($"[ProjectionTest] Percent: {percent:N5}");
            var position = _movementCalculator.EvaluateAt((float)percent);
            Debug.DrawLine(position + Vector3.up * 0.5f, projectedPoint.position + Vector3.up * 0.5f, 
                gizmosColor, gizmosDuration);
        }

        private void Update()
        {
            if (projectedPoint != null
                && doCalculate)
            {
                Calculate();
                // doCalculate = false;
            }
        }
    }
    

    
}