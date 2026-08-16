using Caliburn.Micro;

namespace Fovero.UI;

public sealed class Selectable<T>(T item) : PropertyChangedBase
{
    public T Item { get; } = item;

    public bool IsSelected
    {
        get;
        set => Set(ref field, value);
    } = true;
}
