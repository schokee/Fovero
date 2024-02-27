namespace Fovero.Model.Geometry;

public static class GeometryExtensions
{
    public static float SmallestSize(this Size2D size)
    {
        return Math.Min(size.Width, size.Height);
    }

    public static float LargestSize(this Size2D size)
    {
        return Math.Max(size.Width, size.Height);
    }

    public static float SmallestSize(this Rectangle rectangle)
    {
        return Math.Min(rectangle.Width, rectangle.Height);
    }

    public static Point2D Min(this Point2D p1, Point2D p2)
    {
        return new Point2D(MathF.Min(p1.X, p2.X), MathF.Min(p1.Y, p2.Y));
    }

    public static Point2D Max(this Point2D p1, Point2D p2)
    {
        return new Point2D(MathF.Max(p1.X, p2.X), MathF.Max(p1.Y, p2.Y));
    }

    public static Rectangle Union(this IEnumerable<Point2D> points)
    {
        return points.Aggregate(new Rectangle(), (result, p) => new Rectangle(result.TopLeft.Min(p), result.BottomRight.Max(p)));
    }

    public static Rectangle ReduceBy(this Rectangle rectangle, float size)
    {
        return rectangle.ReduceBy(new Size2D(size, size));
    }

    public static Rectangle ReduceBy(this Rectangle rectangle, Size2D offset)
    {
        return new Rectangle(rectangle.TopLeft + offset, rectangle.BottomRight - offset);
    }

    public static Rectangle CenteredAt(this Rectangle rectangle, Point2D center)
    {
        return rectangle with { X = center.X - rectangle.Width / 2, Y = center.Y - rectangle.Height / 2 };
    }
}
