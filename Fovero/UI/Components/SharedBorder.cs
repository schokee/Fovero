using Fovero.Model;
using Fovero.Model.Tiling;

namespace Fovero.UI.Components;

public abstract class SharedBorder : Boundary, ISharedBorder
{
    private bool _isOpen;

    protected SharedBorder(IEdge edge) : base(edge)
    {
        if (!edge.IsShared)
        {
            throw new ArgumentException($"{nameof(SharedBorder)} requires shared edge");
        }
    }

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (Set(ref _isOpen, value))
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
