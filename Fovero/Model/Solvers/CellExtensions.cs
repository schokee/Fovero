namespace Fovero.Model.Solvers;

public static class CellExtensions
{
    public static IEnumerable<Path> Traverse(this ICell origin, Func<ICell, float> prioritise)
    {
        var visitedCells = new HashSet<ICell>();
        var cellsToCheck = new PriorityQueue<Path, float>();

        cellsToCheck.Enqueue(new Path(origin), 0);

        while (cellsToCheck.Count > 0)
        {
            var step = cellsToCheck.Dequeue();

            if (visitedCells.Add(step.LastCell))
            {
                yield return step;

                foreach (var neighbor in step.LastCell.AccessibleAdjacentCells.Except(visitedCells))
                {
                    cellsToCheck.Enqueue(step.To(neighbor), prioritise(neighbor));
                }
            }
        }
    }

    public static IEnumerable<Movement> SwitchTo(this Path from, Path to)
    {
        var branchedAt = from
            .WalkStartToEnd()
            .Zip(to.WalkStartToEnd())
            .TakeWhile(x => Equals(x.First, x.Second))
            .Count();

        var retreat = Enumerable.Repeat((Movement) new Retreat(), from.Count - branchedAt);

        var advance = to
            .WalkEndToStart()
            .Take(to.Count - branchedAt)
            .Reverse()
            .Select(x => new Advance(x));

        return retreat.Concat(advance);
    }
}
