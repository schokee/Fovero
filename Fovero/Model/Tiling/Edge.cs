using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling;

public sealed class Edge : IEdge
{
    private record DiscretePoint(int X, int Y)
    {
        public static implicit operator DiscretePoint(Point2D point)
        {
            return new DiscretePoint((int)point.X, (int)point.Y);
        }
    }

    private Edge(Point2D start, Point2D end, params ITile[] neighbors)
    {
        Id = start.MidPointTo(end).ScaledBy(1000);
        Start = start;
        End = end;
        Neighbors = neighbors;
    }

    private DiscretePoint Id { get; }

    public Point2D Start { get; }

    public Point2D End { get; }

    public IReadOnlyList<ITile> Neighbors { get; }

    public bool Equals(Edge other)
    {
        return Id == other?.Id;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Edge);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Start} - {End} ({Id})";
    }

    public static Edge CreateBorder(Point2D start, Point2D end, ITile relativeTo)
    {
        return new Edge(start, end, relativeTo);
    }

    public static Edge CreateShared(Point2D start, Point2D end, ITile neighborA, ITile neighborB)
    {
        if (neighborA.Ordinal == neighborB.Ordinal)
        {
            throw new InvalidOperationException("Neighbors must be different");
        }

        return new Edge(start, end, neighborA, neighborB);
    }

    //private sealed class TileComparer : IEqualityComparer<ITile>
    //{
    //    public static TileComparer Instance { get; } = new();

    //    public bool Equals(ITile x, ITile y)
    //    {
    //        return x!.Ordinal == y!.Ordinal;
    //    }

    //    public int GetHashCode(ITile obj)
    //    {
    //        return obj.Ordinal;
    //    }
    //}
}
