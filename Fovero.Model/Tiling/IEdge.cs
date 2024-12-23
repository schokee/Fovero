namespace Fovero.Model.Tiling;

/// <summary>
/// Represents an edge in a tiling system.
/// </summary>
public interface IEdge
{
    /// <summary>
    /// Gets the list of neighboring tiles.
    /// </summary>
    IReadOnlyList<ITile> Neighbors { get; }

    /// <summary>
    /// Gets a value indicating whether the edge is shared by more than one tile.
    /// </summary>
    bool IsShared => Neighbors.Count > 1;

    /// <summary>
    /// Gets the path data for the edge.
    /// </summary>
    string PathData { get; }

    /// <summary>
    /// Gets the draw data for the edge.
    /// </summary>
    string DrawData { get; }
}
