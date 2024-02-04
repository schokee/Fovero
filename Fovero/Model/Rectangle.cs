namespace Fovero.Model;

public record Rectangle(double X, double Y, double Width, double Height)
{
    public Rectangle(Point2D topLeft, Point2D bottomRight) : this(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y)
    {
    }

    public Point2D Center => new(X + Width / 2, Y + Height / 2);

    public double Left => X;
    public double Top => Y;

    public double Right => X + Width;
    public double Bottom => Y + Height;
}
