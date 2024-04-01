using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class PalazzoFormatEditor() : RegularFormatEditor("Palazzo")
{
    private bool _hasVoids = true;

    public bool HasVoids
    {
        get => _hasVoids;
        set => SetFormat(ref _hasVoids, value);
    }

    public override Maze CreateLayout()
    {
        return HasVoids
            ? new Maze(new PalazzoTiling((ushort)Columns, (ushort)Rows), edge => new Door(edge) { Color = "DimGray" })
            : new Maze(new PalazzoTiling((ushort)Columns, (ushort)Rows) { Mask = GridTiling.NoMask });
    }
}
