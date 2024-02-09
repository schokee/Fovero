using MoreLinq;

namespace Fovero.Model.Tiling;

public class PyramidTiling(ushort height) : ITiling
{
    private static float CellHeight { get; } = MathF.Sqrt(3) / 2;

    public ushort Height { get; } = height;

    public Rectangle Bounds => new(0, 0, Height, CellHeight * Height);

    public IEnumerable<ITile> Generate()
    {
        return Enumerable
            .Range(0, Height)
            .SelectMany(r => Enumerable
                .Range(0, ColumnsInRow(r))
                .Select(c => new TriangleTile(this, c, r)));
    }

    protected bool Contains(Location location)
    {
        return location.Row >= 0 && location.Row < Height && location.Column >= 0 && location.Column < ColumnsInRow(location.Row);
    }

    protected readonly struct Location(int column, int row)
    {
        public int Column { get; init; } = column;
        public int Row { get; init; } = row;
    }

    private static int ColumnsInRow(int row)
    {
        return row * 2 + 1;
    }

    private record TriangleTile : ITile
    {
        private readonly PyramidTiling _format;
        private readonly int _column;
        private readonly int _row;
        private readonly bool _pointingUp;

        public TriangleTile(PyramidTiling format, int column, int row)
        {
            _format = format;
            _column = column;
            _row = row;
            _pointingUp = _column % 2 == 0;
        }

        public ushort Ordinal => (ushort)(_row * _row + _column);

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds => new((_format.Height - _row + _column) / 2f, _row * CellHeight, 1, CellHeight);

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
                                1 => location with { Row = _row + 1, Column = _column + 1},
                                2 => location with { Column = _column - 1 },
                                _ => location
                            }
                            : edge switch
                            {
                                0 => location with { Row = _row - 1, Column = _column - 1 },
                                1 => location with { Column = _column + 1 },
                                2 => location with { Column = _column - 1 },
                                _ => location
                            };

                        var isSharedEdge = _format.Contains(neighbor);

                        return isSharedEdge
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
