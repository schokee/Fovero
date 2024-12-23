namespace Fovero.Model.Presentation;

/// <summary>
/// Represents a cell in a maze, which can be visited and has an ordinal position and path data.
/// </summary>
public interface IMazeCell : INode, IVisitable
{
    /// <summary>
    /// Gets the ordinal position of the maze cell.
    /// </summary>
    ushort Ordinal { get; }

    /// <summary>
    /// Gets the path data associated with the maze cell.
    /// </summary>
    string PathData { get; }
}
