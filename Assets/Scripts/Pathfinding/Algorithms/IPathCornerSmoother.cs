using System.Collections.Generic;
using Pathfinding.Algorithms.Impl;

namespace Pathfinding.Algorithms
{
    public interface IPathCornerSmoother
    {
        public IList<IPathSegment> SmoothCorners(IList<LinePathSegment> segments);
    }
}