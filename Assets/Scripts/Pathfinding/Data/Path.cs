using System.Collections.Generic;

namespace Pathfinding.Data
{
    public class Path
    {
        public IList<GridCoord2> Points;
        public bool foundPathToDestination;
        
        public Path(IList<GridCoord2> points, bool foundPathToDestination)
        {
            Points = points;
            this.foundPathToDestination = foundPathToDestination;
        }
    }
}