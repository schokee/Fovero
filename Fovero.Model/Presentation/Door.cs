using Fovero.Model.Tiling;

namespace Fovero.Model.Presentation;

public sealed class Door(IEdge edge) : SharedBorder(edge)
{
    public string Color { get; set; } = "#F02090FF";
}

