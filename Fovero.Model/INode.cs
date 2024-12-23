using Fovero.Model.Geometry;

namespace Fovero.Model;

/// <summary>
/// Represents a node with a location and a collection of neighboring nodes.
/// </summary>
public interface INode
{
    /// <summary>
    /// Gets the location of the node.
    /// </summary>
    Point2D Location { get; }

    /// <summary>
    /// Gets the neighboring nodes of this node.
    /// </summary>
    IEnumerable<INode> Neighbors { get; }
}
