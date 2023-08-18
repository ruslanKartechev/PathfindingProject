using System.Collections.Generic;

namespace Pathfinding.Agents
{
    public static class AgentsContainer
    {
        public static List<IPathfindingAgent> Agents = new List<IPathfindingAgent>();

        public static void Clear()
        {
            Agents.Clear();
        }

        public static void Add(IPathfindingAgent agent)
        {
            Agents.Add(agent);
        }

        public static void Remove(IPathfindingAgent agent)
        {
            Agents.Remove(agent);
        }
    }
}