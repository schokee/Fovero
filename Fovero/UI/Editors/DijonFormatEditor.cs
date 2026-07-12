using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class DijonFormatEditor() : RegularFormatEditor("Dijon")
{
    public bool HasVoids
    {
        get;
        set => SetFormat(ref field, value);
    }

    public override Maze CreateLayout()
    {
        return HasVoids
            ? new Maze(new DijonTiling((ushort)Columns, (ushort)Rows), edge => new Door(edge) { Color = "DimGray" })
            : new Maze(new DijonTiling((ushort)Columns, (ushort)Rows) { Mask = GridTiling.NoMask });
    }
}
