using Pathfinding.Data;

namespace Pathfinding.Algorithms.Impl
{
    public class DistanceHeuristic : IHeuristicFunction
    {
        public double GetHeuristic(GridCoord2 start, GridCoord2 end)
        {
            return (end - start).DistanceEstimate();
        }
    }
}