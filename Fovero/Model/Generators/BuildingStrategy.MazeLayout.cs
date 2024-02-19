using System.Collections;
using MoreLinq;

namespace Fovero.Model.Generators;

public partial record BuildingStrategy<T>
{
    private interface ILink
    {
        ICell StartCell { get; }

        ICell EndCell { get; }

        T Wall { get; }
    }

    private interface ICell
    {
        bool HasBeenVisited { get; set; }

        ICell RandomNeighbor { get; }

        IEnumerable<ILink> Neighbors { get; }

        IEnumerable<ILink> UnvisitedNeighbors => Neighbors.Where(link => !link.StartCell.HasBeenVisited);
    }

    private sealed class MazeLayout : IEnumerable<ICell>
    {
        private readonly Random _random;
        private readonly ILookup<ushort, Link> _allLinks;
        private readonly HashSet<ushort> _unvisitedCells;

        public MazeLayout(IEnumerable<T> allWalls, Random random)
        {
            _random = random;
            _allLinks = allWalls
                .Shuffle(random)
                .SelectMany(sharedWall => new[]
                {
                    new Link(this, sharedWall.NeighborA, sharedWall.NeighborB, sharedWall),
                    new Link(this, sharedWall.NeighborB, sharedWall.NeighborA, sharedWall)
                })
                .Distinct()
                .ToLookup(x => x.Cell);

            _unvisitedCells = [.._allLinks.Select(x => x.Key)];
        }

        public bool IsEmpty => !_allLinks.Any();

        public bool IsComplete => _unvisitedCells.Count == 0;

        public IEnumerable<ICell> UnvisitedCells => _unvisitedCells.Select(x => new Cell(this, x));

        public ICell RandomUnvisited => new Cell(this, _unvisitedCells.SelectRandom(_random));

        public IEnumerator<ICell> GetEnumerator()
        {
            return _allLinks.Select(x => new Cell(this, x.Key)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private record Link(MazeLayout Layout, ushort Cell, ushort Neighbor, T Wall) : ILink
        {
            public ICell StartCell => new Cell(Layout, Cell);

            public ICell EndCell => new Cell(Layout, Neighbor);

            public T Wall { get; } = Wall;

            public Link Reversed => this with { Cell = Neighbor, Neighbor = Cell };
        }

        private sealed class Cell(MazeLayout layout, ushort ordinal) : ICell
        {
            private readonly ushort _ordinal = ordinal;

            private IEnumerable<Link> Links => layout._allLinks[_ordinal];

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

            public ICell RandomNeighbor => Links.ElementAt(layout._random.Next(Links.Count())).EndCell;

            public IEnumerable<ILink> Neighbors => Links.Select(link => link.Reversed);

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
