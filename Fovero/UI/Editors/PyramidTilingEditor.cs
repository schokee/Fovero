using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class PyramidTilingEditor() : TilingEditor("Pyramid")
{
    private int _rows = 10;

    public int Rows
    {
        get => _rows;
        set => SetFormat(ref _rows, value);
    }

    public override ITiling CreateTiling()
    {
        return new PyramidTiling((ushort)Rows);
    }
}
