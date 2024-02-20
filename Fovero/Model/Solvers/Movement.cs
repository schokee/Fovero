namespace Fovero.Model.Solvers;

public abstract class Movement;

public sealed class Retreat : Movement;

public sealed class Advance(ICell nextCell) : Movement
{
    public ICell NextCell { get; } = nextCell;
}
