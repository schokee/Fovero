using MoreLinq;

namespace Fovero.Model.Generators;

public partial record BuildingStrategy<T>(string Name, Func<IReadOnlyList<T>, Random, IEnumerable<T>> SelectWallsToBeOpened) where T : ISharedWall
{
    public override string ToString()
    {
        return Name;
    }

    public static IReadOnlyList<BuildingStrategy<T>> All =>
    [
        HuntAndKill,
        Kruskal,
        Prim,
        PrimMixed,
        PrimOldest,
        RecursiveBacktracker,
        Wilson
    ];

    public static BuildingStrategy<T> Kruskal
    {
        get
        {
            return new BuildingStrategy<T>("Kruskal's Algorithm", Build);

            IEnumerable<T> Build(IReadOnlyList<T> allWalls, Random random)
            {
                var shuffledWalls = allWalls.Shuffle(random).ToList();
                var sets = new DisjointSet<ushort>(shuffledWalls.SelectMany(wall => wall.Neighbors));

                foreach (var wall in shuffledWalls.Where(x => sets.AreDisjoint(x.NeighborA, x.NeighborB)))
                {
                    sets.Merge(wall.NeighborA, wall.NeighborB);
                    yield return wall;
                }
            }
        }
    }

    public static BuildingStrategy<T> HuntAndKill
    {
        get
        {
            return new BuildingStrategy<T>("Hunt and Kill", Build);

            static IEnumerable<T> Build(IReadOnlyList<T> allWalls, Random random)
            {
                var layout = new MazeLayout(allWalls, random);

                var cell = layout.RandomUnvisited;

                while (!layout.IsComplete)
                {
                    var stepToNeighbor =
                        cell.UnvisitedNeighbors.FirstOrDefault() ??
                        layout.VisitedCells.SelectMany(cell => cell.UnvisitedNeighbors).First();

                    cell = stepToNeighbor.End;  // Unvisited neighbor
                    cell.HasBeenVisited = true;

                    yield return stepToNeighbor.Wall;
                }
            }
        }
    }

    public static BuildingStrategy<T> Prim
    {
        get => new("Prim's", (allWalls, random) => GrowingTree(allWalls, random, x => x.SelectRandom(random)));
    }

    public static BuildingStrategy<T> PrimMixed
    {
        get => new("Prim's (Mixed)", (allWalls, random) => GrowingTree(allWalls, random, x => random.Next(2) > 0 ? x.SelectRandom(random) : x.Last()));
    }

    public static BuildingStrategy<T> PrimOldest
    {
        get => new("Prim's (Oldest)", (allWalls, random) => GrowingTree(allWalls, random, x => x.First()));
    }

    private static IEnumerable<T> GrowingTree(IReadOnlyCollection<T> allWalls, Random random, Func<IReadOnlyCollection<ICell>, ICell> selectCell)
    {
        var layout = new MazeLayout(allWalls, random);

        var pool = new HashSet<ICell>();
        Visit(layout.RandomUnvisited);

        while (!layout.IsComplete)
        {
            var cell = selectCell(pool);
            var stepToNeighbor = cell.UnvisitedNeighbors.FirstOrDefault();

            if (stepToNeighbor is null)
            {
                pool.Remove(cell);
                continue;
            }

            Visit(stepToNeighbor.End);
            yield return stepToNeighbor.Wall;
        }

        void Visit(ICell cell)
        {
            pool.Add(cell);
            cell.HasBeenVisited = true;
        }
    }

    public static BuildingStrategy<T> RecursiveBacktracker
    {
        get
        {
            return new BuildingStrategy<T>("Recursive Backtracker", Build);

            static IEnumerable<T> Build(IReadOnlyList<T> allWalls, Random random)
            {
                var layout = new MazeLayout(allWalls, random);

                var stack = new Stack<ICell>();
                Visit(layout.RandomUnvisited);

                while (!layout.IsComplete)
                {
                    var cell = stack.Peek();
                    var stepToNeighbor = cell.UnvisitedNeighbors.FirstOrDefault();

                    if (stepToNeighbor is null)
                    {
                        stack.Pop();
                        continue;
                    }

                    Visit(stepToNeighbor.End);
                    yield return stepToNeighbor.Wall;
                }

                void Visit(ICell cell)
                {
                    stack.Push(cell);
                    cell.HasBeenVisited = true;
                }
            }
        }
    }

    public static BuildingStrategy<T> Wilson
    {
        get
        {
            return new BuildingStrategy<T>("Wilson's", Build);

            static IEnumerable<T> Build(IReadOnlyList<T> allWalls, Random random)
            {
                var layout = new MazeLayout(allWalls, random)
                {
                    RandomUnvisited = { HasBeenVisited = true }
                };

                while (!layout.IsComplete)
                {
                    var cell = layout.RandomUnvisited;
                    var path = new List<ICell> { cell };

                    // Walk
                    while (!cell.HasBeenVisited)
                    {
                        cell = cell.RandomNeighbor;

                        var truncateAt = path.IndexOf(cell) + 1;

                        if (truncateAt > 0)
                        {
                            // Loop detected - truncate the path
                            path.RemoveRange(truncateAt, path.Count - truncateAt);
                        }
                        else
                        {
                            path.Add(cell);
                        }
                    }

                    // Carve
                    foreach (var step in path.Pairwise((cell, neighbor) => cell.Neighbors.First(x => Equals(neighbor, x.End))))
                    {
                        step.Start.HasBeenVisited = true;
                        yield return step.Wall;
                    }
                }
            }
        }
    }
}
