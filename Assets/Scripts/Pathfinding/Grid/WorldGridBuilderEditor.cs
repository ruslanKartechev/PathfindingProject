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
            Button("Generate", Color.green, me.Generate);
            Button("Draw", Color.white, () => { me.draw = true;});
            Button("No Draw", Color.white * 0.6f, () => { me.draw = false;});
            
            base.OnInspectorGUI();
            
        }

        private void Button(string name, Color color, Action action)
        {
            GUI.color = color;
            if (GUILayout.Button(name,
                    GUILayout.Width(80),
                    GUILayout.Height(30)))
            {
                action.Invoke();   
            }
            GUI.color = Color.white;
        }
    }
}
#endif