namespace Fovero.Model;

public interface ISharedBorder
{
    bool IsOpen { get; }

    IEnumerable<ushort> Neighbors { get; }

    ushort NeighborA => Neighbors.ElementAt(0);
    ushort NeighborB => Neighbors.ElementAt(1);

    public IEnumerable<(T From, T To)> SelectPathwaysFrom<T>(T source, Func<ushort, T> createNode)
    {
        var nodeA = createNode(NeighborA);
        var nodeB = createNode(NeighborB);

        if (Equals(source, nodeA))
        {
            yield return (source, nodeB);
        }
        else if (Equals(source, nodeB))
        {
            yield return (source, nodeA);
        }
    }
}
