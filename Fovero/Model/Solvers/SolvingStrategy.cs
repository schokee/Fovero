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
        RandomWalk
    ];

    public static SolvingStrategy AStarEuclidean => new("A* Euclidean", SolveUsing(TraversalPrioritisedBy(PathPrioritisation.EuclidianDistance)));
    public static SolvingStrategy AStarManhattan => new("A* Manhattan", SolveUsing(TraversalPrioritisedBy(PathPrioritisation.ManhattanDistance)));
    public static SolvingStrategy BreadthFirstSearch => new("BFS", SolveUsing(BreadthFirstTraversal));
    public static SolvingStrategy DepthFirstSearch => new("DFS", SolveUsing(DepthFirstTraversal));
    public static SolvingStrategy RandomWalk => new("Random Walk", SolveUsing(TraversalPrioritisedBy(PathPrioritisation.Random)));

    private static class PathPrioritisation
    {
        public delegate float Method(Point2D from, Point2D to);

        public static Method ManhattanDistance => (from, to) => from.ManhattanDistanceTo(to);

        public static Method EuclidianDistance => (from, to) => from.EuclidianDistanceTo(to);

        public static Method Random => (_, _) => System.Random.Shared.NextSingle();
    }

    private delegate IEnumerable<Path<ICell>> TraversalStrategy(Path<ICell> origin, ICell goal);

    private static SolvingFunction SolveUsing(TraversalStrategy traverse)
    {
        return (origin, goal) => traverse(new Path<ICell>(origin), goal).TakeUntil(path => path.Last.Equals(goal));
    }

    private static TraversalStrategy TraversalPrioritisedBy(PathPrioritisation.Method prioritise) => (startingFrom, goal) =>
        Traverse.Prioritised(startingFrom, SelectUnvisitedAlternatives, path => prioritise(path.Last.Location, goal.Location));

    private static TraversalStrategy BreadthFirstTraversal => (startingFrom, _) =>
        Traverse.BreadthFirst(startingFrom, SelectUnvisitedAlternatives);

    private static TraversalStrategy DepthFirstTraversal => (startingFrom, _) =>
        Traverse.DepthFirst(startingFrom, SelectUnvisitedAlternatives);

    private static Func<Path<ICell>, IEnumerable<Path<ICell>>> SelectUnvisitedAlternatives
    {
        get
        {
            var visitedCells = new HashSet<ICell>();
            return path => path.Last.AccessibleAdjacentCells.Where(visitedCells.Add).Select(path.To);
        }
    }
}

