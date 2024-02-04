namespace Fovero.Model.Tiling;

public interface ITile
{
    int Ordinal { get; }

    Point2D Center { get; }

    Rectangle Bounds { get; }

    IEnumerable<IEdge> Edges { get; }
}
