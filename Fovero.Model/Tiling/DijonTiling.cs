using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public sealed class DijonTiling(ushort columns, ushort rows, float spacing = 0.7f) : MultiShapeTiling(columns, rows, spacing)
{
    protected override ITile? CreateTile(ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
    {
        var evenCol = location.Column % 2 == 0;
        var evenRow = location.Row % 2 == 0;

        return evenCol
            ? evenRow
                ? null
                : new VerticalTile(this, ordinal, location, lookup)
            : evenRow
                ? new HorizontalTile(this, ordinal, location, lookup)
                : new QuadrilateralTile(this, ordinal, location, lookup);
    }

    private sealed class VerticalTile : Tile
    {
        private readonly float _midPoint;

        public VerticalTile(MultiShapeTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
            : base(format, ordinal, location, lookup)
        {
            _midPoint = Bounds.Width / 2;
            Bounds = new Rectangle(new Point2D(Bounds.Left, Bounds.Top - _midPoint), new Point2D(Bounds.Right, Bounds.Bottom + _midPoint));
        }

        public override IEnumerable<IEdge> Edges
        {
            get
            {
                return CornerPoints
                    .Repeat()
                    .Pairwise((start, end) => (Start: start, End: end))
                    .Take(6)
                    .Select((segment, edge) =>
                    {
                        var neighbor = edge switch
                        {
                            0 => new Location(Location.Column - 1, Location.Row - 1),
                            1 => new Location(Location.Column + 1, Location.Row - 1),
                            2 => Location with { Column = Location.Column + 1 },
                            3 => new Location(Location.Column + 1, Location.Row + 1),
                            4 => new Location(Location.Column - 1, Location.Row + 1),
                            5 => Location with { Column = Location.Column - 1 },
                            _ => Location
                        };

                        return CreateEdge(neighbor, segment.Start, segment.End);
                    });
            }
        }

        private IEnumerable<Point2D> CornerPoints
        {
            get
            {
                yield return new Point2D(Bounds.Left, Bounds.Top + _midPoint);
                yield return new Point2D(Bounds.Left + _midPoint, Bounds.Top);
                yield return new Point2D(Bounds.Right, Bounds.Top + _midPoint);
                yield return new Point2D(Bounds.Right, Bounds.Bottom - _midPoint);
                yield return new Point2D(Bounds.Left + _midPoint, Bounds.Bottom);
                yield return new Point2D(Bounds.Left, Bounds.Bottom - _midPoint);
            }
        }
    }

    private sealed class HorizontalTile : Tile
    {
        private readonly float _midPoint;

        public HorizontalTile(MultiShapeTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
            : base(format, ordinal, location, lookup)
        {
            _midPoint = Bounds.Height / 2;
            Bounds = new Rectangle(new Point2D(Bounds.Left - _midPoint, Bounds.Top), new Point2D(Bounds.Right + _midPoint, Bounds.Bottom));
        }

        public override IEnumerable<IEdge> Edges
        {
            get
            {
                return CornerPoints
                    .Repeat()
                    .Pairwise((start, end) => (Start: start, End: end))
                    .Take(6)
                    .Select((segment, edge) =>
                    {
                        var neighbor = edge switch
                        {
                            0 => new Location(Location.Column - 1, Location.Row - 1),
                            1 => Location with { Row = Location.Row - 1 },
                            2 => new Location(Location.Column + 1, Location.Row - 1),
                            3 => new Location(Location.Column + 1, Location.Row + 1),
                            4 => Location with { Row = Location.Row + 1 },
                            5 => new Location(Location.Column - 1, Location.Row + 1),
                            _ => Location
                        };

                        return CreateEdge(neighbor, segment.Start, segment.End);
                    });
            }
        }

        private IEnumerable<Point2D> CornerPoints
        {
            get
            {
                yield return new Point2D(Bounds.Left, Bounds.Top + _midPoint);
                yield return new Point2D(Bounds.Left + _midPoint, Bounds.Top);
                yield return new Point2D(Bounds.Right - _midPoint, Bounds.Top);
                yield return new Point2D(Bounds.Right, Bounds.Top + _midPoint);
                yield return new Point2D(Bounds.Right - _midPoint, Bounds.Bottom);
                yield return new Point2D(Bounds.Left + _midPoint, Bounds.Bottom);
            }
        }
    }
}
