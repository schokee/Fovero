using Fovero.Model;
using Fovero.Model.Geometry;

namespace Fovero.UI;

public interface IMazeCell : INode, IVisitable
{
    ushort Ordinal { get; }

    Rectangle Bounds { get; }

    string PathData { get; }
}
