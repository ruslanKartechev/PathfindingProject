using UnityEngine;

namespace Pathfinding.Algorithms
{
    public interface IPathSegment
    {
        public Vector3 GetPosition(double t);
        public double beginT { get; set; }
        public double endT { get; set; }
        public double lengthT { get; set; }
        
        public float GetLength();
        public void DrawSegment(Color color);
    }
}