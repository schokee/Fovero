using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Tiling;

namespace Fovero.UI;

public sealed class Wall(ICanvas canvas, IEdge edge) : PropertyChangedBase, ISharedWall
{
    private bool _isOpen;

    public ICanvas Canvas { get; } = canvas;

    public IEdge Edge { get; } = edge;

    public bool IsShared => Edge.IsShared;

    public float X1 => Edge.Start.X;

    public float Y1 => Edge.Start.Y;

    public float X2 => Edge.End.X;

    public float Y2 => Edge.End.Y;

    public string Geometry => $"M {X1},{Y1} {X2},{Y2}";

    public bool IsOpen
    {
        get => _isOpen;
        set => Set(ref _isOpen, value);
    }

    public IEnumerable<(T From, T To)> SelectPathways<T>(Func<ushort, T> createNode)
    {
        if (!IsShared)
        {
            yield break;
        }

        var p1 = createNode(Neighbors.ElementAt(0));
        var p2 = createNode(Neighbors.ElementAt(1));

        yield return (p1, p2);
        yield return (p2, p1);
    }

    #region ISharedWall

    public IEnumerable<ushort> Neighbors => Edge.Neighbors.Select(x => x.Ordinal);

    #endregion
}
