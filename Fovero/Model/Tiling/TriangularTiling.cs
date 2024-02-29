using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

// Rectangular Delta (Hexagonal Delta, Triangular Delta)
public sealed class TriangularTiling(ushort columns, ushort rows) : RegularTiling(columns, rows)
{
    private static float CellHeight { get; } = MathF.Sqrt(3) / 2;

    public override Rectangle Bounds => new Rectangle(0, 0, Columns / 2f, Rows * CellHeight).ToScaledUnits();

    protected override ITile CreateTile(int col, int row)
    {
        return new TriangleTile(this, col, row);
    }

    private record TriangleTile : ITile
    {
        private readonly TriangularTiling _format;
        private readonly int _column;
        private readonly int _row;
        private readonly bool _pointingUp;

        public TriangleTile(TriangularTiling format, int column, int row)
        {
            _format = format;
            _column = column;
            _row = row;
            _pointingUp = (column + row) % 2 == 0;
        }

        public ushort Ordinal => (ushort)(_row * _format.Columns + _column);

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds => new Rectangle(_column / 2f, _row * CellHeight, 1, CellHeight).ToScaledUnits();

        public IEnumerable<Point2D> CornerPoints
        {
            get
            {
                var bounds = Bounds;

                if (_pointingUp)
                {
                    yield return bounds.Center with { Y = bounds.Top };
                    yield return new Point2D(bounds.Right, bounds.Bottom);
                    yield return new Point2D(bounds.Left, bounds.Bottom);
                }
                else
                {
                    yield return new Point2D(bounds.Left, bounds.Top);
                    yield return new Point2D(bounds.Right, bounds.Top);
                    yield return bounds.Center with { Y = bounds.Bottom };
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
                    .Take(3)
                    .Select((segment, edge) =>
                    {
                        var location = new Location(_column, _row);
                        var neighbor = _pointingUp
                            ? edge switch
                            {
                                0 => location with { Column = _column + 1 },
                                1 => location with { Row = _row + 1 },
                                2 => location with { Column = _column - 1 },
                                _ => location
                            }
                            : edge switch
                            {
                                0 => location with { Row = _row - 1 },
                                1 => location with { Column = _column + 1 },
                                2 => location with { Column = _column - 1 },
                                _ => location
                            };

                        return _format.Contains(neighbor)
                            ? Edge.CreateShared(segment.Start, segment.End, this, new TriangleTile(_format, neighbor.Column, neighbor.Row))
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
