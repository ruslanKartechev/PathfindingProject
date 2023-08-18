using System.Collections.Generic;
using Pathfinding.Data;
using UnityEngine;

namespace Pathfinding.Grid
{
    public interface IPathfindingGrid
    {
        public int LengthX { get; }
        public int LengthY { get; }
        public IList<GridPoint> Points { get; }
        public bool GetWalkable(GridCoord2 coordinate);
        public bool GetWalkableBetween(GridCoord2 start, GridCoord2 end, float t);

        public Vector3 GetWorldPosition(GridCoord2 coordinate);
        public GridCoord2 GetGridCoordinate(Vector3 worldPosition);
        public ICollection<GridCoord2> GetBusyCoords();

        public void SetBusy(GridCoord2 coordinate);
        public void FreeCoord(GridCoord2 coordinate);
        public bool CheckBusy(GridCoord2 coordinate);
        public bool CheckWalkableAndFree(GridCoord2 coordinate);
        
    }
    
}