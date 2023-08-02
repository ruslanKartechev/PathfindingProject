namespace Pathfinding.Data
{
    public readonly struct PathNode
    {
        public PathNode(GridCoord2 position, double costSoFar, double heuristic)
        {
            Position = position;
            CostSoFar = costSoFar;
            EstimatedTotalCost = costSoFar + heuristic;
        }

        public GridCoord2 Position { get; }
        public double CostSoFar { get; }
        public double EstimatedTotalCost { get; }
    }
    
}