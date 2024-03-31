using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public sealed class DijonTiling : ITiling
{
    private float Spacing { get; }

    public DijonTiling(ushort columns, ushort rows, float spacing = 0.7f)
    {
        Spacing = spacing;
        Columns = (ushort)(columns * 2 - 1);
        Rows = (ushort)(rows * 2 - 1);
        Bounds = new Rectangle(0, 0, OffsetAt(Columns), OffsetAt(Rows)).ToScaledUnits();
    }

    public ushort Columns { get; }

    public ushort Rows { get; }

    public Rectangle Bounds { get; }

    public Func<int, int, bool> Mask { get; set; } = (c, r) => c % 2 == 1 && r % 2 == 1;

    public IEnumerable<ITile> Generate()
    {
        var allLocations = Enumerable
            .Range(0, Rows)
            .SelectMany(r => Enumerable
                .Range(0, Columns)
                .Where(c => !Mask(c, r))
                .Select(c => new Location(c, r)));

        ushort ordinal = 0;
        var lookup = new Dictionary<Location, ITile>();

        foreach (var location in allLocations)
        {
            var oddCol = location.Column % 2 == 0;
            var oddRow = location.Row % 2 == 0;

            ITile? tile =
                !oddCol && !oddRow ? new SquareTile(this, ordinal, location, lookup) :
                oddCol && !oddRow ? new VerticalTile(this, ordinal, location, lookup) :
                oddRow && !oddCol ? new HorizontalTile(this, ordinal, location, lookup) : null;

            if (tile is null)
            {
                continue;
            }

            lookup.Add(location, tile);
            ++ordinal;
        }

        return lookup.Values;
    }

    private float OffsetAt(int n)
    {
        return (1 + Spacing) * (n >> 1) + n % 2;
    }

    private Point2D OffsetAt(Location location)
    {
        return new Point2D(OffsetAt(location.Column), OffsetAt(location.Row));
    }

    private readonly struct Location(int column, int row)
    {
        public int Column { get; init; } = column;

        public int Row { get; init; } = row;
    }

    private abstract class Tile : ITile
    {
        protected Location Location { get; }
        private readonly IReadOnlyDictionary<Location, ITile> _lookup;

        protected Tile(DijonTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
        {
            _lookup = lookup;

            Location = location;
            Ordinal = ordinal;
            Bounds = new Rectangle(format.OffsetAt(location), format.OffsetAt(new Location(location.Column + 1, location.Row + 1))).ToScaledUnits();
        }

        public ushort Ordinal { get; }

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds { get; protected init; }

        public abstract IEnumerable<IEdge> Edges { get; }

        public override string ToString()
        {
            return Ordinal.ToString();
        }

        protected IEdge CreateBorder(Location neighbor, Point2D start, Point2D end)
        {
            return _lookup.TryGetValue(neighbor, out var tile)
                ? Edge.CreateShared(start, end, this, tile)
                : Edge.CreateBorder(start, end, this);
        }
    }

    private sealed class SquareTile(DijonTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
        : Tile(format, ordinal, location, lookup)
    {
        private IEnumerable<Point2D> CornerPoints
        {
            get
            {
                yield return new Point2D(Bounds.Left, Bounds.Top);
                yield return new Point2D(Bounds.Right, Bounds.Top);
                yield return new Point2D(Bounds.Right, Bounds.Bottom);
                yield return new Point2D(Bounds.Left, Bounds.Bottom);
            }
        }

        public override IEnumerable<IEdge> Edges
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
                            _ => Location
                        };

                        return CreateBorder(neighbor, segment.Start, segment.End);
                    });
            }
        }
    }

    private sealed class VerticalTile : Tile
    {
        private readonly float _midPoint;

        public VerticalTile(DijonTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
            : base(format, ordinal, location, lookup)
        {
            _midPoint = Bounds.Width / 2;
            Bounds = new Rectangle(new Point2D(Bounds.Left, Bounds.Top - _midPoint), new Point2D(Bounds.Right, Bounds.Bottom + _midPoint));
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

                        return CreateBorder(neighbor, segment.Start, segment.End);
                    });
            }
        }
    }

    private sealed class HorizontalTile : Tile
    {
        private readonly float _midPoint;

        public HorizontalTile(DijonTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
            : base(format, ordinal, location, lookup)
        {
            _midPoint = Bounds.Height / 2;
            Bounds = new Rectangle(new Point2D(Bounds.Left - _midPoint, Bounds.Top), new Point2D(Bounds.Right + _midPoint, Bounds.Bottom));
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

                        return CreateBorder(neighbor, segment.Start, segment.End);
                    });
            }
        }
    }
}
