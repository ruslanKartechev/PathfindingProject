using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pathfinding.Data;
using Pathfinding.Grid;

namespace Pathfinding.Algorithms.Impl
{
    public abstract class PathfindingAlgorithm
    {
        
        protected const int MaxNeighbours = 8;
        protected readonly PathNode[] neighbours = new PathNode[MaxNeighbours];
        
        protected IPathfindingGrid _grid;

        protected readonly IBinaryHeap<GridCoord2, PathNode> openList;
        protected readonly HashSet<GridCoord2> closedList;

        protected readonly IDictionary<GridCoord2, GridCoord2> links;
        protected readonly IList<GridCoord2> pathGridPoints;

        public PathfindingAlgorithm()
        {
            var comparer = Comparer<PathNode>.Create((a, b) => b.EstimatedTotalCost.CompareTo(a.EstimatedTotalCost));
            openList = new BinaryHeapNodes(comparer);
            closedList =  new HashSet<GridCoord2>();
            links = new Dictionary<GridCoord2, GridCoord2>();
            pathGridPoints = new Collection<GridCoord2>();
        }
        
        public abstract Path FindPath(GridCoord2 from, GridCoord2 target);
        public abstract Path FindPathOnWaypoints(GridCoord2 start, IList<GridCoord2> targets, IEnumerable<GridCoord2> excluded);

        public virtual void SetGrid(IPathfindingGrid grid)
        {
            _grid = grid;
        }

        protected void Refresh()
        {
            openList.Clear();
            closedList.Clear();
            links.Clear();
        }
        

    }
}