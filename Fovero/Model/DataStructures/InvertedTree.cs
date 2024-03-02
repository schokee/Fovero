using Caliburn.Micro;
using MoreLinq;

namespace Fovero.Model.DataStructures;

public class InvertedTree<T>
{
    private readonly Dictionary<T, T> _previous = new();

    public void Insert(IEnumerable<T> sequence)
    {
        var list = sequence.ToList();

        list.Pairwise((first, second) => (Previous: first, Item: second))
            .Apply(pair => InsertAfter(pair.Item, pair.Previous));
    }

    public IEnumerable<T> GetPredecessors(T item)
    {
        while (_previous.TryGetValue(item, out item))
        {
            yield return item;
        }
    }

    public IEnumerable<T> GetSequenceTo(T item)
    {
        return !_previous.ContainsKey(item)
            ? []
            : GetPredecessors(item)
                .Reverse()
                .Append(item);
    }

    public void InsertAfter(T item, T previous)
    {
        _previous.TryAdd(item, previous);
    }

    public void InsertManyAfter(IEnumerable<T> items, T previous)
    {
        foreach (T item in items)
        {
            InsertAfter(item, previous);
            previous = item;
        }
    }

    public void Clear()
    {
        _previous.Clear();
    }
}
