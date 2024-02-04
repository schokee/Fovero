namespace Fovero.Model;

public record Rectangle(decimal X, decimal Y, decimal Width, decimal Height)
{
    public Rectangle(Point2D topLeft, Point2D bottomRight) : this(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y)
    {
    }

    public Point2D Center => new(X + Width / 2, Y + Height / 2);

    public decimal Left => X;
    public decimal Top => Y;

    public decimal Right => X + Width;
    public decimal Bottom => Y + Height;
}
