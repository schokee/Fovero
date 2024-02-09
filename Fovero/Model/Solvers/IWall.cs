using Fovero.Model.Tiling;

namespace Fovero.Model.Solvers;

public interface IWall
{
    bool IsOpen { get; }

    bool IsShared { get; }

    IEdge Edge { get; }
}
