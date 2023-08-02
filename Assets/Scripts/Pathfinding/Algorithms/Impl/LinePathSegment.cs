using Pathfinding.Data;
using UnityEngine;

namespace Pathfinding.Algorithms.Impl
{
    public class LinePathSegment : IPathSegment
    {
        public Vector3 start;
        public Vector3 end;
        
        public double beginT { get; set; }
        public double lengthT { get; set; }
        public double endT { get; set; }
        public float GetLength() => _length;

        private float _length;
        
        public LinePathSegment(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
            _length = (end - start).magnitude;
        }

        public void CalculateLength()
        {
            _length = (end - start).magnitude;
        }

        public Vector3 GetPosition(double t)
        {
            // Debug.Log($"<color=yellow> Line t: {t}, Corrected: {(t - beginT) / lengthT} </color>");
            return Vector3.Lerp(start, end, (float)((t - beginT) / lengthT));
        }
            
        public void DrawSegment(Color color)
        {
            var p1 = start + Vector3.up * DebugSettings.DrawUpOffset;
            var p2 = Vector3.Lerp(start, end, 0.5f) + Vector3.up * DebugSettings.DrawUpOffset;
            var p3 = end + Vector3.up * DebugSettings.DrawUpOffset;
            Debug.DrawLine(p1, p2, color, DebugSettings.DrawPathDur);
            Debug.DrawLine(p2, p3, color * 0.35f, DebugSettings.DrawPathDur);
            
            // Debug.DrawRay(start, Vector3.up, Color.yellow, 2f);
            // Debug.DrawRay(end, Vector3.up, Color.yellow, 2f);
            
        }
    }
}