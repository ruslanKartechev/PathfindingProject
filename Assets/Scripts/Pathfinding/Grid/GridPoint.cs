
namespace Pathfinding.Grid
{
    [System.Serializable]
    public struct GridPoint
    {
        public int X;
        public int Y;
        public bool IsWalkable;
        public float Weight;
        
        public GridPoint(int x, int y, bool isWalkable, float weight)
        {
            X = x;
            Y = y;
            IsWalkable = isWalkable;
            Weight = weight;
        }

        public GridPoint(GridPoint other)
        {
            X = other.X;
            Y = other.Y;
            IsWalkable = other.IsWalkable;
            Weight = other.Weight;
        }

        public static bool operator ==(GridPoint one, GridPoint two)
        {
            return one.X == two.X && one.Y == two.Y;
        } 
        
        public static bool operator !=(GridPoint one, GridPoint two)
        {
            return one.X != two.X || one.Y != two.Y;
        } 
    }
}