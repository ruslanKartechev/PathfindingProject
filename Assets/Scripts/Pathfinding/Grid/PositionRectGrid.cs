using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Grid
{
    [System.Serializable]
    public class PositionRectGrid
    {
        public int x_count;
        public int y_count;
        public float x_spacing;
        public float y_spacing;
        public Transform root;
        public List<Vector3> worldPositions;
        
        public void Generate()
        {
            worldPositions = new List<Vector3>(x_count * y_count);
            var xOffset = x_count / 2 * x_spacing;
            xOffset -= x_spacing / 2f;
            var yOffset = y_count / 2 * y_spacing;
            yOffset -= y_spacing / 2f;
            for (var x = 0; x < x_count; x++)
            {
                for (var y = 0; y < y_count; y++)
                {
                    var posX =  x * x_spacing - xOffset;
                    var posZ =  y * y_spacing - yOffset;
                    // var instance = PrefabUtility.InstantiatePrefab(prefab, root) as GameObject;
                    // instance.name = $"G {x} {y}";
                    // instance.transform.localPosition = new Vector3(posX, posY, 0);
                    var localPos = new Vector3(posX, 0f, posZ);
                    var worldPos = root.TransformPoint(localPos);
                    worldPositions.Add(worldPos);
                }                
            }
        }

        #if UNITY_EDITOR
        [Space(10)]
        public float gizmoRad;
        public Color gizmoColor;
        
        public void Draw()
        {
            if (worldPositions == null)
                return;
            Gizmos.color = gizmoColor;
            foreach (var position in worldPositions)
            {
                Gizmos.DrawSphere(position, gizmoRad);
            }
            Gizmos.color = Color.white;
        }
        #endif
    }
}