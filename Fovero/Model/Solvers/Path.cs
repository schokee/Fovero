namespace Fovero.Model.Solvers;

public record Path
{
    public static Path Empty { get; } = new(default, null, 0);

    public Path(ICell lastCell) : this(lastCell, null, 1)
    {
    }

    private Path(ICell lastCell, Path precedingSteps, int count)
    {
        LastCell = lastCell;
        PrecedingSteps = precedingSteps;
        Count = count;
    }

    public ICell LastCell { get; }

    public Path PrecedingSteps { get; }

    public int Count { get;  }

    public Path To(ICell lastCell)
    {
        return new Path(lastCell, this, Count + 1);
    }

    public IEnumerable<ICell> WalkStartToEnd()
    {
        return WalkEndToStart().Reverse();
    }

    public IEnumerable<ICell> WalkEndToStart()
    {
        for (var step = this; step?.Count > 0; step = step.PrecedingSteps)
        {
            yield return step.LastCell;
        }
    }
}
