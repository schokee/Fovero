using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public abstract class MultiShapeTiling : ITiling
{
    protected MultiShapeTiling(ushort columns, ushort rows, float spacing = 0.7f)
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

        var lookup = new Dictionary<Location, ITile>();

        foreach (var location in allLocations)
        {
            var tile = CreateTile((ushort)lookup.Count, location, lookup);

            if (tile is null)
            {
                continue;
            }

            lookup.Add(location, tile);
        }

        return lookup.Values;
    }

    protected float Spacing { get; }

    private float OffsetAt(int n)
    {
        return (1 + Spacing) * (n >> 1) + n % 2;
    }

    private Point2D OffsetAt(Location location)
    {
        return new Point2D(OffsetAt(location.Column), OffsetAt(location.Row));
    }

    protected abstract ITile? CreateTile(ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup);

    protected readonly struct Location(int column, int row)
    {
        public int Column { get; init; } = column;

        public int Row { get; init; } = row;
    }

    protected abstract class Tile(MultiShapeTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup) : ITile
    {
        protected Location Location { get; } = location;

        public ushort Ordinal { get; } = ordinal;

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds { get; protected init; } = new Rectangle(format.OffsetAt(location), format.OffsetAt(new Location(location.Column + 1, location.Row + 1))).ToScaledUnits();

        public abstract IEnumerable<IEdge> Edges { get; }

        public override string ToString()
        {
            return Ordinal.ToString();
        }

        protected IEdge CreateEdge(Location neighbor, Point2D start, Point2D end)
        {
            return lookup.TryGetValue(neighbor, out var tile)
                ? Edge.CreateShared(start, end, this, tile)
                : Edge.CreateBorder(start, end, this);
        }
    }

    protected sealed class QuadrilateralTile(MultiShapeTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
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

                        return CreateEdge(neighbor, segment.Start, segment.End);
                    });
            }
        }
    }
}
