using Fovero.Model.Tiling;

namespace Fovero.UI.Components;

public sealed class Door(IEdge edge) : SharedBorder(edge)
{
    public string Color { get; set; } = "#F02090FF";
}

