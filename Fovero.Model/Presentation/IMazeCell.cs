using Fovero.Model.Geometry;

namespace Fovero.Model.Presentation;

public interface IMazeCell : INode, IVisitable
{
    ushort Ordinal { get; }

    Rectangle Bounds { get; }

    string PathData { get; }
}
