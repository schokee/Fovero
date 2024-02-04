namespace Fovero.Model;

public record Point2D(double X, double Y) : IComparable<Point2D>
{
    public Point2D MidpointTo(Point2D endPoint)
    {
        return new Point2D((X + endPoint.X) / 2, (Y + endPoint.Y) / 2);
    }

    public int CompareTo(Point2D other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;

        var diff = Y.CompareTo(other.Y);
        return diff != 0 ? diff : X.CompareTo(other.X);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static bool operator <(Point2D left, Point2D right)
    {
        return Comparer<Point2D>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(Point2D left, Point2D right)
    {
        return Comparer<Point2D>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(Point2D left, Point2D right)
    {
        return Comparer<Point2D>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(Point2D left, Point2D right)
    {
        return Comparer<Point2D>.Default.Compare(left, right) >= 0;
    }
}
