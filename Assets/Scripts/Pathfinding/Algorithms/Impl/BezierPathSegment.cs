using Pathfinding.Data;
using UnityEngine;

namespace Pathfinding.Algorithms.Impl
{
    public class BezierPathSegment : IPathSegment
    {
        private const float LengthSamplesCount = 10;

        public Vector3 start;
        public Vector3 end;
        public Vector3 inflection;
        
        public double beginT { get; set; }
        public double endT { get; set; }
        public double lengthT { get; set; }
        
        private float _length;
        public float GetLength() => _length;

        public void DrawSegment(Color color)
        {
            for (var i = 1; i <= LengthSamplesCount; i++)
            {
                var p1 = BezierCurve.CalculatePosition(start, inflection, end, ((i - 1) / LengthSamplesCount));
                var p2 = BezierCurve.CalculatePosition(start, inflection, end, (i / LengthSamplesCount));
                Debug.DrawLine(p1 + Vector3.up * DebugSettings.DrawUpOffset, p2 + Vector3.up * DebugSettings.DrawUpOffset, color, DebugSettings.DrawPathDur);
            }
        }

        public void CalculateLength()
        {
            // sample based
            var length = 0f;
            for (var i = 1; i <= LengthSamplesCount; i++)
            {
                var p1 = BezierCurve.CalculatePosition(start, inflection, end, ((i - 1) / LengthSamplesCount));
                var p2 = BezierCurve.CalculatePosition(start, inflection, end, (i / LengthSamplesCount));
                length += (p2 - p1).magnitude;
            }
            _length = length;
        }

        public BezierPathSegment(Vector3 start, Vector3 end, Vector3 inflection)
        {
            this.start = start;
            this.end = end;
            this.inflection = inflection;
            CalculateLength();
        }

        public Vector3 GetPosition(double t)
        {
            // Debug.Log($"<color=green> Bezier t: {t}, Corrected: {(t - beginT) / lengthT} </color>");
            return BezierCurve.CalculatePosition(start, inflection, end, (float)((t - beginT) / lengthT));
        }   
    }
}