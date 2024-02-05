namespace Fovero.Model.Tiling;

public class Edge : IEdge, IEquatable<Edge>
{
    public Edge(ITile origin, Point2D start, Point2D end)
    {
        Origin = origin;
        Start = start;
        End = end;

        var midPoint = start.MidPointTo(end);
        Id = (int)Math.Round(midPoint.Y * 10) * 1000 + (int)Math.Round(midPoint.X * 10);
    }

    public int Id { get; }

    public ITile Origin { get; }

    public Point2D Start { get; }

    public Point2D End { get; }

    public bool Equals(Edge other)
    {
        return other is not null && (ReferenceEquals(this, other) || Id == other.Id);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Edge);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public override string ToString()
    {
        return Id.ToString();
    }
}
