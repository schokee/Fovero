namespace Fovero.Model.Presentation;

internal abstract class CollectionChange;

internal sealed class RemoveLast : CollectionChange;

internal sealed class Append<T>(T item) : CollectionChange
{
    public T Item { get; } = item;
}
