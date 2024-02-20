using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Solvers;

public delegate IEnumerable<Path> SolvingFunction(ICell origin, ICell goal);

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

    public static SolvingStrategy AStarEuclidean => new("A* Euclidean", SolveUsing(PathPrioritisation.EuclidianDistance));
    public static SolvingStrategy AStarManhattan => new("A* Manhattan", SolveUsing(PathPrioritisation.ManhattanDistance));
    public static SolvingStrategy BreadthFirstSearch => new("BFS", SolveUsing(PathPrioritisation.Indiscriminate));

    private static class PathPrioritisation
    {
        public delegate float Method(Point2D from, Point2D to);

        public static Method Indiscriminate => (_, _) => 0;

        public static Method ManhattanDistance => (from, to) => from.ManhattanDistanceTo(to);

        public static Method EuclidianDistance => (from, to) => from.EuclidianDistanceTo(to);
    }

    private static SolvingFunction SolveUsing(PathPrioritisation.Method prioritisePath)
    {
        return (origin, goal) => origin
            .Traverse(cell => prioritisePath(cell.Location, goal.Location))
            .TakeUntil(path => path.LastCell.Equals(goal));
    }
}
