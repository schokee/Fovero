namespace Fovero.Model;

public record Rectangle(float X, float Y, float Width, float Height)
{
    public Rectangle() : this(0, 0, 0, 0)
    {
    }

    public Rectangle(Point2D topLeft, Point2D bottomRight) : this(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y)
    {
    }

    public Point2D Center => new(X + Width / 2, Y + Height / 2);

    public float Left => X;
    public float Top => Y;

    public float Right => X + Width;
    public float Bottom => Y + Height;
}
