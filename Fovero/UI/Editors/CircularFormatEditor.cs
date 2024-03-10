using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class CircularFormatEditor() : FormatEditor("Circular")
{
    private int _rings = 20;
    private int _segments = 16;
    private bool _curved = true;
    private bool _adaptive = true;

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

    public bool Curved
    {
        get => _curved;
        set => SetFormat(ref _curved, value);
    }

    public bool Adaptive
    {
        get => _adaptive;
        set => SetFormat(ref _adaptive, value);
    }

    public override Maze CreateLayout()
    {
        return new Maze(Adaptive
            ? new AdaptiveCircularTiling((ushort)Rings, (ushort)Segments, Curved)
            : new SlicedCircularTiling((ushort)Rings, (ushort)Segments, Curved));
    }
}
