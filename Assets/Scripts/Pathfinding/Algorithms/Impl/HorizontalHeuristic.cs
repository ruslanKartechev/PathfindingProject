using System;
using Pathfinding.Data;

namespace Pathfinding.Algorithms.Impl
{
    public class HorizontalHeuristic : IHeuristicFunction
    {
        public double GetHeuristic(GridCoord2 start, GridCoord2 end)
        {
            return 1f/3f * Math.Abs(end.x - start.x) + 2f/3f * Math.Abs(end.y - start.y);
        }
    }
}