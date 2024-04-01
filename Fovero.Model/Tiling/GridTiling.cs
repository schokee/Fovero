using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public abstract class GridTiling(ushort columns, ushort rows) : ITiling
{
    public static Func<int, int, bool> NoMask { get; } = (_, _) => false;

    public ushort Columns { get; } = columns;

    public ushort Rows { get; } = rows;

    public virtual Rectangle Bounds => new Rectangle(0, 0, OffsetAt(Columns), OffsetAt(Rows)).ToScaledUnits();

    public Func<int, int, bool> Mask { get; set; } = NoMask;

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

    protected abstract ITile? CreateTile(ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup);

    protected virtual float OffsetAt(int n)
    {
        return n;
    }

    protected Point2D OffsetAt(Location location)
    {
        return new Point2D(OffsetAt(location.Column), OffsetAt(location.Row));
    }

    protected readonly struct Location(int column, int row)
    {
        public static Location None { get; } = new(int.MinValue, int.MinValue);

        public int Column { get; init; } = column;

        public int Row { get; init; } = row;
    }

    protected abstract class Tile(GridTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup) : ITile
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

    protected sealed class RectangleTile(GridTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
        : Tile(format, ordinal, location, lookup)
    {
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
                            _ => Location.None
                        };

                        return CreateEdge(neighbor, segment.Start, segment.End);
                    });
            }
        }

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
    }
}
