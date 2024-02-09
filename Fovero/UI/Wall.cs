using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;

namespace Fovero.UI;

public sealed class Wall(ICanvas canvas, IEdge edge) : PropertyChangedBase, ISharedWall, IWall
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

    #region ISharedWall

    public IEnumerable<ushort> Neighbors => Edge.Neighbors.Select(x => x.Ordinal);

    #endregion
}
