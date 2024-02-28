using Caliburn.Micro;

namespace Fovero.Model;

public static class Extensions
{
    public static T SelectRandom<T>(this IReadOnlyCollection<T> source, Random random)
    {
        return source.ElementAt(random.Next(source.Count));
    }

    public static bool TrimAllAfter<T>(this IObservableCollection<T> source, T element)
    {
        var endIndex = source.IndexOf(element);

        if (endIndex < 0)
        {
            return false;
        }

        for (var i = source.Count; --i > endIndex;)
        {
            source.RemoveAt(i);
        }

        return true;
    }
}
