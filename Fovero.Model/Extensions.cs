namespace Fovero.Model;

public static class Extensions
{
    public static IEnumerable<Action> ToScript<T>(this IEnumerable<T> source, Action<T> itemAction)
    {
        return source.Select(x => new Action(() => itemAction(x)));
    }

    public static T SelectRandom<T>(this IReadOnlyCollection<T> source, Random random)
    {
        return source.ElementAt(random.Next(source.Count));
    }
}
