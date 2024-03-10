using Fovero.Model.Solvers;

namespace Fovero.Model.Presentation;

public interface ITrailMap : IEnumerable<IMazeCell>
{
    event EventHandler SolutionChanged;

    IMazeCell StartCell { get; set; }

    IMazeCell EndCell { get; set; }

    IEnumerable<string> Markers { get; }

    IReadOnlyCollection<IMazeCell> Solution { get; }

    bool IsSolved { get; }

    bool CanReset { get; }

    void Reset();

    bool IsValidStart(IMazeCell cell);

    bool IsValidEnd(IMazeCell cell);

    IEnumerable<Action> EnumerateSolutionSteps(SolvingStrategy solvingStrategy);

    void ReverseEndPoints();

    bool HighlightTrailTo(IMazeCell cell);
}
