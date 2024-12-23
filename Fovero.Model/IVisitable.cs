namespace Fovero.Model;

/// <summary>
/// Represents an interface for objects that can be visited.
/// </summary>
public interface IVisitable
{
    /// <summary>
    /// Gets a value indicating whether the object has been visited.
    /// </summary>
    bool HasBeenVisited { get; }

    /// <summary>
    /// Gets or sets the number of times the object has been visited.
    /// </summary>
    uint VisitCount { get; set; }
}
