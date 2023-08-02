#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Pathfinding.Examples.Agents
{
    [CustomEditor(typeof(AgentSpawner))]
    public class AgentSpawnerEditor : Editor
    {
        private AgentSpawner me;

        private void OnEnable()
        {
            me = target as AgentSpawner;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button($"Rebuild grid", GUILayout.Width(100)))
                me.rectGrid.Generate();
        }
    }
}
#endif