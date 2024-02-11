namespace Fovero.Model.Geometry;

public readonly struct Point2D(float x, float y)
{
    public Point2D() : this(0, 0)
    {
    }

    public float X { get; init; } = x;

    public float Y { get; init; } = y;

    public float ManhattanDistanceTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;

        return Math.Abs(dx) + Math.Abs(dy);
    }

    public float EuclidianDistanceTo(Point2D other)
    {
        return MathF.Sqrt(SquaredDistanceTo(other));
    }

    public float SquaredDistanceTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;

        return dx * dx + dy * dy;
    }

    public Point2D MidPointTo(Point2D other)
    {
        return new Point2D((X + other.X) / 2, (Y + other.Y) / 2);
    }

    public Point2D ScaledBy(float factor)
    {
        return new Point2D(X * factor, Y * factor);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static Point2D operator -(Point2D p)
    {
        return new Point2D(-p.X, -p.Y);
    }

    public static Point2D operator +(Point2D p, Size2D o)
    {
        return new Point2D(p.X + o.Width, p.Y + o.Height);
    }

    public static Point2D operator -(Point2D p, Size2D o)
    {
        return new Point2D(p.X - o.Width, p.Y - o.Height);
    }

    public static implicit operator Size2D(Point2D p)
    {
        return new Size2D(p.X, p.Y);
    }
}
