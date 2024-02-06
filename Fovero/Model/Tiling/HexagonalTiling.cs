using MoreLinq;

namespace Fovero.Model.Tiling;

public class HexagonalTiling(ushort columns, ushort rows) : RegularTiling("Hexagonal", columns, rows)
{
    private static float CellHeight { get; } = MathF.Sqrt(3);

    protected override ITile CreateTile(int col, int row)
    {
        return new HexagonalTile(this, col, row);
    }

    private record HexagonalTile : ITile
    {
        private readonly HexagonalTiling _format;
        private readonly int _column;
        private readonly int _row;
        private readonly bool _isEvenColumn;

        public HexagonalTile(HexagonalTiling format, int column, int row)
        {
            _format = format;
            _column = column;
            _row = row;
            _isEvenColumn = _column % 2 == 0;
        }

        public ushort Ordinal => (ushort)(_row * _format.Rows + _column);

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds => new(_column * 1.5f, _row * CellHeight + (_isEvenColumn ? 0 : CellHeight / 2f), 2, CellHeight);

        private IEnumerable<Point2D> CornerPoints
        {
            get
            {
                var bounds = Bounds;

                yield return new Point2D(bounds.Left + 0.5f, bounds.Top);
                yield return new Point2D(bounds.Right - 0.5f, bounds.Top);
                yield return bounds.Center with { X = bounds.Right };
                yield return new Point2D(bounds.Right - 0.5f, bounds.Bottom);
                yield return new Point2D(bounds.Left + 0.5f, bounds.Bottom);
                yield return bounds.Center with { X = bounds.Left };
            }
        }

        public IEnumerable<IEdge> Edges
        {
            get
            {
                return CornerPoints
                    .Repeat()
                    .Pairwise((start, end) => (Start: start, End: end))
                    .Take(6)
                    .Select((x, n) =>
                    {
                        var location = new Location(_column, _row);
                        var neighbor = n switch
                        {
                            0 => location with { Row = _row - 1 },
                            1 when _isEvenColumn => location with { Column = _column + 1, Row = _row - 1 },
                            1 => location with { Column = _column + 1 },
                            2 when _isEvenColumn => location with { Column = _column + 1 },
                            2 => location with { Column = _column + 1, Row = _row + 1 },

                            3 => location with { Row = _row + 1 },
                            4 when _isEvenColumn => location with { Column = _column - 1 },
                            4 => location with { Column = _column - 1, Row = _row + 1 },
                            5 when _isEvenColumn => location with { Column = _column - 1, Row = _row - 1 },
                            5 => location with { Column = _column - 1 },
                            _ => location
                        };

                        return _format.Contains(neighbor)
                            ? Edge.CreateShared(x.Start, x.End, this, new HexagonalTile(_format, neighbor.Column, neighbor.Row))
                            : Edge.CreateBorder(x.Start, x.End, this);
                    });
            }
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }
    }
}
