using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class CircularFormatEditor() : FormatEditor("Circular")
{
    public int Rings
    {
        get;
        set => SetFormat(ref field, value);
    } = 20;

    public int Segments
    {
        get;
        set => SetFormat(ref field, value);
    } = 16;

    public bool Curved
    {
        get;
        set => SetFormat(ref field, value);
    } = true;

    public bool Adaptive
    {
        get;
        set => SetFormat(ref field, value);
    } = true;

    public override Maze CreateLayout()
    {
        return new Maze(Adaptive
            ? new AdaptiveCircularTiling((ushort)Rings, (ushort)Segments, Curved)
            : new SlicedCircularTiling((ushort)Rings, (ushort)Segments, Curved));
    }
}
