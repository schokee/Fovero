using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling;

/// <summary>
/// Represents a tiling interface that defines the structure for tiling operations.
/// </summary>
public interface ITiling
{
    /// <summary>
    /// Gets the bounds of the tiling as a rectangle.
/// </summary>
    Rectangle Bounds { get; }

    /// <summary>
    /// Generates a collection of tiles based on the tiling implementation.
/// </summary>
/// <returns>An enumerable collection of tiles.</returns>
    IEnumerable<ITile> Generate();
}
