using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Fovero.Model.Geometry;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using MoreLinq;

namespace Fovero.Model.Presentation;

public sealed partial class Maze
{
    private class TrailMap : Bindable, ITrailMap
    {
        private readonly Dictionary<INode, Path<INode>> _visitedPaths = [];
        private readonly ObservableCollection<IMazeCell> _solution = [];

        private IMazeCell _startCell;
        private IMazeCell _endCell;
        private IReadOnlyDictionary<ushort, Cell> Cells { get; }

        public event EventHandler? SolutionChanged;

        public TrailMap(IEnumerable<ITile> tiles, IReadOnlyList<ISharedBorder> borders)
        {
            Cells = tiles
                .Select(tile => new Cell(tile, SelectAccessibleNeighbors))
                .ToDictionary(x => x.Ordinal);

            _startCell = Cells.Values.MinBy(x => x.Ordinal)!;
            _endCell = Cells.Values.MaxBy(x => x.Ordinal)!;

            _solution.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                    {
                        _visitedPaths.Clear();
                        break;
                    }

                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (IMazeCell cell in args.NewItems!)
                        {
                            ++cell.VisitCount;
                        }

                        break;
                    }
                }

                NotifyOfPropertyChange(nameof(IsSolved));
                NotifyOfPropertyChange(nameof(CanReset));

                SolutionChanged?.Invoke(this, EventArgs.Empty);
            };

            IEnumerable<Cell> SelectAccessibleNeighbors(Cell cell) => borders
                .Where(border => border.IsOpen)
                .SelectMany(border => border.SelectPathwaysFrom(cell, n => Cells![n]))
                .Select(x => x.To);
        }

        public IMazeCell StartCell
        {
            get => _startCell;
            set
            {
                if (IsValidStart(value))
                {
                    Set(ref _startCell, value);
                    Reset();
                    NotifyOfPropertyChange(nameof(Markers));
                }
            }
        }

        public IMazeCell EndCell
        {
            get => _endCell;
            set
            {
                if (IsValidEnd(value))
                {
                    Set(ref _endCell, value);
                    Reset();
                    NotifyOfPropertyChange(nameof(Markers));
                }
            }
        }

        public IEnumerable<string> Markers
        {
            get
            {
                var bounds = MarkerBounds.CenteredAt(StartCell.Location);
                var radius = bounds.Width / 2;

                yield return $"M {bounds.Left} {StartCell.Location.Y} a {radius} {radius} 0 0 0 {bounds.Width} 0 a {radius} {radius} 0 0 0 {-bounds.Width} 0 z";

                bounds = MarkerBounds.CenteredAt(EndCell.Location);
                radius = bounds.Width / 2;

                yield return $"M {EndCell.Location.X} {bounds.Top} l {radius} {radius} l {-radius} {radius} l {-radius} {-radius} z";
            }
        }

        public IReadOnlyCollection<IMazeCell> Solution => _solution;

        public IEnumerator<IMazeCell> GetEnumerator()
        {
            return Cells.Values.OrderByDescending(x => x.Ordinal).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsSolved => Solution.Count > 1 && EndCell.Equals(Solution.Last());

        public bool CanReset => Solution.Count > 0;

        public void Reset()
        {
            _solution.Clear();

            foreach (var cell in this)
            {
                cell.VisitCount = 0;
            }
        }

        public bool IsValidStart(IMazeCell? cell)
        {
            return !(cell is null || cell.Equals(StartCell) || cell.Equals(EndCell));
        }

        public bool IsValidEnd(IMazeCell? cell)
        {
            return !(cell is null || cell.Equals(StartCell) || cell.Equals(EndCell));
        }

        public IEnumerable<Action> EnumerateSolutionSteps(SolvingStrategy solvingStrategy)
        {
            ArgumentNullException.ThrowIfNull(solvingStrategy, nameof(solvingStrategy));

            return solvingStrategy
                .FindPath(StartCell, EndCell)
                .Pipe(AddTrail)
                .Prepend(Path<INode>.Empty)
                .Pairwise((prev, next) => prev.SwitchTo(next))
                .SelectMany(move => move)
                .ToScript(Update);
        }

        public void ReverseEndPoints()
        {
            (_startCell, _endCell) = (_endCell, _startCell);

            Reset();
            NotifyOfPropertyChange(nameof(StartCell));
            NotifyOfPropertyChange(nameof(EndCell));
            NotifyOfPropertyChange(nameof(Markers));
        }

        public bool HighlightTrailTo(IMazeCell cell)
        {
            ArgumentNullException.ThrowIfNull(cell, nameof(cell));

            if (cell.HasBeenVisited)
            {
                _solution
                    .SwitchTo(_visitedPaths[cell])
                    .ForEach(Update);

                return true;
            }

            if (cell.Equals(StartCell))
            {
                _solution.Add(cell);
                AddTrail(new Path<INode>(cell));
                return true;
            }

            var endOfTrail = _solution.LastOrDefault();

            if (endOfTrail?.Neighbors.Contains(cell) == true)
            {
                _solution.Add(cell);
                AddTrail(_visitedPaths[endOfTrail].To(cell));
                return true;
            }

            return false;
        }

        private void AddTrail(Path<INode> path)
        {
            _visitedPaths.Add(path.Last, path);
        }

        private void Update(CollectionChange change)
        {
            switch (change)
            {
                case RemoveLast:
                    _solution.RemoveAt(Solution.Count - 1);
                    break;
                case Append<INode> { Item: IMazeCell cell }:
                    _solution.Add(cell);
                    break;
            }
        }

        private static Rectangle MarkerBounds { get; } = new Rectangle(0, 0, 0.45f, 0.45f).ToScaledUnits();
    }
}
