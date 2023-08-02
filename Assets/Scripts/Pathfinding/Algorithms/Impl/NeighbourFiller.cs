using System;
using Pathfinding.Data;

namespace Pathfinding.Algorithms.Impl
{
    public static class NeighbourFiller
    {
        private static readonly (GridCoord2 position, double cost)[] CloseNeighbours = {
            (new GridCoord2(1, 0), 1),
            (new GridCoord2(0, 1), 1),
            (new GridCoord2(-1, 0), 1),
            (new GridCoord2(0, -1), 1),
            (new GridCoord2(1, 1), Math.Sqrt(2)),
            (new GridCoord2(1, -1), Math.Sqrt(2)),
            (new GridCoord2(-1, 1), Math.Sqrt(2)),
            (new GridCoord2(-1, -1), Math.Sqrt(2))
        };
             
        public static void Fill(PathNode[] buffer, PathNode parent, GridCoord2 target, IHeuristicFunction heuristicFunction)
        {
            var i = 0;
            foreach ((var relativePosition, var cost) in CloseNeighbours)
            {
                var nodePosition = relativePosition + parent.Position;
                var traverseDistance = parent.CostSoFar + cost;
                buffer[i++] = new PathNode(nodePosition, traverseDistance, heuristicFunction.GetHeuristic(nodePosition, target));
            }
        }   
    }
}