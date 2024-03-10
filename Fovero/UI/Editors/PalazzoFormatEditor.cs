using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class PalazzoFormatEditor() : FormatEditor("Palazzo")
{
    private int _columns;
    private int _rows;
    private bool _hasVoids = true;

    public int Columns
    {
        get => _columns;
        set => SetFormat(ref _columns, value);
    }

    public int Rows
    {
        get => _rows;
        set => SetFormat(ref _rows, value);
    }

    public bool HasVoids
    {
        get => _hasVoids;
        set => SetFormat(ref _hasVoids, value);
    }

    public override Maze CreateLayout()
    {
        return HasVoids
            ? new Maze(new PalazzoTiling((ushort)Columns, (ushort)Rows), edge => new Door(edge) { Color = "DimGray" })
            : new Maze(new PalazzoTiling((ushort)Columns, (ushort)Rows) { Mask = (_, _) => false});
    }
}
