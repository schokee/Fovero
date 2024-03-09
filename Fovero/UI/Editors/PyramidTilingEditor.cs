using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class PyramidFormatEditor() : FormatEditor("Pyramid")
{
    private int _rows = 10;

    public int Rows
    {
        get => _rows;
        set => SetFormat(ref _rows, value);
    }

    public override Maze CreateLayout()
    {
        return new Maze(new PyramidTiling((ushort)Rows));
    }
}
