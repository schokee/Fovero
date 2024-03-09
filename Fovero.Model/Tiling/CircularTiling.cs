using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling;

public abstract class CircularTiling(ushort rings, ushort segments, bool curved) : ITiling
{
    public ushort Rings { get; } = rings;

    public ushort Segments { get; } = segments;

    public bool Curved { get; } = curved;

    public Rectangle Bounds => new Rectangle(0, 0, Rings + 1, Rings + 1).ToScaledUnits();

    public abstract IEnumerable<ITile> Generate();

    protected Angle SegmentSweep { get; } = Angle.FromDegrees((float)Degrees.PerCircle / segments);

    protected Circle CircleAt(float ring) => new(Bounds.Center, ring * Scaling.Unit);

    protected Point2D TopLeftPointOf(ushort ring, ushort segment) => CircleAt(ring).PointAt(SegmentSweep * segment);

    protected readonly struct Location(int ring, int segment)
    {
        public int Ring { get; init; } = ring;
        public int Segment { get; init; } = segment;
    }

    protected static string ArcMarkup(int ring, Point2D end, bool clockwiseSweep)
    {
        return $"A {Scaling.Unit * ring} {Scaling.Unit * ring} 0 0 {(clockwiseSweep ? 1 : 0)} {end.X},{end.Y}";
    }
}
