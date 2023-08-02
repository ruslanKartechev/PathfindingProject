using System.Collections.Generic;

namespace Pathfinding
{
    public static class Extensions
    {
        public static T GetRandom<T>(this IList<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count - 1)];
        }
    }
}