using Caliburn.Micro;
using Fovero.Model.Tiling;

namespace Fovero.UI.Components;

public abstract class Boundary : PropertyChangedBase
{
    protected Boundary(IEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge, nameof(edge));
        Edge = edge;
    }

    public IEdge Edge { get; }

    public virtual string Geometry => Edge.PathData;
}
