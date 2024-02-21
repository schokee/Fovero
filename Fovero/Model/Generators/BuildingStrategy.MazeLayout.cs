using System.Collections;
using MoreLinq;

namespace Fovero.Model.Generators;

public partial record BuildingStrategy<T>
{
    private interface IStep
    {
        ICell Start { get; }

        ICell End { get; }

        T Wall { get; }
    }

    private interface ICell
    {
        bool HasBeenVisited { get; set; }

        ICell RandomNeighbor { get; }

        IEnumerable<IStep> Neighbors { get; }

        IEnumerable<IStep> UnvisitedNeighbors => Neighbors.Where(step => !step.End.HasBeenVisited);

        IEnumerable<IStep> VisitedNeighbors => Neighbors.Where(step => step.End.HasBeenVisited);
    }

    private sealed class MazeLayout : IEnumerable<ICell>
    {
        private readonly Random _random;
        private readonly IReadOnlyDictionary<ushort, ICell> _cells;
        private readonly ILookup<ushort, IStep> _pathToNeighbor;
        private readonly HashSet<ushort> _unvisitedCells;

        public MazeLayout(IReadOnlyCollection<T> allWalls, Random random)
        {
            if (allWalls.Count == 0)
            {
                throw new ArgumentException(nameof(allWalls));
            }

            _random = random;
            _pathToNeighbor = allWalls
                .Shuffle(random)
                .SelectMany(sharedWall => new[]
                {
                    new Step(this, sharedWall.NeighborA, sharedWall.NeighborB, sharedWall),
                    new Step(this, sharedWall.NeighborB, sharedWall.NeighborA, sharedWall)
                })
                .Distinct()
                .ToLookup(x => x.Cell, x => (IStep)x);

            _cells = _pathToNeighbor.ToDictionary(x => x.Key, x => (ICell)new Cell(this, x.Key));
            _unvisitedCells = [.._pathToNeighbor.Select(x => x.Key)];
        }

        public bool IsEmpty => !_pathToNeighbor.Any();

        public bool IsComplete => _unvisitedCells.Count == 0;

        public ICell RandomUnvisited => _cells[_unvisitedCells.SelectRandom(_random)];

        public IEnumerable<ICell> VisitedCells => _cells.Values.Where(x => x.HasBeenVisited);

        public IEnumerable<ICell> UnvisitedCells => _unvisitedCells.Select(x => _cells[x]);

        public IEnumerator<ICell> GetEnumerator()
        {
            return _cells.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private record Step(MazeLayout Layout, ushort Cell, ushort Neighbor, T Wall) : IStep
        {
            public ICell Start => Layout._cells[Cell];

            public ICell End => Layout._cells[Neighbor];

            public T Wall { get; } = Wall;
        }

        private sealed class Cell(MazeLayout layout, ushort ordinal) : ICell
        {
            private readonly ushort _ordinal = ordinal;

            private IEnumerable<IStep> StepsToNeighbors => layout._pathToNeighbor[_ordinal];

            public bool HasBeenVisited
            {
                get => !layout._unvisitedCells.Contains(_ordinal);
                set
                {
                    if (value)
                    {
                        layout._unvisitedCells.Remove(_ordinal);
                    }
                    else
                    {
                        layout._unvisitedCells.Add(_ordinal);
                    }
                }
            }

            public ICell RandomNeighbor => StepsToNeighbors.ElementAt(layout._random.Next(StepsToNeighbors.Count())).End;

            public IEnumerable<IStep> Neighbors => StepsToNeighbors;

            public override string ToString()
            {
                return _ordinal.ToString();
            }

            public override bool Equals(object obj)
            {
                return ReferenceEquals(this, obj) || obj is Cell other && other._ordinal == _ordinal;
            }

            public override int GetHashCode()
            {
                return _ordinal.GetHashCode();
            }
        }
    }
}
