using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling;

public interface IEdge
{
    Point2D Start { get; }

    Point2D End { get; }

    IReadOnlyList<ITile> Neighbors { get; }

    bool IsShared => Neighbors.Count > 1;

    string PathData { get; }
}
