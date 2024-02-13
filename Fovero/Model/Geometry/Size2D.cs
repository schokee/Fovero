namespace Fovero.Model.Geometry;

public readonly struct Size2D(float width, float height)
{
    public float Width { get; } = Math.Abs(width);

    public float Height { get; } = Math.Abs(height);
}
