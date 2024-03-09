using Fovero.Model.Geometry;

namespace Fovero.Model;

public interface INode
{
    Point2D Location { get; }

    IEnumerable<INode> Neighbors { get; }
}
