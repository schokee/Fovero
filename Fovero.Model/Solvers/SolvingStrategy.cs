using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Solvers;

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

    private delegate IEnumerable<Path<INode>> TraversalStrategy(Path<INode> startingFrom, Func<Path<INode>, IEnumerable<Path<INode>>> selectNeighbors);

    private static SolvingFunction SolveUsing(PathPrioritisation.Method prioritise)
    {
        return (startNode, endNode) =>
        {
            var solver = SolveUsing((startingPath, selectNeighbors) => Traverse.Prioritised(startingPath, 0, selectNeighbors, path => prioritise(path.Last.Location, endNode.Location)));
            return solver.Invoke(startNode, endNode);
        };
    }

    private static SolvingFunction SolveUsing(TraversalStrategy traverse)
    {
        return (startNode, endNode) =>
        {
            var visitedNodes = new HashSet<INode> { startNode };

            return traverse(new Path<INode>(startNode), path => path.Last.Neighbors.Where(visitedNodes.Add).Select(path.To))
                .TakeUntil(path => path.Last.Equals(endNode));
        };
    }
}
