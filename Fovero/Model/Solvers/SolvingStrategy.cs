using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Solvers;

public delegate IEnumerable<IReadOnlyCollection<ICell>> SolvingFunction(ICell origin, ICell goal);

public record SolvingStrategy(string Name, SolvingFunction FindPath)
{
    public override string ToString()
    {
        return Name;
    }

    public static IReadOnlyList<SolvingStrategy> All =>
    [
        AStarEuclidean,
        AStarManhattan,
        BreadthFirstSearch,
        DepthFirstSearch,
        RandomWalk,
        HeapWalk
    ];

    public static SolvingStrategy AStarEuclidean => new("A* Euclidean", SolveUsing(PathPrioritisation.EuclidianDistance));
    public static SolvingStrategy AStarManhattan => new("A* Manhattan", SolveUsing(PathPrioritisation.ManhattanDistance));
    public static SolvingStrategy BreadthFirstSearch => new("Breadth-first Search", SolveUsing(Traverse.BreadthFirst));
    public static SolvingStrategy DepthFirstSearch => new("Depth-first Search", SolveUsing(Traverse.DepthFirst));
    public static SolvingStrategy RandomWalk => new("Random Walk", SolveUsing(PathPrioritisation.Random));
    public static SolvingStrategy HeapWalk => new("Heap Walk", SolveUsing(PathPrioritisation.AllEqual));

    private static class PathPrioritisation
    {
        public delegate float Method(Point2D from, Point2D to);

        public static Method ManhattanDistance => (from, to) => from.ManhattanDistanceTo(to);

        public static Method EuclidianDistance => (from, to) => from.EuclidianDistanceTo(to);

        public static Method Random => (_, _) => System.Random.Shared.NextSingle();

        public static Method AllEqual => (_, _) => 0;
    }

    private delegate IEnumerable<Path<ICell>> TraversalStrategy(Path<ICell> startingFrom, Func<Path<ICell>, IEnumerable<Path<ICell>>> selectNeighbors);

    private static SolvingFunction SolveUsing(PathPrioritisation.Method prioritise)
    {
        return (startCell, endCell) =>
        {
            var solver = SolveUsing((startingPath, selectNeighbors) => Traverse.Prioritised(startingPath, selectNeighbors, path => prioritise(path.Last.Location, endCell.Location)));
            return solver.Invoke(startCell, endCell);
        };
    }

    private static SolvingFunction SolveUsing(TraversalStrategy traverse)
    {
        return (startCell, endCell) =>
        {
            var visitedCells = new HashSet<ICell> { startCell };

            return traverse(new Path<ICell>(startCell), path => path.Last.AccessibleAdjacentCells.Where(visitedCells.Add).Select(path.To))
                .TakeUntil(path => path.Last.Equals(endCell));
        };
    }
}
