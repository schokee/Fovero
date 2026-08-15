using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public sealed class VoronoiFormatEditor : FormatEditor
{
    public VoronoiFormatEditor() : base("Voronoi")
    {
    }

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

    public int Variation
    {
        get;
        set => SetFormat(ref field, value);
    }

    public override Maze CreateLayout()
    {
        return new Maze(new VoronoiTiling((ushort)Columns, (ushort)Rows, (ushort)Variation));
    }
}
