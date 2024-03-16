using Fovero.Model.Tiling;

namespace Fovero.Model.Presentation;

public abstract class Boundary : Bindable
{
    protected Boundary(IEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge, nameof(edge));
        Edge = edge;
    }

    public IEdge Edge { get; }

    public virtual string Geometry => Edge.PathData;
}
