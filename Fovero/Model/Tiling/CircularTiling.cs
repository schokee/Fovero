using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public sealed class CircularTiling(ushort rings, ushort segments, bool curved) : ITiling
{
    public ushort Rings { get; } = rings;

    public ushort Segments { get; } = segments;

    public bool Curved { get; } = curved;

    public Rectangle Bounds => new(0, 0, Rings + 1, Rings + 1);

    public IEnumerable<ITile> Generate()
    {
        return Enumerable
            .Range(1, Rings)
            .SelectMany(ring => Enumerable
                .Range(0, Segments)
                .Select(segment => CreateTile(ring, segment)));
    }

    private ArcTile CreateTile(int ring, int segment)
    {
        return new ArcTile(this, (ushort)ring, (ushort)segment);
    }

    private bool Contains(Location location)
    {
        return location.Ring > 0 && location.Ring <= Rings && location.Segment >= 0 && location.Segment < Segments;
    }

    private Angle SegmentSweep { get; } = Angle.FromDegrees((float)Degrees.PerCircle / segments);

    private Point2D TopLeftPointOf(ushort ring, ushort segment)
    {
        var result = new Circle(Bounds.Center, ring).PointAt(SegmentSweep * segment);
        return result;
    }

    private record ArcTile : ITile
    {
        private readonly CircularTiling _format;
        private readonly ushort _ring;
        private readonly ushort _segment;

        public ArcTile(CircularTiling format, ushort ring, ushort segment)
        {
            _format = format;
            _ring = ring;
            _segment = segment;

            var expandBy = new Size2D(0.3f, 0.3f);

            Center = new Circle(_format.Bounds.Center, ring + 0.5f).PointAt(_format.SegmentSweep * (segment + 0.5f));
            Bounds = new Rectangle(Center - expandBy, Center + expandBy);
        }

        public ushort Ordinal => (ushort)((_ring - 1) * _format.Segments + _segment);

        public Point2D Center { get; }

        public Rectangle Bounds { get; }

        public IEnumerable<Point2D> CornerPoints
        {
            get
            {
                var outerRing = (ushort)(_ring + 1);
                var nextSegment = NextSegment;

                yield return _format.TopLeftPointOf(outerRing, _segment);
                yield return _format.TopLeftPointOf(outerRing, nextSegment);
                yield return _format.TopLeftPointOf(_ring, nextSegment);
                yield return _format.TopLeftPointOf(_ring, _segment);
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
                        var location = new Location(_ring, _segment);
                        var neighbor = edge switch
                            {
                                0 => location with { Ring = (ushort)(_ring + 1) },
                                1 => location with { Segment = NextSegment },
                                2 => location with { Ring = (ushort)(_ring - 1) },
                                3 => location with { Segment = PreviousSegment },
                                _ => location
                            };

                        var result = _format.Contains(neighbor)
                            ? Edge.CreateShared(segment.Start, segment.End, this, _format.CreateTile(neighbor.Ring, neighbor.Segment))
                            : Edge.CreateBorder(segment.Start, segment.End, this);

                        switch (edge)
                        {
                            case 0 when _format.Curved:
                                result.DrawData = $"A {_ring + 1} {_ring + 1} 0 0 1 {segment.End.X},{segment.End.Y}";
                                return result;
                            case 2 when _format.Curved:
                                result.DrawData = $"A {_ring} {_ring} 0 0 0 {segment.End.X},{segment.End.Y}";
                                return result;
                            default:
                                return result;
                        }
                    });
            }
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }

        private ushort NextSegment => (ushort)((_segment + 1) % _format.Segments);

        private ushort PreviousSegment => (ushort)((_segment > 0 ? _segment : _format.Segments) - 1);
    }

    private readonly struct Location(int ring, int segment)
    {
        public int Ring { get; init; } = ring;
        public int Segment { get; init; } = segment;
    }
}
