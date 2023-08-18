#define DoDebug
using System.Collections.Generic;
using Pathfinding.Data;
using UnityEngine;

namespace Pathfinding.Grid
{
    [System.Serializable]
    public class WorldGrid : IPathfindingGrid
    {
        [SerializeField] private int _length_x;
        [SerializeField] private int _length_y;

        public int LengthX => _length_x;
        public int LengthY => _length_y;
        public List<GridPoint> GridPoints;

        public float cellSize;
        public Vector3 firstNodeWorldPosition;
        
        public IList<GridPoint> Points => GridPoints;
        private HashSet<GridCoord2> _busyPoints;

        public WorldGrid(int lengthX, int lengthY, 
            float size, Vector3 firstNodeWorldPosition)
        {
            _length_x = lengthX;
            _length_y = lengthY;
            cellSize = size;
            GridPoints = new List<GridPoint>(lengthX *lengthY);
            this.firstNodeWorldPosition = firstNodeWorldPosition;
            _busyPoints = new HashSet<GridCoord2>();
        }

        public void Init()
        {
            _busyPoints = new HashSet<GridCoord2>();
        }
        
        public void CorrectXIndex(int input, out int index)
        {
            index = Mathf.Clamp(input, 0, LengthX - 1);
        }
        
        public void CorrectYIndex(int input, out int index)
        {
            index = Mathf.Clamp(input, 0, LengthY - 1);
        }
        public bool GetWalkable(GridCoord2 coordinate)
        {
            // Debug.Log($"Index: {coordinate.x * LengthY + coordinate.y}, count: {GridPoints.Count}");
            return GridPoints[coordinate.x * LengthY + coordinate.y].IsWalkable;
        }

        /// <summary>
        /// </summary>
        /// <param name="start">Start tile</param>
        /// <param name="end">End tile</param>
        /// <param name="t"> Lerp value between start and end </param>
        /// <returns></returns>
        public bool GetWalkableBetween(GridCoord2 start, GridCoord2 end, float t)
        {
             // if (start.x == end.x || start.y == end.y)
                 // return true;
             var startWorld = GetWorldPosition(start);
             var endWorld = GetWorldPosition(end);
             var midPos = Vector3.Lerp(startWorld, endWorld, t);
             var walkable = GetWalkable(GetGridCoordinate(midPos));
             return walkable;
        }
        
        public Vector3 GetWorldPosition(GridCoord2 coordinate)
        {
            var size = cellSize;
            var relativePos = new Vector3(coordinate.x * size, 0, coordinate.y * size);
            return relativePos + firstNodeWorldPosition;
        }
            
        public GridCoord2 GetGridCoordinate(Vector3 worldPosition)
        {
            var size = cellSize;
            var relativePos = worldPosition - (firstNodeWorldPosition - new Vector3(size / 2, 0, size / 2));
            var x = Mathf.CeilToInt(relativePos.x / size) - 1;
            var y = Mathf.CeilToInt(relativePos.z / size) - 1;
            return new GridCoord2(x, y);
        }

        public ICollection<GridCoord2> GetBusyCoords()
        {
            return _busyPoints;
        }

        public GridCoord2 GetClosestFreeCoord(GridCoord2 coordinate)
        {
            foreach (var direction in Neighbours1)
            {
                var coord = coordinate + direction;
                if((coord.x < 0 || coord.y < 0)
                   || (coord.x >= _length_x || coord.y >= _length_y))
                    continue;
                if (GetWalkable(coord))
                    return coord;
            }
            return coordinate;
        }

        public void SetBusy(GridCoord2 coordinate)
        {
            if (_busyPoints.Contains(coordinate) == false)
                _busyPoints.Add(coordinate);
            #if DoDebug
            WorldGridBuilder.AddOccupiedPoint(coordinate);
            #endif
        }

        public void FreeCoord(GridCoord2 coordinate)
        {
            _busyPoints.Remove(coordinate);
            #if DoDebug
            WorldGridBuilder.RemoveOccupiedPoint(coordinate);
            #endif
        }

        public bool CheckBusy(GridCoord2 coordinate)
        {
            return _busyPoints.Contains(coordinate);
        }
        
        public bool CheckWalkableAndFree(GridCoord2 coordinate)
        {
            return !CheckBusy(coordinate) && GetWalkable(coordinate);
        }

        public void SetWalkable(int x, int y, bool walkable)
        {
            var prev = GridPoints[x * LengthY + y];
            GridPoints[x * LengthY + y] = new GridPoint(x, y, walkable, prev.Weight);
        }


        public static readonly GridCoord2[] Neighbours1 = {
            (new GridCoord2(1, 0)),
            (new GridCoord2(0, 1)),
            (new GridCoord2(-1, 0)),
            (new GridCoord2(0, -1)),
            (new GridCoord2(1, 1)),
            (new GridCoord2(1, -1)),
            (new GridCoord2(-1, 1)),
            (new GridCoord2(-1, -1))
        };
    }
}