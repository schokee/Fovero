namespace Fovero.Model;

/// <summary>
/// Represents a shared border with properties to determine its state and neighbors.
/// </summary>
public interface ISharedBorder
{
    /// <summary>
    /// Gets a value indicating whether the border is open.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Gets the collection of neighboring elements.
    /// </summary>
    IEnumerable<ushort> Neighbors { get; }

    /// <summary>
    /// Gets the first neighbor in the collection.
    /// </summary>
    ushort NeighborA => Neighbors.ElementAt(0);

    /// <summary>
    /// Gets the second neighbor in the collection.
    /// </summary>
    ushort NeighborB => Neighbors.ElementAt(1);

    /// <summary>
    /// Selects pathways from the specified source node to its neighbors.
    /// </summary>
    /// <typeparam name="T">The type of the nodes.</typeparam>
    /// <param name="source">The source node.</param>
    /// <param name="createNode">A function to create a node from a neighbor identifier.</param>
    /// <returns>An enumerable of pathways from the source node to its neighbors.</returns>
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
