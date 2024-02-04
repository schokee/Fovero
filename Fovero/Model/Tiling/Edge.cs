namespace Fovero.Model.Tiling;

public class Edge : IEdge, IEquatable<Edge>
{
    public Edge(ITile origin, Point2D start, Point2D end)
    {
        Origin = origin;
        Start = start;
        End = end;

        //if (start < end)
        //{
        //    Start = start;
        //    End = end;
        //}
        //else
        //{
        //    Start = end;
        //    End = start;
        //}

        var midPoint = new Rectangle(start, end).Center;
        Ordinal = (int)Math.Round((midPoint.Y * 1000 + midPoint.X) * 1000);
    }

    public int Ordinal { get; }

    public ITile Origin { get; }

    public Point2D Start { get; }

    public Point2D End { get; }

    public bool Equals(Edge other)
    {
        return other is not null && (ReferenceEquals(this, other) || Ordinal == other.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Edge);
    }

    public override int GetHashCode()
    {
        return Ordinal;
    }

    public override string ToString()
    {
        return Ordinal.ToString();
    }
}
