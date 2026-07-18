using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public sealed class QuarteredHexTiling(ushort columns, ushort rows) : GridTiling((ushort)((columns * 2 + 1) / 2), (ushort)((rows * 2 + 1) / 2))
{
    private float CellHeight { get; } = MathF.Sqrt(3) / 2;

    private enum Orientation
    {
        SlopeLeftNarrowTop,
        SlopeLeftNarrowBottom,
        SlopeRightNarrowBottom,
        SlopeRightNarrowTop,
    }

    private Func<Location, Orientation> SelectOrientation { get; } =  location => 
        (Orientation)((location.Column & 1) | ((((location.Column >> 1) ^ location.Row) & 1) << 1));
        //(Orientation)(location.Column & 1);

    protected override ITile? CreateTile(ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
    {
        return new RhombusTile(this, ordinal, location, lookup)
        {
            Type = SelectOrientation(location)
        };
    }

    private sealed class RhombusTile(
        QuarteredHexTiling format,
        ushort ordinal,
        Location location,
        IReadOnlyDictionary<Location, ITile> lookup) : ITile
    {
        private Location Location { get; } = location;

        public Orientation Type { get; init; }

        public ushort Ordinal { get; } = ordinal;

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds { get; } = new Rectangle(
            (int)(location.Column / 2) * 1.5f + (location.Column % 2 == 0 ? 0 : 0.5f),
            location.Row * format.CellHeight, 1, format.CellHeight).ToScaledUnits();

        public IEnumerable<IEdge> Edges
        {
            get
            {
                return CornerPoints
                    .Repeat()
                    .Pairwise((start, end) => (Start: start, End: end))
                    .Take(4)
                    .Select((segment, edge) =>
                    {
                        var neighbor = edge switch
                        {
                            0 => Location with { Row = Location.Row - 1 },
                            1 => Location with { Column = Location.Column + 1 },
                            2 => Location with { Row = Location.Row + 1 },
                            3 => Location with { Column = Location.Column - 1 },
                            _ => Location.None
                        };

                        return CreateEdge(neighbor, segment.Start, segment.End);
                    });
            }
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }

        private Edge CreateEdge(Location neighbor, Point2D start, Point2D end)
        {
            return lookup.TryGetValue(neighbor, out var tile)
                ? Edge.CreateShared(start, end, this, tile)
                : Edge.CreateBorder(start, end, this);
        }

        private IEnumerable<Point2D> CornerPoints
        {
            get
            {
                var bounds = Bounds;
                var midPoint = bounds.Center.X;

                switch (Type)
                {
                    case Orientation.SlopeLeftNarrowTop:
                        yield return new Point2D(bounds.Left, bounds.Top);
                        yield return new Point2D(midPoint, bounds.Top);
                        yield return new Point2D(bounds.Right, bounds.Bottom);
                        yield return new Point2D(bounds.Left, bounds.Bottom);
                        break;

                    case Orientation.SlopeLeftNarrowBottom:
                        yield return new Point2D(bounds.Left, bounds.Top);
                        yield return new Point2D(bounds.Right, bounds.Top);
                        yield return new Point2D(bounds.Right, bounds.Bottom);
                        yield return new Point2D(midPoint, bounds.Bottom);
                        break;

                    case Orientation.SlopeRightNarrowBottom:
                        yield return new Point2D(bounds.Left, bounds.Top);
                        yield return new Point2D(bounds.Right, bounds.Top);
                        yield return new Point2D(midPoint, bounds.Bottom);
                        yield return new Point2D(bounds.Left, bounds.Bottom);
                        break;

                    case Orientation.SlopeRightNarrowTop:
                        yield return new Point2D(midPoint, bounds.Top);
                        yield return new Point2D(bounds.Right, bounds.Top);
                        yield return new Point2D(bounds.Right, bounds.Bottom);
                        yield return new Point2D(bounds.Left, bounds.Bottom);
                        break;
                }
            }
        }
    }
}
