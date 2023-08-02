using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Algorithms.Impl
{
    public class PathCornerSmoother : IPathCornerSmoother
    {
        private float _radius;
        public PathCornerSmoother(float radius)
        {
            _radius = radius;
        }

        public IList<IPathSegment> SmoothCorners(IList<LinePathSegment> segments)
        {
            var smoothSegments = new List<IPathSegment>(segments.Count * 2);
            var prev = segments[0];
            smoothSegments.Add(prev);
            var diameter = 2 * _radius;
            var count = segments.Count;
            var removeList = new List<IPathSegment>(2);
            for (var segmentIndex = 1; segmentIndex < segments.Count; segmentIndex++)
            {
                var current = segments[segmentIndex];
                prev = segments[segmentIndex - 1];
                if (current.GetLength() <= diameter && segmentIndex < count - 1)
                {
                    var next = segments[segmentIndex + 1];
                    Vector3 curveStart;
                    if (prev.GetLength() <= _radius)
                    {
                        curveStart = prev.start;
                        removeList.Add(prev);   
                    }
                    else
                        curveStart = prev.end + (prev.start - prev.end).normalized * _radius;
                    prev.end = curveStart;
                    prev.CalculateLength();
                    var curveInflection = Vector3.Lerp(current.start, current.end, 1 / 2f);
                    var curveEnd = next.start + (next.end - next.start).normalized * _radius;
                    next.start = curveEnd;
                    next.CalculateLength();
                    var curvedSegment = new BezierPathSegment(curveStart, curveEnd, curveInflection);
                    smoothSegments.Add(curvedSegment);
                    smoothSegments.Add(next);
                    segmentIndex += 1;
                }
                else
                {
                    var curveInflection = current.start;
                    var curveStart = prev.end + (prev.start - prev.end).normalized * _radius;
                    var curveEnd = current.start + (current.end - current.start).normalized * _radius;
                    if (prev.GetLength() <= _radius)
                    {
                        curveStart = prev.start;
                        removeList.Add(prev);
                    }
                    else
                    {
                        prev.end = curveStart;
                        prev.CalculateLength();
                    }
                    current.start = curveEnd;
                    current.CalculateLength();
                    var curvedSegment = new BezierPathSegment(curveStart, curveEnd, curveInflection);
                    smoothSegments.Add(curvedSegment);
                    smoothSegments.Add(current);
                }
            }
            foreach (var segment in removeList)
                smoothSegments.Remove(segment);
            removeList.Clear();
            return smoothSegments;
        }
    }
}