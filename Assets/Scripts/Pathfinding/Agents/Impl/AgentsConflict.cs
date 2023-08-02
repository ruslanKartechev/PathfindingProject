using System;

namespace Pathfinding.Agents
{
    public class AgentsConflict : IAgentsCoupleConflict
    {
        public IConflictingAgent Agent1() => _agent1;
        public IConflictingAgent Agent2() => _agent2;
        private IConflictingAgent _agent1;
        private IConflictingAgent _agent2;
        
        public AgentsConflict(IConflictingAgent agent1, IConflictingAgent agent2)
        {
            _agent1 = agent1;
            _agent2 = agent2;
        }

        protected bool Equals(AgentsConflict other)
        {
            return Equals(_agent1, other._agent1) && Equals(_agent2, other._agent2);
        }

        public bool Equals(IAgentsCoupleConflict other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return (other.Agent1() == _agent1 && other.Agent2() == _agent2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AgentsConflict)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_agent1, _agent2);
        }
        
    }
}