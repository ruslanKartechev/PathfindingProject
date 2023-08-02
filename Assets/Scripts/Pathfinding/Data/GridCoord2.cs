using System;

namespace Pathfinding.Data
{
    public readonly struct GridCoord2 : IEquatable<GridCoord2>
    {
        private static readonly float Sqr = (float)Math.Sqrt(2);

        public GridCoord2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x { get; }
        public int y { get; }

        public int Length2()
        {
            return Math.Abs(x) + Math.Abs(y);
        }
        
        /// <summary>
        /// Estimated path distance without obstacles.
        /// </summary>
        public float DistanceEstimate()
        {
            var linearSteps = Math.Abs(Math.Abs(y) - Math.Abs(x));
            var diagonalSteps = Math.Max(Math.Abs(y), Math.Abs(x)) - linearSteps;
            return linearSteps + Sqr * diagonalSteps;
        }
        
        public static GridCoord2 operator +(GridCoord2 a, GridCoord2 b) 
            => new GridCoord2(a.x + b.x, a.y + b.y);
        public static GridCoord2 operator -(GridCoord2 a, GridCoord2 b) 
            => new GridCoord2(a.x - b.x, a.y - b.y);
        public static bool operator ==(GridCoord2 a, GridCoord2 b) 
            => a.x == b.x && a.y == b.y;
        public static bool operator !=(GridCoord2 a, GridCoord2 b) 
            => !(a == b);

        public bool Equals(GridCoord2 other)
            => x == other.x && y == other.y;

        public override bool Equals(object obj)
        {
            if (!(obj is GridCoord2))
                return false;

            var other = (GridCoord2) obj;
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
            => HashCode.Combine(x, y);

        public override string ToString()
            => $"({x}, {y})";
    }
}