using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Tiling;

namespace Fovero.UI;

public sealed class Wall(IEdge edge) : PropertyChangedBase, ISharedWall
{
    private bool _isOpen;
    private bool _isLocked;

    public IEdge Edge { get; } = edge;

    public string Geometry => Edge.PathData;

    public bool IsShared => Edge.IsShared;

    public bool IsOpen
    {
        get => _isOpen;
        set => Set(ref _isOpen, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            if (IsShared)
            {
                Set(ref _isLocked, value);
            }
        }
    }

    public IEnumerable<(T From, T To)> SelectPathwaysFrom<T>(T source, Func<ushort, T> createNode)
    {
        if (!IsShared)
        {
            yield break;
        }

        var p1 = createNode(Neighbors.ElementAt(0));
        var p2 = createNode(Neighbors.ElementAt(1));

        if (Equals(source, p1))
        {
            yield return (source, p2);
        }
        else if (Equals(source, p2))
        {
            yield return (source, p1);
        }
    }

    #region ISharedWall

    public IEnumerable<ushort> Neighbors => Edge.Neighbors.Select(x => x.Ordinal);

    #endregion
}
