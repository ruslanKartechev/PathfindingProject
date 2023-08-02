using System;

namespace Pathfinding.Agents
{
    public interface IAgentsCoupleConflict : IEquatable<IAgentsCoupleConflict>
    {
        IConflictingAgent Agent1();
        IConflictingAgent Agent2();
    }
}