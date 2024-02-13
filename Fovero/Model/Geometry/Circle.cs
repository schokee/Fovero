namespace Fovero.Model.Geometry;

public readonly struct Circle : IEquatable<Circle>
{
    public Circle(float radius) : this(new Point2D(), radius)
    {
    }

    public Circle(Point2D center, float radius)
    {
        if (radius < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Radius));
        }

        Center = center;
        Radius = radius;
    }

    public float Diameter => Radius * 2;

    public float Radius { get; }

    public Point2D Center { get; }

    public Point2D PointAt(Angle angle)
    {
        return new Point2D(
            Center.X + Radius * angle.Sin(),
            Center.Y - Radius * angle.Cos());
    }

    public Rectangle Bounds => new(Center.X - Radius, Center.Y - Radius, Diameter, Diameter);

    public Size2D Size2D => new(Diameter, Diameter);

    public Size2D QuadrantSize2D => new(Radius, Radius);

    public Circle GrowBy(float offset)
    {
        return new Circle(Center, Radius + offset);
    }

    public Circle ReduceBy(float offset)
    {
        return new Circle(Center, Math.Max(0, Radius - offset));
    }

    public static Circle CenterWithin(Size2D bounds)
    {
        return new Circle(new Point2D(bounds.Width / 2, bounds.Height / 2), bounds.SmallestSize() / 2);
    }

    public static Circle CenterWithin(Rectangle bounds)
    {
        return new Circle(bounds.Center, bounds.SmallestSize() / 2);
    }

    public bool Equals(Circle other)
    {
        return Radius.Equals(other.Radius) && Center.Equals(other.Center);
    }

    public override bool Equals(object obj)
    {
        return obj is Circle other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Radius.GetHashCode() * 397 ^ Center.GetHashCode();
        }
    }

    public static bool operator ==(Circle left, Circle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Circle left, Circle right)
    {
        return !left.Equals(right);
    }
}
