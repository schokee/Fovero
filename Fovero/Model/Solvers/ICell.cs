using Fovero.Model.Geometry;

namespace Fovero.Model.Solvers;

/// <summary>
/// Represents a cell in a maze.
/// </summary>
public interface ICell
{
    Point2D Location { get; }

    IEnumerable<ICell> AccessibleAdjacentCells { get; }

    ushort Ordinal { get; }
}
