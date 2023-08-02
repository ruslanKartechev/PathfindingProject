using Pathfinding.Data;

namespace Pathfinding.Algorithms
{
    public interface IHeuristicFunction
    {
        double GetHeuristic(GridCoord2 start, GridCoord2 end);
    }
}