using System.Collections.Generic;
using Pathfinding.Agents;
using Pathfinding.Data;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding.Grid
{
    [DefaultExecutionOrder(-1)]
    public class WorldGridBuilder : MonoBehaviour
    {
        #if UNITY_EDITOR
        [Header("Gizmos")] 
        public bool draw;
        public bool drawCoords;
        [SerializeField] private  Color mainColor;
        [SerializeField] private  Color blockedColor;
        [SerializeField] private  Color highlightedColor;
        [SerializeField] private  LayerMask obstaclesMask;
        [SerializeField] private  float gizmosY;
        #endif
        [SerializeField] private Transform root;
        [Header("GridSettings")] 
        [SerializeField] private  WorldGridSettings settings;
        [SerializeField] private  WorldGrid grid;

        private void Awake()
        {
            grid.Init();
            SetForListeners();
        }
        
        

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!draw)
                return;
            Handles.color = Color.black;
            var style = GUI.skin.GetStyle("Label");
            style.fontSize = 10;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            var size = settings.size;
            var sizeVec = new Vector3(size * 0.9f, 0.05f, size * 0.9f);
            foreach (var point in grid.GridPoints)
            {
                if(point.IsWalkable)
                    Gizmos.color = mainColor;
                else
                    Gizmos.color = blockedColor;

                var pos = GetPosition(point.X, point.Y, size);  
                if(drawCoords)
                    Handles.Label(pos, $"{point.X}, {point.Y}", style);
                Gizmos.DrawCube(pos, sizeVec);
            }
            Gizmos.color = Color.white;

            // Gizmos.color = Color.green;
            // foreach (var point in _debugPoints)
            // {
            //     var pos = GetPosition(point.x, point.y, size);
            //     Gizmos.DrawCube(pos + Vector3.up * .05f, sizeVec);
            // }
            // Gizmos.color = Color.white;

            Gizmos.color = highlightedColor;
            foreach (var point in _occupiedPoints)
            {
                var pos = GetPosition(point.x, point.y, size);
                Gizmos.DrawCube(pos, sizeVec);
            }
            Gizmos.color = Color.white;
        }
        
        private void OnValidate()
        {
            if (Application.isPlaying == false)
            {
                ClearCheckedPoints();
                ClearOccupiedPoints();
            }
        }
#endif
        

        public Vector3 GetPosition(int x, int y, float size)
        {
            var xOffset = grid.LengthX / 2 * size - size / 2;
            var yOffset = grid.LengthY / 2 * size - size / 2;
            var localPos = new Vector3(x * size - xOffset, gizmosY, y * size - yOffset);
            return root.TransformPoint(localPos);
        }

        public void Generate()
        {
            var pointsArray = new GridPoint[settings.xCount * settings.yCount];
            var size = settings.size;
            for (var x = 0; x < settings.xCount; x++)
            {
                for (var y = 0; y < settings.yCount; y++)
                {
                    var index = x * settings.yCount + y;
                    var position = GetPosition(x, y, size);
                    pointsArray[index] = new GridPoint(x,y, !CheckForObstacle(position, size), 1f);
                }
            }
            grid = new WorldGrid(settings.xCount, settings.yCount, size, GetPosition(0,0, size));
            grid.GridPoints = new List<GridPoint>(pointsArray);
            SetForListeners();
        }

        private void SetForListeners()
        {
            var listeners = GetComponentsInChildren<IGridBuilderListener>();
            foreach (var listener in listeners)
            {
                listener.SetGrid(grid);
            }
        }
        
        public bool CheckForObstacle(Vector3 position, float size)
        {
            var volume = Vector3.one * size / 2;
            volume.y = size * 4;
            var overlap = Physics.OverlapBox(position, volume, Quaternion.identity, obstaclesMask);
            return overlap.Length > 0;
        }

        public IPathfindingGrid GetGrid() => grid;


        #region Static Gizmos Debugging
#if UNITY_EDITOR
        private static List<GridCoord2> _debugPoints = new List<GridCoord2>();
        private static List<GridCoord2> _occupiedPoints = new List<GridCoord2>();
        
        public static void AddCheckedPoint(GridCoord2 point)
        {
            if(_debugPoints.Contains(point) == false)
                _debugPoints.Add(point);
        }
        
        public static void AddOccupiedPoint(GridCoord2 point)
        {
            if(_occupiedPoints.Contains(point) == false)
                _occupiedPoints.Add(point);
        }
        
        public static void RemoveOccupiedPoint(GridCoord2 point)
        {
            _occupiedPoints.Remove(point);
        }
        
        public static void ClearCheckedPoints() => _debugPoints.Clear();
        public static void ClearOccupiedPoints() => _occupiedPoints.Clear();
#endif
        #endregion

    }
}
