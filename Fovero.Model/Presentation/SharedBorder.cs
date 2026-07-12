using Fovero.Model.Tiling;

namespace Fovero.Model.Presentation;

public abstract class SharedBorder : Boundary, ISharedBorder
{
    protected SharedBorder(IEdge edge) : base(edge)
    {
        if (!edge.IsShared)
        {
            throw new ArgumentException($"{nameof(SharedBorder)} requires shared edge");
        }
    }

    public bool IsOpen
    {
        get;
        set
        {
            if (Set(ref field, value))
            {
                OnStateChanged();
            }
        }
    }

    #region ISharedBorder

    public IEnumerable<ushort> Neighbors => Edge.Neighbors.Select(x => x.Ordinal);

    #endregion

    protected virtual void OnStateChanged()
    {
    }
}
