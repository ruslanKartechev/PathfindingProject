#if UNITY_EDITOR

using Pathfinding.Agents;
using UnityEditor;
using UnityEngine;

namespace Pathfinding.Examples.Agents
{
    [CustomEditor(typeof(PathTestAgent)), CanEditMultipleObjects]
    public class WorldGridAgentEditor : Editor
    {
        public PathTestAgent me;

        private void OnEnable()
        {
            me = target as PathTestAgent;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button($"Move", GUILayout.Width(90)))
                me.MoveToTarget();
        }
    }
}
#endif