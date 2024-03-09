namespace Fovero.Model.Geometry;

public readonly struct Size2D(float width, float height)
{
    public float Width { get; } = MathF.Abs(width);

    public float Height { get; } = MathF.Abs(height);
}
