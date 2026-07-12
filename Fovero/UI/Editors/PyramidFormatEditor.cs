using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class PyramidFormatEditor() : FormatEditor("Pyramid")
{
    public int Rows
    {
        get;
        set => SetFormat(ref field, value);
    } = 10;

    public override Maze CreateLayout()
    {
        return new Maze(new PyramidTiling((ushort)Rows));
    }
}
