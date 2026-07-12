using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class RegularFormatEditor : FormatEditor
{
    public RegularFormatEditor(string name, Func<ushort, ushort, ITiling> createTiling) : this(name)
    {
        TilingMethod = createTiling;
    }

    protected RegularFormatEditor(string name) : base(name)
    {
    }

    protected Func<ushort, ushort, ITiling> TilingMethod { get; init; }

    public int Columns
    {
        get;
        set => SetFormat(ref field, value);
    }

    public int Rows
    {
        get;
        set => SetFormat(ref field, value);
    }

    public override Maze CreateLayout()
    {
        return new Maze(TilingMethod((ushort)Columns, (ushort)Rows));
    }
}
