namespace Fovero.Model.Tiling;

public interface ITile
{
    ushort Ordinal { get; }

    Point2D Center { get; }

    Rectangle Bounds { get; }

    IEnumerable<IEdge> Edges { get; }
}
