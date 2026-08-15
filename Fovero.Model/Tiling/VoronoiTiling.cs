using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling;

public sealed class VoronoiTiling : ITiling
{
    private const float MaxJitter = 0.35f;
    private const float Tolerance = 0.01f;
    private const float NeighborTolerance = 0.25f;

    public VoronoiTiling(ushort columns, ushort rows, ushort variation = 1)
    {
        Columns = columns;
        Rows = rows;
        Variation = Math.Max((ushort)1, variation);
        Bounds = new Rectangle(0, 0, Columns, Rows).ToScaledUnits();
    }

    public ushort Columns { get; }

    public ushort Rows { get; }

    public ushort Variation { get; }

    public Rectangle Bounds { get; }

    public IEnumerable<ITile> Generate()
    {
        var sites = CreateSites().ToArray();
        var polygons = sites.Select(site => CreatePolygon(site, sites)).ToArray();

        var tiles = polygons
            .Select((polygon, n) => new VoronoiTile(sites[n].Ordinal, sites[n].Center, CreateBounds(polygon)))
            .ToArray();

        foreach (var (tile, polygon) in tiles.Zip(polygons))
        {
            tile.SetEdges(CreateEdges(tile, polygon, tiles).ToArray());
        }

        return tiles;
    }

    private IEnumerable<Site> CreateSites()
    {
        ushort ordinal = 0;

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                yield return new Site(ordinal++, CreateCenter(column, row));
            }
        }
    }

    private Point2D CreateCenter(int column, int row)
    {
        var jitter = MaxJitter * Variation;
        var x = (column + 0.5f + Jitter(column, row, 0x9E3779B9u) * jitter) * Scaling.Unit;
        var y = (row + 0.5f + Jitter(column, row, 0x85EBCA77u) * jitter) * Scaling.Unit;
        return new Point2D(x, y);
    }

    private static float Jitter(int column, int row, uint salt)
    {
        var value = (uint)(column + 1) * 374761393u + (uint)(row + 1) * 668265263u + salt;
        value = (value ^ (value >> 13)) * 1274126177u;
        value ^= value >> 16;
        return value / (float)uint.MaxValue - 0.5f;
    }

    private List<Point2D> CreatePolygon(Site site, IReadOnlyList<Site> sites)
    {
        var polygon = new List<Point2D>
        {
            Bounds.TopLeft,
            new(Bounds.Right, Bounds.Top),
            Bounds.BottomRight,
            new(Bounds.Left, Bounds.Bottom)
        };

        foreach (var other in sites)
        {
            if (other.Ordinal == site.Ordinal)
            {
                continue;
            }

            polygon = ClipPolygon(polygon, site.Center, other.Center);

            if (polygon.Count < 3)
            {
                break;
            }
        }

        return polygon;
    }

    private static List<Point2D> ClipPolygon(IReadOnlyList<Point2D> polygon, Point2D site, Point2D other)
    {
        var result = new List<Point2D>();

        if (polygon.Count == 0)
        {
            return result;
        }

        for (var index = 0; index < polygon.Count; index++)
        {
            var start = polygon[index];
            var end = polygon[(index + 1) % polygon.Count];
            var startInside = Evaluate(start, site, other) <= Tolerance;
            var endInside = Evaluate(end, site, other) <= Tolerance;

            if (startInside && endInside)
            {
                AddPoint(result, end);
                continue;
            }

            if (startInside)
            {
                AddPoint(result, FindIntersection(start, end, site, other));
                continue;
            }

            if (endInside)
            {
                AddPoint(result, FindIntersection(start, end, site, other));
                AddPoint(result, end);
            }
        }

        if (result.Count > 1 && AreClose(result[0], result[^1]))
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    private static float Evaluate(Point2D point, Point2D site, Point2D other)
    {
        var midpoint = site.MidPointTo(other);
        var dx = other.X - site.X;
        var dy = other.Y - site.Y;
        return ((point.X - midpoint.X) * dx) + ((point.Y - midpoint.Y) * dy);
    }

    private static Point2D FindIntersection(Point2D start, Point2D end, Point2D site, Point2D other)
    {
        var startValue = Evaluate(start, site, other);
        var endValue = Evaluate(end, site, other);
        var offset = startValue / (startValue - endValue);
        return new Point2D(start.X + ((end.X - start.X) * offset), start.Y + ((end.Y - start.Y) * offset));
    }

    private static void AddPoint(ICollection<Point2D> polygon, Point2D point)
    {
        if (polygon.Count == 0)
        {
            polygon.Add(point);
            return;
        }

        if (polygon.Last() is var previous && !AreClose(previous, point))
        {
            polygon.Add(point);
        }
    }

    private static bool AreClose(Point2D first, Point2D second)
    {
        return MathF.Abs(first.X - second.X) <= Tolerance && MathF.Abs(first.Y - second.Y) <= Tolerance;
    }

    private static Rectangle CreateBounds(IReadOnlyList<Point2D> polygon)
    {
        var left = polygon[0].X;
        var top = polygon[0].Y;
        var right = polygon[0].X;
        var bottom = polygon[0].Y;

        foreach (var point in polygon.Skip(1))
        {
            left = MathF.Min(left, point.X);
            top = MathF.Min(top, point.Y);
            right = MathF.Max(right, point.X);
            bottom = MathF.Max(bottom, point.Y);
        }

        return new Rectangle(left, top, right - left, bottom - top);
    }

    private IEnumerable<IEdge> CreateEdges(VoronoiTile tile, IReadOnlyList<Point2D> polygon, IReadOnlyList<VoronoiTile> tiles)
    {
        for (var index = 0; index < polygon.Count; index++)
        {
            var start = polygon[index];
            var end = polygon[(index + 1) % polygon.Count];

            if (IsBoundaryEdge(start, end))
            {
                yield return Edge.CreateBorder(start, end, tile);
                continue;
            }

            var neighbor = SelectNeighbor(tile, start, end, tiles);
            yield return neighbor is null
                ? Edge.CreateBorder(start, end, tile)
                : Edge.CreateShared(start, end, tile, neighbor);
        }
    }

    private bool IsBoundaryEdge(Point2D start, Point2D end)
    {
        return IsOn(start.X, Bounds.Left) && IsOn(end.X, Bounds.Left)
            || IsOn(start.X, Bounds.Right) && IsOn(end.X, Bounds.Right)
            || IsOn(start.Y, Bounds.Top) && IsOn(end.Y, Bounds.Top)
            || IsOn(start.Y, Bounds.Bottom) && IsOn(end.Y, Bounds.Bottom);
    }

    private static bool IsOn(float value, float expected)
    {
        return MathF.Abs(value - expected) <= Tolerance;
    }

    private static VoronoiTile? SelectNeighbor(VoronoiTile tile, Point2D start, Point2D end, IReadOnlyList<VoronoiTile> tiles)
    {
        var midpoint = start.MidPointTo(end);
        var currentDistance = midpoint.SquaredDistanceTo(tile.Center);
        VoronoiTile? neighbor = null;
        var smallestDifference = float.MaxValue;

        foreach (var other in tiles)
        {
            if (other.Ordinal == tile.Ordinal)
            {
                continue;
            }

            var difference = MathF.Abs(midpoint.SquaredDistanceTo(other.Center) - currentDistance);

            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                neighbor = other;
            }
        }

        return smallestDifference <= NeighborTolerance ? neighbor : null;
    }

    private sealed record Site(ushort Ordinal, Point2D Center);

    private sealed class VoronoiTile(ushort ordinal, Point2D center, Rectangle bounds) : ITile
    {
        private IReadOnlyList<IEdge> _edges = [];

        public ushort Ordinal { get; } = ordinal;

        public Point2D Center { get; } = center;

        public Rectangle Bounds { get; } = bounds;

        public IEnumerable<IEdge> Edges => _edges;

        public void SetEdges(IReadOnlyList<IEdge> edges)
        {
            _edges = edges;
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }
    }
}
