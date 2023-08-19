#define DoDebug__
using System;
using System.Collections.Generic;
using Pathfinding.Algorithms;
using Pathfinding.Algorithms.Impl;
using Pathfinding.Grid;
using UnityEngine;
using Pathfinding.Data;

namespace Pathfinding.Agents
{
    public class MovementCalculator
    {
        public bool DoLog;
        private IList<IPathSegment> _segments;
        private float _t;
        private int _currentIndex;
        private float _totalDistance;

        #region Creating Path

        public MovementCalculator(Path path, Vector3 startPos, Vector3 endWorldPos, IPathfindingGrid grid,
            IPathCornerSmoother smoother = null, bool doLog = false)
        {
            DoLog = doLog;
            _segments = new List<IPathSegment>();
            var lineSegments = new List<LinePathSegment>();
            var count = path.Points.Count;
            LinePathSegment lineSegment = null;
            // Building from End to Start, because Path is reversed
            GridCoord2 start, end = new GridCoord2(-1,-1);
            for (var i = count - 1; i >= 1; i--)
            {
                if (i == count - 1)
                    lineSegment = new LinePathSegment(startPos, grid.GetWorldPosition(path.Points[i - 1]));
                else if (i == 1)
                    lineSegment = new LinePathSegment(grid.GetWorldPosition(path.Points[1]), endWorldPos);
                else
                {
                    lineSegment = new LinePathSegment(grid.GetWorldPosition(path.Points[i]),
                        grid.GetWorldPosition(path.Points[i - 1]));
                }
                lineSegments.Add(lineSegment);
                #if UNITY_EDITOR && DoDebug
                Debug.DrawLine(lineSegment.start + Vector3.up * 0.25f, 
                    lineSegment.end + Vector3.up * 0.25f, 
                    Color.black, 2f);
                #endif
            }
            // NoSmoothing(lineSegments);
            // return;
            if (smoother != null)
                Smooth(lineSegments, smoother);
            else
                NoSmoothing(lineSegments);
        }

        private void NoSmoothing(List<LinePathSegment> lineSegments)
        {
            _segments = new List<IPathSegment>(lineSegments.Count);
            var totalLength = 0f;
            var passedDistance = 0f;

            foreach (var segment in lineSegments)
            {
                _segments.Add(segment);
                totalLength += segment.GetLength();
            }
            for (var i = 0; i < _segments.Count; i++)
            {
                var start = passedDistance / totalLength;
                var length = _segments[i].GetLength() / totalLength;
                var end = start + length;
                _segments[i].beginT = start;
                _segments[i].lengthT = length;
                _segments[i].endT = end;
                passedDistance += _segments[i].GetLength();
#if UNITY_EDITOR && DoDebug
                 if(DoLog)
                     _segments[i].DrawSegment(Color.white);
#endif
            }
            _totalDistance = totalLength;
        }

        private void Smooth(IList<LinePathSegment> lineSegments, IPathCornerSmoother smoother)
        {
            if (lineSegments.Count >= 2)
            {
                _segments = smoother.SmoothCorners(lineSegments);
                var totalLength = 0f;
                foreach (var segment in _segments)
                    totalLength += segment.GetLength();
                var passedDistance = 0f;
                for (var i = 0; i < _segments.Count; i++)
                {
                    var startT = passedDistance / totalLength;
                    var lengthT = _segments[i].GetLength() / totalLength;
                    var end = startT + lengthT;
                    _segments[i].beginT = startT;
                    _segments[i].lengthT = lengthT;
                    _segments[i].endT = (i != _segments.Count-1) ? end : 1f;
                    passedDistance += _segments[i].GetLength();
#if UNITY_EDITOR && DoDebug
                    if (true)
                    {
                        if (_segments[i] is LinePathSegment)
                            _segments[i].DrawSegment(Color.white);
                        else
                            _segments[i].DrawSegment(Color.blue);               
                    }
#endif
                }
                _totalDistance = totalLength;
            }
            else
            {
                lineSegments[0].lengthT = 1f;
                lineSegments[0].endT = 1f;
                _totalDistance = lineSegments[0].GetLength();
                _segments = new List<IPathSegment>() { lineSegments[0] };
#if UNITY_EDITOR && DoDebug
                if (DoLog)
                  lineSegments[0].DrawSegment(Color.white);
#endif
            }
        }
        #endregion

        public float GetTotalLength() => _totalDistance;

        public double GetPercent(Vector3 position, double start = 0f, double end = 1f, int slices = 50)
        {
            return GetPercentFor(_segments.Count + 1, position, start, end, slices);
        }

        public Vector3 EvaluateAt(double percent)
        {
            percent = Math.Clamp(percent, 0d, 1d);
            for (var i = 0; i < _segments.Count; i++)
            {
                if (_segments[i].beginT <= percent && _segments[i].endT >= percent)
                    return _segments[i].GetPosition(percent);
            }
            Dbg.Red($"[MovementCalculator] Returning zero pos. Percent: {percent}");
            return _segments[0].GetPosition(0);
        }

        private void EvaluateAt(ref Vector3 pos, double percent)
        {
            var t = percent < 0 ? 0 : percent;
            percent = t > 1f ? 1f : percent;
            
            for (var i = 0; i < _segments.Count; i++)
            {
                if (_segments[i].beginT <= percent
                    && _segments[i].endT >= percent)
                    pos = _segments[i].GetPosition((float)percent);
            }
        }

        private double GetPercentFor(int iterations, Vector3 point, double start, double end, int slices)
        {
            var closestPercent = 0.0;
            var closestDistance = float.MaxValue;
            while (true)
            {
                if (iterations <= 0)
                {
                    var distToStart = (point - EvaluateAt(start)).sqrMagnitude;
                    var distToEnd = (point - EvaluateAt(end)).sqrMagnitude;
                    if (distToEnd != distToStart)
                        return distToEnd < distToStart ? end : start;
                    return (start + end) / 2;
                }
                var tick = (end - start) / slices;
                var tempPercent = start;
                var pos = Vector3.zero;
                while (true)
                {
                    EvaluateAt(ref pos, tempPercent);
                    var dist = (point - pos).sqrMagnitude;
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestPercent = tempPercent;
                    }
                    if (tempPercent >= end) break;
                    tempPercent = MoveValue(tempPercent, end, tick);
                }
                var newStart = closestPercent - tick;
                if (newStart < start) 
                    newStart = start;
                var newEnd = closestPercent + tick;
                if (newEnd > end) 
                    newEnd = end;
                start = newStart;
                end = newEnd;
                iterations = --iterations;
            }
        }

        private static double MoveValue(double current, double target, double amount)
        {
            if (target >= current)
            {
                current += amount;
                if (current > target) return target;
            }
            else
            {
                current -= amount;
                if (current < target) return target;
            }
            return current;
        }

        public void Draw()
        {
            if (_segments == null)
                return;
            foreach (var segment in _segments)
            {
                if (segment is LinePathSegment)
                    segment.DrawSegment(Color.magenta * 0.8f);
                else
                    segment.DrawSegment(Color.blue);
            }
        }
    }
}

