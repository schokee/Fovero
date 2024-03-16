using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public class PalazzoTiling : ITiling
{
    private float Spacing { get; }

    public PalazzoTiling(ushort columns, ushort rows, float spacing = 0.5f)
    {
        Spacing = spacing;
        Columns = (ushort)(columns * 2 - 1);
        Rows = (ushort)(rows * 2 - 1);
        Bounds = new Rectangle(0, 0, OffsetAt(Columns), OffsetAt(Rows)).ToScaledUnits();
    }

    public ushort Columns { get; }

    public ushort Rows { get; }

    public Rectangle Bounds { get; }

    public Func<int, int, bool> Mask { get; set; } = (c, r) => c % 2 == 1 && r % 2  == 1;

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
            lookup.Add(location, new Tile(this, ordinal++, location, lookup));
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

    private record Tile : ITile
    {
        private readonly Location _location;
        private readonly IReadOnlyDictionary<Location, ITile> _lookup;

        public Tile(PalazzoTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
        {
            _location = location;
            _lookup = lookup;

            Ordinal = ordinal;
            Bounds = new Rectangle(format.OffsetAt(location), format.OffsetAt(new Location(location.Column + 1, location.Row + 1))).ToScaledUnits();
        }

        public ushort Ordinal { get; }

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds { get; }

        public IEnumerable<Point2D> CornerPoints
        {
            get
            {
                var bounds = Bounds;

                yield return new Point2D(bounds.Left, bounds.Top);
                yield return new Point2D(bounds.Right, bounds.Top);
                yield return new Point2D(bounds.Right, bounds.Bottom);
                yield return new Point2D(bounds.Left, bounds.Bottom);
            }
        }

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
                            0 => _location with { Row = _location.Row - 1 },
                            1 => _location with { Column = _location.Column + 1 },
                            2 => _location with { Row = _location.Row + 1 },
                            3 => _location with { Column = _location.Column - 1 },
                            _ => _location
                        };

                        return _lookup.TryGetValue(neighbor, out var tile)
                            ? Edge.CreateShared(segment.Start, segment.End, this, tile)
                            : Edge.CreateBorder(segment.Start, segment.End, this);
                    });
            }
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }
    }
}
