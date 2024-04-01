using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public sealed class LatticeFormatEditor() : RegularFormatEditor("Lattice")
{
    public override Maze CreateLayout()
    {
        return new Maze(new LatticeTiling((ushort)Columns, (ushort)Rows), edge => new Door(edge) { Color = "DimGray" });
    }
}
