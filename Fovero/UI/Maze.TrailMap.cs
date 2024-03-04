using System.Collections;
using System.Collections.Specialized;
using Caliburn.Micro;
using Fovero.Model.DataStructures;
using Fovero.Model.Geometry;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using MoreLinq;

namespace Fovero.UI;

public sealed partial class Maze
{
    private class TrailMap : PropertyChangedBase, ITrailMap
    {
        private IMazeCell _startCell;
        private IMazeCell _endCell;
        private IReadOnlyDictionary<ushort, Cell> Cells { get; }

        private readonly InvertedTree<IMazeCell> _visitedPaths = new();

        public TrailMap(IEnumerable<ITile> tiles, IReadOnlyList<Wall> walls)
        {
            Cells = tiles
                .Select(tile => new Cell(tile, SelectAdjacent))
                .ToDictionary(x => x.Ordinal);

            StartCell = Cells.Values.MinBy(x => x.Ordinal);
            EndCell = Cells.Values.MaxBy(x => x.Ordinal);

            Solution.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    if (args.NewStartingIndex > 0)
                    {
                        _visitedPaths.Insert(Solution.Skip(args.NewStartingIndex - 1));
                    }

                    foreach (IMazeCell cell in args.NewItems!)
                    {
                        cell.VisitCount++;
                    }
                }

                NotifyOfPropertyChange(nameof(IsSolved));
                NotifyOfPropertyChange(nameof(CanReset));
            };

            IEnumerable<Cell> SelectAdjacent(Cell cell) => walls
                .Where(wall => wall.IsOpen)
                .SelectMany(wall => wall.SelectPathwaysFrom(cell, n => Cells[n]))
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

        public IObservableCollection<IMazeCell> Solution { get; } = new BindableCollection<IMazeCell>();

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
            Solution.Clear();
            _visitedPaths.Clear();

            foreach (var cell in this)
            {
                cell.VisitCount = 0;
            }
        }

        public bool IsValidStart(IMazeCell cell)
        {
            return !(cell is null || cell.Equals(StartCell) || cell.Equals(EndCell));
        }

        public bool IsValidEnd(IMazeCell cell)
        {
            return !(cell is null || cell.Equals(StartCell) || cell.Equals(EndCell));
        }

        public IEnumerable<IMazeCell> GetPathToVisitedCell(IMazeCell cell)
        {
            return _visitedPaths.GetPathTo(cell);
        }

        public IEnumerable<CollectionChange> FindSolution(SolvingStrategy solvingStrategy)
        {
            ArgumentNullException.ThrowIfNull(solvingStrategy, nameof(solvingStrategy));

            return solvingStrategy
                .FindPath(StartCell, EndCell)
                .Prepend(Array.Empty<IMazeCell>())
                .Pairwise((prev, next) => prev.SwitchTo(next))
                .SelectMany(move => move);
        }

        public void ReverseEndPoints()
        {
            (_startCell, _endCell) = (_endCell, _startCell);

            Reset();
            NotifyOfPropertyChange(nameof(StartCell));
            NotifyOfPropertyChange(nameof(EndCell));
            NotifyOfPropertyChange(nameof(Markers));
        }

        public void Update(CollectionChange change)
        {
            switch (change)
            {
                case RemoveLast:
                    Solution.RemoveAt(Solution.Count - 1);
                    break;
                case Append<ICell> { Item: IMazeCell cell }:
                    Solution.Add(cell);
                    break;
            }
        }

        private static Rectangle MarkerBounds { get; } = new Rectangle(0, 0, 0.45f, 0.45f).ToScaledUnits();
    }
}
