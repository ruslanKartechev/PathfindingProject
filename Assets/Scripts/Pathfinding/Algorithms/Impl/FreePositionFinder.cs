#define Draw__
using System.Collections.Generic;
using Pathfinding.Agents;
using Pathfinding.Data;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding.Algorithms.Impl
{
    public static class FreePositionFinder
    {
        private const int MaxOrder = 3;
        
        public static GridCoord2 GetClosestFreeAround(IPathfindingGrid grid, GridCoord2 from, GridCoord2 to)
        {
            for (var order = 1; order <= MaxOrder; order++)
            {
                var freeCoords = GetFree(grid, to, order);
                if (freeCoords.Count > 0)
                {
                    // Dbg.Green($"Order: {order} found free coords: {freeCoords.Count}");
                    return GetClosest(freeCoords, from);
                }
            }
            // Dbg.Red($"[FreePositionFinder] No free was found");
            return from;
        }
        
        public static GridCoord2 GetClosestFree(IPathfindingGrid grid, GridCoord2 from, GridCoord2 to)
        {
            if (grid.CheckWalkableAndFree(to))
                return to;
            for (var order = 1; order <= MaxOrder; order++)
            {
                var freeCoords = GetFree(grid, to, order);
                if (freeCoords.Count > 0)
                {
                    // Dbg.Green($"Order: {order} found free coords: {freeCoords.Count}");
                    return GetClosest(freeCoords, from);
                }
            }
            // Dbg.Red($"[FreePositionFinder] No free was found");
            return from;
        }

        public static Vector3 GetClosestPos(IPathfindingGrid grid, IPathfindingAgent agent, Vector3 from, Vector3 toPos, float agentRadius)
        {
            if (CollisionDetector.GetOverlapAgents(toPos, agentRadius, agent).Count == 0)
            {
                if(grid.CheckWalkableAndFree(grid.GetGridCoordinate(toPos)))
                    return toPos;
            }
            
            var distMultiplier = 2f;
            const float distMultiplierMax = 6f;
            while (distMultiplier <= distMultiplierMax)
            {
                var distance = agentRadius * distMultiplier;
                var freePositions = GetFreePositions(toPos, distance, agentRadius, grid, agent);
                if (freePositions.Count > 0)
                {
                    // Dbg.Red($"distance: {distance}");
                    var r = GetClosest(freePositions, from);
                    Debug.DrawLine(toPos + Vector3.up, r + Vector3.up, Color.blue, 10f);
                    return r;
                }
                distMultiplier += 1f;
            }
            // Dbg.Red("No result found");
            return toPos;
        }

        private static Vector3 GetClosest(ICollection<Vector3> positions, Vector3 start)
        {
            var minDistance2 = float.MaxValue;
            var result = start;
            foreach (var pos in positions)
            {
                var d2 = (pos - start).sqrMagnitude;
                if (d2 < minDistance2)
                {
                    minDistance2 = d2;
                    result = pos;
                }   
            }
            return result;
        }
        
        private static IList<Vector3> GetFreePositions(Vector3 toPos, float distance, float radius, 
            IPathfindingGrid grid, IPathfindingAgent agent)
        {
            var angle = 0f;
            const float angleStep = 45f;
            const float angleMax = 360f - angleStep;
            Vector3 direction;
            var results = new List<Vector3>();
            while (angle <= angleMax)
            {
                var oldAngle = angle;
                angle += angleStep;
                direction = Quaternion.Euler(0f, oldAngle, 0f) * Vector3.forward;
                var position = toPos + direction * distance;
                if (CollisionDetector.GetOverlapAgents(position, radius, agent).Count > 0) 
                    continue;
                var coordinate = grid.GetGridCoordinate(position);
                if(grid.CheckBusy(coordinate) == true)
                    continue;
                results.Add(position);
            }
            return results;
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
#if Draw
            var centerPos = grid.GetWorldPosition(center) + Vector3.up * 5;
#endif
            var freeCoords = new List<GridCoord2>(4);
            var radius = order * 2 - 1; // length without diagonal bits
            // upper hor line
            var lineX = center.x - (order - 1);
            var lineY = center.y + order;
            var lastX =  center.x + (order - 1);
            for (var x = lineX; x <= lastX; x++)
                Add(new GridCoord2(x, lineY));
            // bottom hor line
            lineY = center.y - order;
            for (var x = lineX; x <= lastX; x++)
                Add(new GridCoord2(x, lineY));
            // left vert line
            lineX = center.x - order;
            lineY = center.y - (order - 1);
            var lastY = center.y + radius / 2;
            for (var y = lineY; y <= lastY; y++)
                Add(new GridCoord2(lineX, y));
            
            // right vert line
            lineX = center.x + order;
            for (var y = lineY; y <= lastY; y++)
                Add(new GridCoord2(lineX, y));
            // diagonals left
            for (var i = 1; i <= order; i++)
                Add(new GridCoord2(center.x + i, center.y + i));
            for (var i = 1; i <= order; i++)
                Add(new GridCoord2(center.x - i, center.y + i));
            for (var i = 1; i <= order; i++)
                Add(new GridCoord2(center.x - i, center.y - i));
            for (var i = 1; i <= order; i++)
                Add(new GridCoord2(center.x + i, center.y - i));
           
            return freeCoords;
            
            void Add(GridCoord2 pos)
            {
                var doAdd = grid.CheckWalkableAndFree(pos);
                if (doAdd)
                {
                    freeCoords.Add(pos);
                    #if Draw
                    Debug.DrawLine(centerPos, grid.GetWorldPosition(pos), Color.red, 1f);
                    #endif
                }   
            }
        }
        
        
    }
}