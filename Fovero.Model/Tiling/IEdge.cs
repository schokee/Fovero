namespace Fovero.Model.Tiling;

public interface IEdge
{
    IReadOnlyList<ITile> Neighbors { get; }

    bool IsShared => Neighbors.Count > 1;

    string PathData { get; }

    string DrawData { get; }
}
