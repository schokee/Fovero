using Fovero.Model.Geometry;
using Fovero.Model.Solvers;

namespace Fovero.UI;

public interface IMazeCell : ICell
{
    ushort Ordinal { get; }

    Rectangle Bounds { get; }

    string PathData { get; }

    bool HasBeenVisited { get; }

    uint VisitCount { get; set; }
}
