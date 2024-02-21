namespace Fovero.Model.Solvers;

public abstract class CollectionChange;

public sealed class RemoveLast : CollectionChange;

public sealed class Append<T>(T item) : CollectionChange
{
    public T Item { get; } = item;
}
