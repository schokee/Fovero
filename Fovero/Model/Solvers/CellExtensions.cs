namespace Fovero.Model.Solvers;

public static class CellExtensions
{
    public static IEnumerable<ICell> Traverse(this ICell origin, Func<ICell, float> prioritise)
    {
        var visitedCells = new HashSet<ICell>();
        var cellsToCheck = new PriorityQueue<ICell, float>();

        cellsToCheck.Enqueue(origin, 0);

        while (cellsToCheck.Count > 0)
        {
            var cell = cellsToCheck.Dequeue();
            visitedCells.Add(cell);

            yield return cell;

            foreach (var neighbor in cell.AccessibleAdjacentCells.Except(visitedCells))
            {
                cellsToCheck.Enqueue(neighbor, prioritise(neighbor));
            }
        }
    }
}
