using Fovero.Model.Tiling;
using MoreLinq;

namespace Fovero.Model.Solvers;

public delegate IEnumerable<ITile> SolvingFunction(ITile start, ITile end, IReadOnlyList<IWall> walls);

public record SolvingStrategy(string Name, SolvingFunction Solve)
{
    public override string ToString()
    {
        return Name;
    }

    public static SolvingStrategy AStarEuclidean => new("A* Euclidean", GetAStarVariant(TileExtensions.EuclideanDistance));
    public static SolvingStrategy AStarManhattan => new("A* Manhattan", GetAStarVariant(TileExtensions.ManhattanDistance));
    public static SolvingStrategy BreadthFirstSearch => new("BFS", SolveBfs);

    private static IEnumerable<ITile> SolveBfs(ITile start, ITile end, IReadOnlyList<IWall> walls)
    {
        var queue = new Queue<ITile>();
        var visited = new HashSet<ITile>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Any())
        {
            var tile = queue.Dequeue();
            visited.Add(tile);
            yield return tile;

            if (tile.Ordinal == end.Ordinal) yield break;

            tile.GetNeighbors(walls)
                .Where(t => !visited.Contains(t))
                .ForEach(queue.Enqueue);
        }
    }

    private static SolvingFunction GetAStarVariant(Func<ITile, ITile, double> heuristic)
        => (start, end, walls) => SolveAStar(start, end, walls, heuristic);

    private static IEnumerable<ITile> SolveAStar(ITile start, ITile end, IReadOnlyList<IWall> walls, Func<ITile, ITile, double> heuristic)
    {
        var queue = new PriorityQueue<(ITile tile, int dist), double>();
        var visited = new HashSet<ITile>();

        queue.Enqueue((start, 0), 0);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var element = queue.Dequeue();
            visited.Add(element.tile);
            yield return element.tile;

            if (element.tile.Ordinal == end.Ordinal) yield break;

            element.tile
                .GetNeighbors(walls)
                .Where(t => !visited.Contains(t))
                .ForEach(t => queue.Enqueue((t, element.dist + 1), heuristic(t, end)));
        }
    }
}
