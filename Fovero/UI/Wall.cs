using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Tiling;

namespace Fovero.UI;

public sealed class Wall(IEdge edge, decimal scaling) : PropertyChangedBase, IWall
{
    private bool _isOpen;

    public IEdge Edge { get; } = edge;

    public bool IsShared => Edge is SharedEdge;

    public decimal X1 => Edge.Start.X * scaling;

    public decimal Y1 => Edge.Start.Y * scaling;

    public decimal X2 => Edge.End.X * scaling;

    public decimal Y2 => Edge.End.Y * scaling;

    public string Geometry => $"M {X1},{Y1} {X2},{Y2}";

    public bool IsOpen
    {
        get => _isOpen;
        set => Set(ref _isOpen, value);
    }

    #region IWall

    public int NeighborA => Edge.Origin.Ordinal;

    public int NeighborB => (Edge as SharedEdge)?.OpposingTile.Ordinal ?? -1;

    #endregion
}
