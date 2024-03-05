using Fovero.Model.Geometry;
using MoreLinq;

namespace Fovero.Model.Tiling;

public sealed class AdaptiveCircularTiling : CircularTiling
{
    private IReadOnlyList<int> SegmentsAtRing { get; }

    public AdaptiveCircularTiling(ushort rings, ushort segments, bool curved) : base(rings, segments, curved)
    {
        var segmentSweep = SegmentSweep;

        SegmentsAtRing = Enumerable
            .Range(1, rings)
            .Select(ring => (int)(Segments * MathF.Pow(2, MathF.Truncate(MathF.Log2(segmentSweep.Radians * ring)))))
            .Prepend(0)
            .ToList();
    }

    public override IEnumerable<ITile> Generate()
    {
        var allLocations = Enumerable
            .Range(1, Rings)
            .SelectMany(ring => Enumerable
                .Range(0, SegmentsAtRing[ring])
                .Select(segment => new Location(ring, segment)));

        var lookup = new Dictionary<Location, ITile>();
        ushort ordinal = 0;

        foreach (var location in allLocations)
        {
            lookup.Add(location, new ArcTile(this, lookup, ordinal++, location));
        }

        return lookup.Values;
    }

    private record ArcTile : ITile
    {
        private readonly IReadOnlyDictionary<Location, ITile> _lookup;

        public ArcTile(AdaptiveCircularTiling format, IReadOnlyDictionary<Location, ITile> lookup, ushort ordinal, Location location)
        {
            _lookup = lookup;

            Format = format;
            Ordinal = ordinal;
            Ring = (ushort)location.Ring;
            Segment = (ushort)location.Segment;

            var expandBy = new Size2D(0.3f, 0.3f).ToScaledUnits();

            Center = Format.CircleAt(Ring + 0.5f).PointAt(SegmentSweep * (Segment + 0.5f));
            Bounds = new Rectangle(Center - expandBy, Center + expandBy);
        }

        public ushort Ordinal { get; }

        public Point2D Center { get; }

        public Rectangle Bounds { get; }

        public IEnumerable<IEdge> Edges
        {
            get
            {
                var location = new Location(Ring, Segment);
                var isOuterEdgeSplit = IsOuterEdgeSplit;
                var isInnerEdgeSplit = IsInnerEdgeSplit;

                return CornerPoints
                    .Repeat()
                    .Pairwise((start, end) => (Start: start, End: end))
                    .Take(isOuterEdgeSplit ? 5 : 4)
                    .Select((segment, edge) =>
                    {
                        var locationOfNeighbor = isOuterEdgeSplit
                            ? edge switch
                            {
                                0 => location with { Ring = (ushort)(Ring + 1), Segment = Segment * 2 },
                                1 => location with { Ring = (ushort)(Ring + 1), Segment = Segment * 2 + 1 },
                                2 => location with { Segment = NextSegment },
                                3 when isInnerEdgeSplit => location with { Ring = (ushort)(Ring - 1), Segment = Segment / 2 },
                                3 => location with { Ring = (ushort)(Ring - 1) },
                                4 => location with { Segment = PreviousSegment },
                                _ => location
                            }
                            : edge switch
                            {
                                0 => location with { Ring = (ushort)(Ring + 1) },
                                1 => location with { Segment = NextSegment },
                                2 when isInnerEdgeSplit => location with { Ring = (ushort)(Ring - 1), Segment = Segment / 2 },
                                2 => location with { Ring = (ushort)(Ring - 1) },
                                3 => location with { Segment = PreviousSegment },
                                _ => location
                            };

                        var result = _lookup.TryGetValue(locationOfNeighbor, out var neighbor)
                            ? Edge.CreateShared(segment.Start, segment.End, this, neighbor)
                            : Edge.CreateBorder(segment.Start, segment.End, this);

                        if (Format.Curved)
                        {
                            if (isOuterEdgeSplit)
                            {
                                switch (edge)
                                {
                                    case 0:
                                    case 1:
                                        result.DrawData = OuterArcMarkup();
                                        break;
                                    case 3:
                                        result.DrawData = InnerArcMarkup();
                                        break;
                                }
                            }
                            else
                            {
                                switch (edge)
                                {
                                    case 0:
                                        result.DrawData = OuterArcMarkup();
                                        break;
                                    case 2:
                                        result.DrawData = InnerArcMarkup();
                                        break;
                                }
                            }

                            string OuterArcMarkup() => ArcMarkup(Ring + 1, segment.End, true);
                            string InnerArcMarkup() => ArcMarkup(Ring, segment.End, false);
                        }

                        return result;
                    });
            }
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }

        private AdaptiveCircularTiling Format { get; }
        private ushort Ring { get; }
        private ushort Segment { get; }

        private ushort Segments => (ushort)Format.SegmentsAtRing[Ring];
        private Angle SegmentSweep => Angle.FromDegrees((float)Degrees.PerCircle / Segments);

        private ushort NextSegment => (ushort)((Segment + 1) % Segments);
        private ushort PreviousSegment => (ushort)((Segment > 0 ? Segment : Segments) - 1);

        private bool IsOuterEdgeSplit => Ring + 1 < Format.Rings && Segments < Format.SegmentsAtRing[Ring + 1];
        private bool IsInnerEdgeSplit => Ring > 1 && Segments > Format.SegmentsAtRing[Ring - 1];

        private IEnumerable<Point2D> CornerPoints
        {
            get
            {
                var segmentSweep = SegmentSweep;

                var outerArc = Format.CircleAt(Ring + 1);

                yield return outerArc.PointAt(segmentSweep * Segment);

                if (IsOuterEdgeSplit)
                {
                    yield return outerArc.PointAt(segmentSweep * (Segment + 0.5f));
                }

                var nextSegment = NextSegment;

                yield return outerArc.PointAt(segmentSweep * nextSegment);

                var innerArc = Format.CircleAt(Ring);

                yield return innerArc.PointAt(segmentSweep * nextSegment);
                yield return innerArc.PointAt(segmentSweep * Segment);
            }
        }
    }
}
