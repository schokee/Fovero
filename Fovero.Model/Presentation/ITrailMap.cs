using Fovero.Model.Solvers;

namespace Fovero.Model.Presentation;

/// <summary>
/// Represents a trail map interface that provides functionality for managing and solving a maze.
/// </summary>
public interface ITrailMap : IEnumerable<IMazeCell>
{
    /// <summary>
    /// Occurs when the solution to the maze changes.
    /// </summary>
    event EventHandler SolutionChanged;

    /// <summary>
    /// Gets or sets the starting cell of the maze.
    /// </summary>
    IMazeCell StartCell { get; set; }

    /// <summary>
    /// Gets or sets the ending cell of the maze.
    /// </summary>
    IMazeCell EndCell { get; set; }

    /// <summary>
    /// Gets the collection of markers in the maze.
    /// </summary>
    IEnumerable<string> Markers { get; }

    /// <summary>
    /// Gets the read-only collection of cells that form the solution to the maze.
    /// </summary>
    IReadOnlyCollection<IMazeCell> Solution { get; }

    /// <summary>
    /// Gets a value indicating whether the maze is solved.
    /// </summary>
    bool IsSolved { get; }

    /// <summary>
    /// Gets a value indicating whether the maze can be reset.
    /// </summary>
    bool CanReset { get; }

    /// <summary>
    /// Resets the maze to its initial state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Determines whether the specified cell is a valid starting cell.
    /// </summary>
    /// <param name="cell">The cell to validate.</param>
    /// <returns><c>true</c> if the cell is a valid starting cell; otherwise, <c>false</c>.</returns>
    bool IsValidStart(IMazeCell cell);

    /// <summary>
    /// Determines whether the specified cell is a valid ending cell.
    /// </summary>
    /// <param name="cell">The cell to validate.</param>
    /// <returns><c>true</c> if the cell is a valid ending cell; otherwise, <c>false</c>.</returns>
    bool IsValidEnd(IMazeCell cell);

    /// <summary>
    /// Enumerates the steps to solve the maze using the specified solving strategy.
    /// </summary>
    /// <param name="solvingStrategy">The strategy to use for solving the maze.</param>
    /// <returns>An enumerable collection of actions representing the solution steps.</returns>
    IEnumerable<Action> EnumerateSolutionSteps(SolvingStrategy solvingStrategy);

    /// <summary>
    /// Reverses the start and end points of the maze.
    /// </summary>
    void ReverseEndPoints();

    /// <summary>
    /// Highlights the trail to the specified cell.
    /// </summary>
    /// <param name="cell">The cell to highlight the trail to.</param>
    /// <returns><c>true</c> if the trail was successfully highlighted; otherwise, <c>false</c>.</returns>
    bool HighlightTrailTo(IMazeCell cell);
}
