using MoreLinq;

namespace Fovero.Model.Tiling;

public class TruncatedSquareTiling(ushort columns, ushort rows) : RegularTiling("Truncated Square Tile", columns, rows)
{
    protected override ITile CreateTile(int col, int row)
    {
        return new Tile(this, col, row);
    }

    private record Tile : ITile
    {
        private readonly TruncatedSquareTiling _format;
        private readonly int _column;
        private readonly int _row;
        private readonly bool _isSquare;

        public Tile(TruncatedSquareTiling format, int column, int row)
        {
            _format = format;
            _column = column;
            _row = row;
            _isSquare = (column + row) % 2 == 1;
        }

        public ushort Ordinal => (ushort)(_row * _format.Rows + _column);

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds
        {
            get
            {
                var c = (_column >> 1) * 4f;
                var r = (_row >> 1) * 4f;

                return _isSquare
                    ? new(c + (_column % 2 == 0 ? 1 : 3), r + (_row % 2 == 0 ? 1 : 3), 1, 1)
                    : new(c, r, 3, 3);
            }
        }

        private IEnumerable<Point2D> CornerPoints
        {
            get
            {
                var bounds = Bounds;

                if (_isSquare)
                {
                    yield return new Point2D(bounds.Left, bounds.Top);
                    yield return new Point2D(bounds.Right, bounds.Top);
                    yield return new Point2D(bounds.Right, bounds.Bottom);
                    yield return new Point2D(bounds.Left, bounds.Bottom);
                }
                else
                {
                    yield return new Point2D(bounds.Left + 1, bounds.Top);
                    yield return new Point2D(bounds.Left + 2, bounds.Top);
                    yield return new Point2D(bounds.Right, bounds.Top + 1);
                    yield return new Point2D(bounds.Right, bounds.Top + 2);
                    yield return new Point2D(bounds.Left + 2, bounds.Bottom);
                    yield return new Point2D(bounds.Left + 1, bounds.Bottom);
                    yield return new Point2D(bounds.Left, bounds.Top + 2);
                    yield return new Point2D(bounds.Left, bounds.Top + 1);
                }
            }
        }

        public IEnumerable<IEdge> Edges
        {
            get
            {
                return CornerPoints
                    .Repeat()
                    .Pairwise((start, end) => (Start: start, End: end))
                    .Take(_isSquare ? 4 : 8)
                    .Select((segment, edge) =>
                    {
                        var location = new Location(_column, _row);
                        var neighbor = _isSquare
                            ? edge switch
                            {
                                0 => location with { Row = _row - 1 },
                                1 => location with { Column = _column + 1 },
                                2 => location with { Row = _row + 1 },
                                3 => location with { Column = _column - 1 },
                                _ => location
                            }
                            : edge switch
                            {
                                0 => location with { Row = _row - 1 },
                                2 => location with { Column = _column + 1 },
                                4 => location with { Row = _row + 1 },
                                6 => location with { Column = _column - 1 },
                                1 => new Location(_column + 1, _row - 1),
                                3 => new Location(_column + 1, _row + 1),
                                5 => new Location(_column - 1, _row + 1),
                                7 => new Location(_column - 1, _row - 1),
                                _ => location
                            };

                        return _format.Contains(neighbor)
                            ? Edge.CreateShared(segment.Start, segment.End, this, Create(neighbor))
                            : Edge.CreateBorder(segment.Start, segment.End, this);
                    });
            }
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }

        private Tile Create(Location location)
        {
            return new Tile(_format, location.Column, location.Row);
        }
    }
}
