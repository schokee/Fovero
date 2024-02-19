namespace Fovero.Model;

public static class Extensions
{
    public static T SelectRandom<T>(this IReadOnlyCollection<T> source, Random random)
    {
        return source.ElementAt(random.Next(source.Count));
    }
}
