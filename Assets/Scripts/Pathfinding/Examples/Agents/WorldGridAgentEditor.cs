#if UNITY_EDITOR
using UnityEditor;

namespace Pathfinding.Examples.Agents
{
    [CustomEditor(typeof(SimpleAnimatedAgent)), CanEditMultipleObjects]
    public class SimpleAnimatedAgentEditor : Editor
    {
        public SimpleAnimatedAgent me;

        private void OnEnable()
        {
            me = target as SimpleAnimatedAgent;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
        }
    }
}
#endif