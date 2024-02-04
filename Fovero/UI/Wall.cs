using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Tiling;

namespace Fovero.UI;

public sealed class Wall(IEdge edge, double scaling) : PropertyChangedBase, IWall
{
    private bool _isOpen;

    public IEdge Edge { get; } = edge;

    public bool IsShared => Edge is SharedEdge;

    public double X1 => Edge.Start.X * scaling;

    public double Y1 => Edge.Start.Y * scaling;

    public double X2 => Edge.End.X * scaling;

    public double Y2 => Edge.End.Y * scaling;

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
