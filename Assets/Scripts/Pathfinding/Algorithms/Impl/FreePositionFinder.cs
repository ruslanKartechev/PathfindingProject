using System.Collections.Generic;
using Pathfinding.Data;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding.Algorithms.Impl
{
    public class FreePositionFinder
    {
        public static GridCoord2 GetClosestFree(IPathfindingGrid grid, GridCoord2 from, GridCoord2 to)
        {
            if (grid.CheckWalkableAndFree(to))
                return to;
            var maxOrder = 3;
            for (var order = 1; order <= maxOrder; order++)
            {
                var freeCoords = GetFree(grid, to, order);
                if (freeCoords.Count > 0)
                {
                    // Dbg.Green($"Order: {order} found free coords: {freeCoords.Count}");
                    return GetClosest(freeCoords, from);
                }
            }
            Dbg.Red($"[FreePositionFinder] No free was found");
            return from;
        }

        private static GridCoord2 GetClosest(IList<GridCoord2> coords, GridCoord2 center)
        {
            var shortest = float.MaxValue;
            var result = coords[0];
            foreach (var coord in coords)
            {
                var d2 = (center - coord).DistanceEstimate();
                if (d2 < shortest)
                {
                    shortest = d2;
                    result = coord;
                }
            }
            return result;
        }

        private static IList<GridCoord2> GetFree(IPathfindingGrid grid, GridCoord2 center, int order = 1)
        {
            var freeCoords = new List<GridCoord2>(4);
            var radius = order * 2 - 1; // length without diagonal bits
            // upper hor line
            var lineX = center.x - (order - 1);
            var lineY = center.y + order;
            var lastX =  center.x + (order - 1);
            var color = Color.red;
            var centerPos = grid.GetWorldPosition(center) + Vector3.up * 5;
            for (var x = lineX; x <= lastX; x++)
            {
                CheckAdd(new GridCoord2(x, lineY));
            }
            // bottom hor line
            lineY = center.y - order;
            for (var x = lineX; x <= lastX; x++)
            {
                CheckAdd(new GridCoord2(x, lineY));
            }
            
            // left vert line
            lineX = center.x - order;
            lineY = center.y - (order - 1);
            var lastY = center.y + radius / 2;
            for (var y = lineY; y <= lastY; y++)
                CheckAdd(new GridCoord2(lineX, y));
            
            // right vert line
            lineX = center.x + order;
            for (var y = lineY; y <= lastY; y++)
                CheckAdd(new GridCoord2(lineX, y));
            
            // diagonals left
            for (var i = 1; i <= order; i++)
                CheckAdd(new GridCoord2(center.x + i, center.y + i));
            for (var i = 1; i <= order; i++)
                CheckAdd(new GridCoord2(center.x - i, center.y + i));
            for (var i = 1; i <= order; i++)
                CheckAdd(new GridCoord2(center.x - i, center.y - i));
            for (var i = 1; i <= order; i++)
                CheckAdd(new GridCoord2(center.x + i, center.y - i));
            // // // 
            return freeCoords;
            
            void CheckAdd(GridCoord2 pos)
            {
                if (grid.CheckWalkableAndFree(pos))
                {
                    // Dbg.Green($"Free and walkable: {pos}");
                    freeCoords.Add(pos);
                    Debug.DrawLine(centerPos, grid.GetWorldPosition(pos), color, 1f);
                }   
            }
        }
        
    }
}