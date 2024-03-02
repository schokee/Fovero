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
        BreadthFirstSearch
    ];

    public static SolvingStrategy AStarEuclidean => new("A* Euclidean", SolveUsing(TraversalPrioritisedBy(PathPrioritisation.EuclidianDistance)));
    public static SolvingStrategy AStarManhattan => new("A* Manhattan", SolveUsing(TraversalPrioritisedBy(PathPrioritisation.ManhattanDistance)));
    public static SolvingStrategy BreadthFirstSearch => new("BFS", SolveUsing(BreadthFirstTraversal));

    private static class PathPrioritisation
    {
        public delegate float Method(Point2D from, Point2D to);

        public static Method ManhattanDistance => (from, to) => from.ManhattanDistanceTo(to);

        public static Method EuclidianDistance => (from, to) => from.EuclidianDistanceTo(to);
    }

    private delegate IEnumerable<Path<ICell>> TraversalStrategy(ICell origin, ICell goal);

    private static SolvingFunction SolveUsing(TraversalStrategy traverse)
    {
        return (origin, goal) => traverse(origin, goal).TakeUntil(path => path.Last.Equals(goal));
    }

    private static TraversalStrategy TraversalPrioritisedBy(PathPrioritisation.Method prioritise) => (startingCell, goal) =>
    {
        var visitedCells = new HashSet<ICell>();

        return Traverse.Prioritised(new Path<ICell>(startingCell),
            path => path.Last.AccessibleAdjacentCells.Where(visitedCells.Add).Select(path.To),
            path => prioritise(path.Last.Location, goal.Location));
    };

    private static TraversalStrategy BreadthFirstTraversal => (startingCell, _) =>
    {
        var visitedCells = new HashSet<ICell>();

        return Traverse.BreadthFirst(new Path<ICell>(startingCell),
            path => path.Last.AccessibleAdjacentCells.Where(visitedCells.Add).Select(path.To));
    };
}
