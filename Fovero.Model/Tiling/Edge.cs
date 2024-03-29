﻿using Fovero.Model.Geometry;

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
        Id = neighbors.Length == 2 ? neighbors[0].Center.MidPointTo(neighbors[1].Center) : start.MidPointTo(end);
        Start = start;
        End = end;
        DrawData = $"L {end.X},{end.Y}";
        Neighbors = neighbors;
    }

    private DiscretePoint Id { get; }

    public Point2D Start { get; }

    public Point2D End { get; }

    public string PathData => $"M {Start.X},{Start.Y} {DrawData}";

    public string DrawData { get; internal set; }

    public IReadOnlyList<ITile> Neighbors { get; }

    public bool Equals(Edge? other)
    {
        return Id == other?.Id;
    }

    public override bool Equals(object? obj)
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
}
