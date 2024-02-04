namespace Fovero.Model.Tiling;

public interface IEdge
{
    public int Ordinal { get; }

    Point2D Start { get; }

    Point2D End { get; }

    ITile Origin { get; }
}
