using System.Collections;
using Pathfinding.Agents;
using Pathfinding.Grid;

namespace Pathfinding
{
    public partial class PathfindingManager
    {
        private static PathfindingManager _instance;
        public static IPathfindingGrid Grid => _instance._currentGrid;

        public static void AddAgent(IPathfindingAgent agent)
        {
            PathfindingAgentsContainer.Add(agent);
        }
        
        public static void RemoveAgent(IPathfindingAgent agent)
        {
            PathfindingAgentsContainer.Remove(agent);
        }


        public static void AddMovingAgent(IPathfindingAgent agent)
        {
            _instance._addMovingAgentsQueue.Add(agent);
        }
        
        public static void RemoveMovingAgent(IPathfindingAgent agent)
        {
            _instance._removeMovingAgentsQueue.Add(agent);
        }
        
        
    }
}