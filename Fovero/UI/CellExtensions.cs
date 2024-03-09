namespace Fovero.UI;

internal static class CellExtensions
{
    public static IEnumerable<CollectionChange> SwitchTo<T>(this IReadOnlyCollection<T> from, IReadOnlyCollection<T> to)
    {
        var branchedAt = from
            .Zip(to)
            .TakeWhile(x => Equals(x.First, x.Second))
            .Count();

        return Enumerable
            .Repeat((CollectionChange)new RemoveLast(), from.Count - branchedAt)
            .Concat(to.Skip(branchedAt).Select(item => new Append<T>(item)));
    }
}
