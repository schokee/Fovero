using Caliburn.Micro;
using MoreLinq;

namespace Fovero.Model.DataStructures;

public class InvertedTree<T>
{
    private readonly Dictionary<T, T> _parents = new();

    public void Insert(IEnumerable<T> sequence)
    {
        sequence
            .Pairwise((first, second) => (Parent: first, Item: second))
            .Apply(pair => InsertAfter(pair.Item, pair.Parent));
    }

    public IEnumerable<T> GetAncestors(T item)
    {
        while (_parents.TryGetValue(item, out item))
        {
            yield return item;
        }
    }

    public IEnumerable<T> GetPathTo(T item)
    {
        return !_parents.ContainsKey(item)
            ? []
            : GetAncestors(item)
                .Reverse()
                .Append(item);
    }

    public void InsertAfter(T item, T parent)
    {
        _parents.TryAdd(item, parent);
    }

    public void InsertManyAfter(IEnumerable<T> items, T parent)
    {
        Insert(items.Prepend(parent));
    }

    public void Clear()
    {
        _parents.Clear();
    }
}
