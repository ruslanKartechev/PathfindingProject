namespace Pathfinding.Agents
{
    public interface IPathAgentListener
    {
        void OnBeganRotation();
        void OnEndedRotation();
        void OnBeganMovement();
        void OnStopped();
        void OnReachedFinalPoint();
        void OnStateChanged(AgentState newState, AgentState prevState);
    }
}