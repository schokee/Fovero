﻿using MoreLinq;

namespace Fovero.Model.Tiling;

public class SquareTiling(int columns, int rows) : RegularTiling("Square", columns, rows)
{
    protected override ITile CreateTile(int col, int row)
    {
        return new SquareTile(this, col, row);
    }

    private record SquareTile : ITile
    {
        private readonly SquareTiling _format;
        private readonly int _column;
        private readonly int _row;

        public SquareTile(SquareTiling format, int column, int row)
        {
            _format = format;
            _column = column;
            _row = row;
        }

        public int Ordinal => _row * _format.Rows + _column;

        public Point2D Center => Bounds.Center;

        public Rectangle Bounds => new(_column, _row, 1, 1);

        private IEnumerable<Point2D> CornerPoints
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
                    .Select((segment, n) =>
                    {
                        var location = new Location(_column, _row);
                        var neighbor = n switch
                        {
                            0 => location with { Row = _row - 1 },
                            1 => location with { Column = _column + 1 },
                            2 => location with { Row = _row + 1 },
                            3 => location with { Column = _column - 1 },
                            _ => location
                        };

                        return _format.Contains(neighbor)
                            ? new SharedEdge(this, segment.Start, segment.End, new SquareTile(_format, neighbor.Column, neighbor.Row))
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