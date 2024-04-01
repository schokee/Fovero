using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public class LatticeTiling(ushort columns, ushort rows) : GridTiling(columns, rows)
{
    protected override ITile CreateTile(ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
    {
        return new CrossTile(this, ordinal, location, lookup);
    }

    private sealed class CrossTile(GridTiling format, ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
        : Tile(format, ordinal, location, lookup)
    {
        public override IEnumerable<IEdge> Edges
        {
            get
            {
                return CornerPoints
                    .Repeat()
                    .Pairwise((start, end) => (Start: start, End: end))
                    .Take(12)
                    .Select((segment, edge) =>
                    {
                        var neighbor = edge switch
                        {
                            0 => Location with { Row = Location.Row - 1 },
                            3 => Location with { Column = Location.Column + 1 },
                            6 => Location with { Row = Location.Row + 1 },
                            9 => Location with { Column = Location.Column - 1 },
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

        private IEnumerable<Point2D> CornerPoints
        {
            get
            {
                var offset = Bounds.Width / 6;
                var centerSquare = new Rectangle(Bounds.Left + offset, Bounds.Top + offset, offset * 4, offset * 4);

                // Top edge
                yield return new Point2D(centerSquare.Left, Bounds.Top);
                yield return new Point2D(centerSquare.Right, Bounds.Top);

                yield return new Point2D(centerSquare.Right, centerSquare.Top);

                // Right edge
                yield return new Point2D(Bounds.Right, centerSquare.Top);
                yield return new Point2D(Bounds.Right, centerSquare.Bottom);

                yield return new Point2D(centerSquare.Right, centerSquare.Bottom);

                // Bottom edge
                yield return new Point2D(centerSquare.Right, Bounds.Bottom);
                yield return new Point2D(centerSquare.Left, Bounds.Bottom);

                yield return new Point2D(centerSquare.Left, centerSquare.Bottom);

                // Left edge
                yield return new Point2D(Bounds.Left, centerSquare.Bottom);
                yield return new Point2D(Bounds.Left, centerSquare.Top);

                yield return new Point2D(centerSquare.Left, centerSquare.Top);
            }
        }
    }
}
