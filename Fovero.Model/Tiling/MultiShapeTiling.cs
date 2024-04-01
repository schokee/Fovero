namespace Fovero.Model.Tiling;

public abstract class MultiShapeTiling : GridTiling
{
    protected MultiShapeTiling(ushort columns, ushort rows, float spacing = 0.7f) : base((ushort)(columns * 2 - 1), (ushort)(rows * 2 - 1))
    {
        Spacing = spacing;
        Mask  = (c, r) => c % 2 == 1 && r % 2 == 1;
    }

    protected float Spacing { get; }

    protected override float OffsetAt(int n)
    {
        return (1 + Spacing) * (n >> 1) + n % 2;
    }
}
