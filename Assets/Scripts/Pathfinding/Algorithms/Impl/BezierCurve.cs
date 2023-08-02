using UnityEngine;

namespace Pathfinding.Algorithms.Impl
{
    public static class BezierCurve
    {
        public static Vector3 CalculatePosition(Vector3 start, Vector3 inflection, Vector3 end, float t)
        {
            return start * ((1 - t) * (1 - t)) 
                   + inflection * (2 * t * (1 - t))
                   + end * (t * t);
        }
        
    }
}