﻿using MoreLinq;

namespace Fovero.Model.Tiling;

public class PyramidTiling(int height) : ITiling
{
    private static decimal CellHeight { get; } = (decimal)Math.Sqrt(3) / 2;

    public int Height { get; } = height;

    public IEnumerable<ITile> Generate()
    {
        return Enumerable
            .Range(0, Height)
            .SelectMany(r => Enumerable
                .Range(0, ColumnsInRow(r))
                .Select(c => new TriangleTile(this, c, r)));
    }

    public override string ToString()
    {
        return $"Pyramid ({Height} levels)";
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

    // Source: https://math.stackexchange.com/questions/2435816/a-formula-for-the-sum-of-the-triangular-numbers
    private static int TriangularSum(int row)
    {
        return row * (row + 1) * (row + 2) / 6;
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

        public int Ordinal => TriangularSum(_row) + _column - 1;

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds => new((_format.Height - _row + _column) / 2M, _row * CellHeight, 1, CellHeight);

        private IEnumerable<Point2D> CornerPoints
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
                    .Select((segment, n) =>
                    {
                        var location = new Location(_column, _row);
                        var neighbor = _pointingUp
                            ? n switch
                            {
                                0 => location with { Column = _column + 1 },
                                1 => location with { Row = _row + 1 },
                                2 => location with { Column = _column - 1 },
                                _ => location
                            }
                            : n switch
                            {
                                0 => location with { Row = _row - 1, Column = _column - 1 },
                                1 => location with { Column = _column + 1 },
                                2 => location with { Column = _column - 1 },
                                _ => location
                            };

                        var isSharedEdge = _format.Contains(neighbor);

                        return isSharedEdge
                            ? new SharedEdge(this, segment.Start, segment.End, new TriangleTile(_format, neighbor.Column, neighbor.Row))
                            : new Edge(this, segment.Start, segment.End);
                    });
            }
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }
    }
}