namespace Fovero.Model;

public readonly struct Point2D(float x, float y)
{
    public float X { get; init; } = x;

    public float Y { get; init; } = y;

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
}
