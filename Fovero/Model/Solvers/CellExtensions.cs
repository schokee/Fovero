using Fovero.Model.DataStructures;

namespace Fovero.Model.Solvers;

internal static class CellExtensions
{
    public static IEnumerable<Path<ICell>> Traverse(this ICell origin, Func<ICell, float> prioritise)
    {
        var visitedCells = new HashSet<ICell>();
        var pathsToCheck = new ExtendedQueue<Path<ICell>>();

        pathsToCheck.Enqueue(new Path<ICell>(origin), 0);

        while (pathsToCheck.Count > 0)
        {
            var path = pathsToCheck.Dequeue();

            if (visitedCells.Add(path.Last))
            {
                yield return path;

                foreach (var neighbor in path.Last.AccessibleAdjacentCells.Except(visitedCells))
                {
                    pathsToCheck.Enqueue(path.To(neighbor), prioritise(neighbor));
                }
            }
        }
    }

    public static IEnumerable<CollectionChange> SwitchTo<T>(this IReadOnlyCollection<T> from, IReadOnlyCollection<T> to)
    {
        var branchedAt = from
            .Zip(to)
            .TakeWhile(x => Equals(x.First, x.Second))
            .Count();

        return Enumerable
            .Repeat((CollectionChange)new RemoveLast(), from.Count - branchedAt)
            .Concat(to.Skip(branchedAt).Select(item => new Append<T>(item)));
    }
}
