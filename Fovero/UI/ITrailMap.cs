using Caliburn.Micro;
using Fovero.Model.Solvers;

namespace Fovero.UI;

public interface ITrailMap : IEnumerable<IMazeCell>
{
    IMazeCell StartCell { get; set; }

    IMazeCell EndCell { get; set; }

    IEnumerable<string> Markers { get; }

    IObservableCollection<IMazeCell> Solution { get; }

    bool IsSolved { get; }

    bool CanReset { get; }

    void Reset();

    bool IsValidStart(IMazeCell cell);

    bool IsValidEnd(IMazeCell cell);

    void RestorePathToVisitedCell(IMazeCell cell);

    IEnumerable<CollectionChange> FindSolution(SolvingStrategy solvingStrategy);

    void ReverseEndPoints();

    void Update(CollectionChange change);
}
