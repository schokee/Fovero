using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class CircularTilingEditor() : TilingEditor("Circular")
{
    private int _rings = 20;
    private int _segments = 16;

    public int Rings
    {
        get => _rings;
        set => SetFormat(ref  _rings, value);
    }

    public int Segments
    {
        get => _segments;
        set => SetFormat(ref _segments, value);
    }

    public override ITiling CreateTiling()
    {
        return new CircularTiling((ushort)Rings, (ushort)Segments);
    }
}
