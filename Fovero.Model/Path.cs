using System.Collections;

namespace Fovero.Model;

public sealed class Path<T> : IReadOnlyCollection<T>
{
    private readonly Path<T>? _prologue;
    private readonly Lazy<IReadOnlyCollection<T>> _collection;

#pragma warning disable CS8604 // Possible null reference argument.
    public static Path<T> Empty { get; } = new(default);
#pragma warning restore CS8604 // Possible null reference argument.

    public Path(T last) : this(Empty, last)
    {
    }

    private Path(Path<T>? prologue, T last)
    {
        _prologue = prologue;
        _collection = new Lazy<IReadOnlyCollection<T>>(ToArray);

        Count = prologue?.Count + 1 ?? 0;
        Last = last;
    }

    public int Count { get;  }

    public T Last { get; }

    public Path<T> To(T last)
    {
        return new Path<T>(this, last);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _collection.Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator Path<T>(T item)
    {
        return new Path<T>(item);
    }

    private T[] ToArray()
    {
        var result = new T[Count];
        var i = Count;

        for (var path = this; --i >= 0 && path is not null; path = path._prologue)
        {
            result[i] = path.Last;
        }

        return result;
    }
}
