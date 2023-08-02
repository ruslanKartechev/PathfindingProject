using System.Collections;
using Pathfinding.Algorithms.Impl;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding
{
    public class PosFinderTest : MonoBehaviour
    {
        public Transform point;
        private IPathfindingGrid _grid;
        private void Awake()
        {
            _grid = FindObjectOfType<WorldGridBuilder>().GetGrid();
            
        }

        [ContextMenu("Test")]
        public void Test()
        {
            var startPos = _grid.GetGridCoordinate(transform.position);
            var endGridPos = _grid.GetGridCoordinate(point.position);
            _grid.SetBusy(endGridPos);
            var gridPos = FreePositionFinder.GetClosestFree(_grid, startPos, endGridPos);
            Dbg.Green($"free pos: {gridPos}");
            var startGizmo = transform.position + Vector3.up * 0.5f;
            var endGizmo = _grid.GetWorldPosition(gridPos) + Vector3.up * 0.5f;
            Debug.DrawLine(startGizmo, endGizmo, Color.cyan, 5f);
        }
        

       
    }
}