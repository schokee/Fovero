namespace Fovero.Model.Tiling;

public sealed class SharedEdge(ITile origin, Point2D start, Point2D end, ITile opposingTile) : Edge(origin, start, end)
{
    public ITile OpposingTile { get; } = opposingTile;
}
