using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling;

/// <summary>
/// Represents a tile with specific properties such as ordinal, center, bounds, and edges.
/// </summary>
public interface ITile
{
    /// <summary>
    /// Gets the ordinal number of the tile.
    /// </summary>
    ushort Ordinal { get; }

    /// <summary>
    /// Gets the center point of the tile.
    /// </summary>
    Point2D Center { get; }

    /// <summary>
    /// Gets the rectangular bounds of the tile.
    /// </summary>
    Rectangle Bounds { get; }

    /// <summary>
    /// Gets the collection of edges that define the tile.
    /// </summary>
    IEnumerable<IEdge> Edges { get; }
}
