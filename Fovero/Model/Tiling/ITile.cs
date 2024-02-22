using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling;

public interface ITile
{
    ushort Ordinal { get; }

    Point2D Center { get; }

    Rectangle Bounds { get; }

    IEnumerable<IEdge> Edges { get; }

    public string PathData => string.Join(" ", Edges.Select((edge, n) => n == 0 ? edge.PathData : edge.DrawData));
}
