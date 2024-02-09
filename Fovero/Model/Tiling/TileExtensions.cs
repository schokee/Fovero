using Fovero.Model.Solvers;

namespace Fovero.Model.Tiling;

public static class TileExtensions
{
    public static IEnumerable<ITile> GetNeighbors(this ITile tile, IReadOnlyList<IWall> walls)
    {
        return tile.Edges
            .Join(walls, e => e, w => w.Edge, (_, w) => w)
            .Where(w => w.IsOpen && w.IsShared)
            .SelectMany(w => w.Edge.Neighbors)
            .Where(t => t != tile);
    }

    public static double ManhattanDistance(this ITile first, ITile second)
    {
        return Math.Abs(first.Center.X - second.Center.X) +
               Math.Abs(first.Center.Y - second.Center.Y);
    }

    public static double EuclideanDistance(this ITile first, ITile second)
    {
        return Math.Sqrt(Math.Pow(first.Center.X - second.Center.X, 2) +
                         Math.Pow(first.Center.Y - second.Center.Y, 2));
    }
}
