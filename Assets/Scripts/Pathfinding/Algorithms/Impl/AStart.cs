#define DoDebug
using System.Collections.Generic;
using Pathfinding.Data;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding.Algorithms.Impl
{
    public class AStart : PathfindingAlgorithm
    {
        private const int IterationsMaxCount = 300;
        private GridCoord2 _currentEndPoint;
        private IHeuristicFunction _heuristicFunction;
        private HashSet<GridCoord2> _occupied;
        private ISet<GridCoord2> _excludedPos;

        public AStart(IPathfindingGrid grid, IHeuristicFunction heuristicFunction = null)
        {
            _grid = grid;
            if (heuristicFunction == null)
                _heuristicFunction = new DistanceHeuristic();
            else
                _heuristicFunction = heuristicFunction;
            _excludedPos = new HashSet<GridCoord2>();
        }
        
        public override Path FindPath(GridCoord2 start, GridCoord2 target)
        {
            Refresh();
            pathGridPoints.Clear();
            var foundPathToDestination = GeneratePath(start, target);
            pathGridPoints.Add(_currentEndPoint);
            while (links.TryGetValue(_currentEndPoint, out _currentEndPoint))
                pathGridPoints.Add(_currentEndPoint);
            RemoveExtraPoints(pathGridPoints, _grid);
            return new Path(pathGridPoints, foundPathToDestination);
        }

        public Path FindPath(GridCoord2 start, GridCoord2 target, IList<GridCoord2> excluded)
        {
            Refresh();
            pathGridPoints.Clear();
            foreach (var pos in excluded)
                closedList.Add(pos);
            var foundPathToDestination = GeneratePath(start, target);
            pathGridPoints.Add(_currentEndPoint);
            while (links.TryGetValue(_currentEndPoint, out _currentEndPoint))
                pathGridPoints.Add(_currentEndPoint);
            RemoveExtraPoints(pathGridPoints, _grid);
            return new Path(pathGridPoints, foundPathToDestination);
        }
        
        public override Path FindPathOnWaypoints(GridCoord2 start, IList<GridCoord2> targets, IEnumerable<GridCoord2> excluded)
        {
            Refresh();
            pathGridPoints.Clear();
            var foundPathToTarget = true;
            var count = targets.Count;
            var pathFragments = new List<List<GridCoord2>>(count);
            for (var i = 0; i < count; i++)
            {
                var ithDest = targets[i];
                var pathFragment = new List<GridCoord2>();
                var found = GeneratePath(start, ithDest);
                closedList.Clear();
                pathFragment.Add(_currentEndPoint);
                while (links.TryGetValue(_currentEndPoint, out _currentEndPoint))
                {
                    pathFragment.Add(_currentEndPoint);
                    closedList.Add(_currentEndPoint);
                }
                if (i != count - 1)
                    pathFragment.Remove(ithDest);
                if (!found)
                {
                    foundPathToTarget = false;
                    break;
                }
                start = ithDest;
                openList.Clear();
                links.Clear();
                RemoveExtraPoints(pathFragment, _grid);
                pathFragments.Add(pathFragment);

            }
            for (var i = pathFragments.Count - 1; i >= 0; i--)
            {
                foreach (var coord in pathFragments[i])
                    pathGridPoints.Add(coord);
            }
            return new Path(pathGridPoints, foundPathToTarget);
        }
        
        
        private bool GeneratePath(GridCoord2 start, GridCoord2 target)
        {
            var currentNode = new PathNode(start, 0, _heuristicFunction.GetHeuristic(start, target));
            openList.Enqueue(currentNode);
            var steps = 0;
            while (openList.Count > 0 && steps < IterationsMaxCount)
            {
                currentNode = openList.Dequeue();
                closedList.Add(currentNode.Position);
                _currentEndPoint = currentNode.Position;
                if (_currentEndPoint == target)
                    return true;
                AddToOpenList(currentNode, target);
                steps++;
            }
            return false;
        }
        
        private void AddToOpenList(PathNode parent, GridCoord2 target)
        {
            NeighbourFiller.Fill(neighbours, parent, target, _heuristicFunction);
            foreach (var nextNode in neighbours)
            {
                if (_grid.GetWalkable(nextNode.Position) == false)
                {
                    closedList.Add(nextNode.Position);
                    continue;
                }
                if (closedList.Contains(nextNode.Position)) 
                    continue;
                if (openList.TryGet(nextNode.Position, 
                        out var existingNode) == false)
                {
                    // Node is not on the open list.
                    openList.Enqueue(nextNode);
                    links[nextNode.Position] = parent.Position; // Add link to dictionary.
                }
                else if (nextNode.EstimatedTotalCost < existingNode.EstimatedTotalCost)
                {
                    // If already on the list, check if current estimate is less than previous
                    openList.Modify(nextNode);
                    links[nextNode.Position] = parent.Position; // Add link to dictionary.
                }
#if UNITY_EDITOR && DoDebug
                WorldGridBuilder.AddCheckedPoint(parent.Position);
                WorldGridBuilder.AddCheckedPoint(nextNode.Position);
#endif
            }
        }

        
        public void RemoveExtraPoints(IList<GridCoord2> path, IPathfindingGrid grid)
        {
            if(path.Count < 3)
                return;
            var startIndex = path.Count-1;
            var nextIndex = startIndex - 2;
            while (nextIndex >= 0)
            {
                if (CheckWalkable(path[startIndex], path[nextIndex]))
                {
                    path.RemoveAt(nextIndex + 1);
                    startIndex--;
                    nextIndex--;
                }
                else
                {
                    startIndex = nextIndex + 1;
                    nextIndex = startIndex - 2;
                }
            }
            
            bool CheckWalkable(GridCoord2 start, GridCoord2 end)
            {
                var distance = Mathf.Floor((end - start).DistanceEstimate());
                var step = 0.1f;
                while (step <= distance)
                {
                    var walkable = grid.GetWalkableBetween(start, end, step / distance);
                    if (!walkable)
                        return false;
                    step += 1;
                }
                return true;
            }
        }
        
        
    }
}