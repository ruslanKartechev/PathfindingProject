#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Pathfinding.Grid
{
    [CustomEditor(typeof(WorldGridBuilder))]
    public class WorldGridBuilderEditor : Editor
    {
        private WorldGridBuilder me;

        private void OnEnable()
        {
            me = target as WorldGridBuilder;
        }

        public override void OnInspectorGUI()
        {
            var sizeX = 120;
            var sizeY = 140;
            var offsetX = 20;
            var rect1 = new Rect(new Vector2(offsetX, 0), new Vector2(sizeX, sizeY));
            var rect2 = new Rect(new Vector2(sizeX + offsetX, 0), new Vector2(sizeX, sizeY));
            
            GUILayout.BeginArea(rect1);
            Button("Generate", Color.green, me.Generate);
            Button("Draw", Color.white, () => { me.draw = true;});
            Button("No Draw", Color.white * 0.75f, () => { me.draw = false;});
            GUILayout.EndArea();
            
            GUILayout.BeginArea(rect2);
            Button("Clear Checked", Color.white, WorldGridBuilder.ClearCheckedPoints);
            Button("Clear Busy", Color.white, WorldGridBuilder.ClearOccupiedPoints);
            // Button("No Draw", Color.white * 0.75f, () => { me.draw = false;});
            GUILayout.EndArea();
            
            GUILayout.Space(sizeY);
            base.OnInspectorGUI();
            
        }

        private void Button(string name, Color color, Action action)
        {
            GUI.color = color;
            if (GUILayout.Button(name,
                    GUILayout.Width(110),
                    GUILayout.Height(33)))
            {
                action.Invoke();   
            }
            GUI.color = Color.white;
        }
    }
}
#endif