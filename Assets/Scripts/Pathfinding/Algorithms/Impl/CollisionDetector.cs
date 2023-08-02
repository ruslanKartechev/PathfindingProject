using System;
using System.Collections.Generic;
using Pathfinding.Agents;

namespace Pathfinding.Algorithms.Impl
{
    public static class CollisionDetector
    {
        
        public static IList<IPathfindingAgent> GetOverlapAgents(IPathfindingAgent agent)
        {
            var results = new List<IPathfindingAgent>();
            var myPos = agent.GetNextPosition();
            var myRad = agent.GetRadius();
            foreach (var other in PathfindingAgentsContainer.Agents)
            {
                if (other == agent)
                    continue;
                var otherPos = other.GetNextPosition();
                var distance = Math.Sqrt(Math.Pow((myPos.x - otherPos.x), 2)
                                         + Math.Pow((myPos.z - otherPos.z), 2));
                if (distance <= myRad + other.GetRadius())
                {
                    results.Add(other);
                }
            }
            return results;
        }
        
        public static IPathfindingAgent GetOverlapAgent(IPathfindingAgent agent)
        {
            var myPos = agent.GetNextPosition();
            var myRad = agent.GetRadius();
            foreach (var other in PathfindingAgentsContainer.Agents)
            {
                if (other == agent)
                    continue;
                var otherPos = other.GetNextPosition();
                var distance = Math.Sqrt(Math.Pow((myPos.x - otherPos.x), 2)
                                         + Math.Pow((myPos.z - otherPos.z), 2));
                if (distance <= myRad + other.GetRadius())
                    return other;
            }
            return null;
        }
    }
}