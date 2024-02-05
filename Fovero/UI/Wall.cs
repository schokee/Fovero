using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Tiling;

namespace Fovero.UI;

public sealed class Wall(ICanvas canvas, IEdge edge) : PropertyChangedBase, IWall
{
    private bool _isOpen;

    public ICanvas Canvas { get; } = canvas;

    public IEdge Edge { get; } = edge;

    public bool IsShared => Edge is SharedEdge;

    public double X1 => Edge.Start.X;

    public double Y1 => Edge.Start.Y;

    public double X2 => Edge.End.X;

    public double Y2 => Edge.End.Y;

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
