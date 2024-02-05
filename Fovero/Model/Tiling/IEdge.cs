namespace Fovero.Model.Tiling;

public interface IEdge
{
    public int Id { get; }

    Point2D Start { get; }

    Point2D End { get; }

    ITile Origin { get; }
}
