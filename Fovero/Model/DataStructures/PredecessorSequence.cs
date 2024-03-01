using Caliburn.Micro;
using MoreLinq;

namespace Fovero.Model.DataStructures;

public class PredecessorSequence<T> where T : IEquatable<T>
{
    private readonly Dictionary<T, T> _previous = new();

    public void Insert(IEnumerable<T> sequence)
    {
        var list = sequence.ToList();

        if (list.Count < 2)
        {
            throw new InvalidOperationException();
        }

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
            ? null
            : GetPredecessors(item)
                .Reverse()
                .Append(item);
    }

    public void InsertAfter(T item, T previous)
    {
        _previous[item] = previous;
    }

    public void Clear()
    {
        _previous.Clear();
    }
}
