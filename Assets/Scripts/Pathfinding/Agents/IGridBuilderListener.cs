using Pathfinding.Grid;
namespace Pathfinding.Agents
{
    public interface IGridBuilderListener
    {
        public void SetGrid(WorldGrid grid);
    }
}