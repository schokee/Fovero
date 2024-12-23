using MoreLinq;

namespace Fovero.Model.Generators;

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Name">The name of the strategy</param>
/// <param name="SelectBordersToBeOpened"> A function that takes a list of borders and a random number generator, and
/// returns an enumerable of borders to be opened.</param>
/// <remarks>
/// <para>
/// The <see cref="BuildingStrategy{T}"/> class provides a flexible framework for generating structures using various
/// algorithms. Each algorithm is encapsulated in a static property that returns a configured <see cref="BuildingStrategy{T}"/>
/// instance. The algorithms use different techniques to traverse and connect cells, ensuring diverse and
/// interesting structure generation.
/// </para>
/// <para>
/// Each building strategy in the <see cref="BuildingStrategy{T}"/> class offers unique characteristics suitable for
/// different applications, from maze generation in games to network design and procedural content generation. The choice of
/// strategy depends on the desired properties of the generated structure, such as path length, distribution, and complexity.
/// </para>
/// </remarks>
public partial record BuildingStrategy<T>(string Name, Func<IReadOnlyList<T>, Random, IEnumerable<T>> SelectBordersToBeOpened) where T : ISharedBorder
{
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    /// Returns a list of all available building strategies.
    /// </summary>
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

    /// <summary>
    /// Gets a <see cref="BuildingStrategy{T}"/> that uses Kruskal's Algorithm to build a structure.
    /// </summary>
    /// <returns>A <see cref="BuildingStrategy{T}"/> that uses Kruskal's Algorithm.</returns>
    /// <remarks>
    /// Kruskal's Algorithm is a minimum spanning tree algorithm which finds an edge of the least possible
    /// weight that connects any two trees in the forest.
    /// </remarks>
    public static BuildingStrategy<T> Kruskal
    {
        get
        {
            return new BuildingStrategy<T>("Kruskal's Algorithm", Build);

            IEnumerable<T> Build(IReadOnlyList<T> allBorders, Random random)
            {
                var shuffledBorders = allBorders.Shuffle(random).ToList();
                var sets = new DisjointSet<ushort>(shuffledBorders.SelectMany(border => border.Neighbors));

                foreach (var border in shuffledBorders.Where(x => sets.AreDisjoint(x.NeighborA, x.NeighborB)))
                {
                    sets.Merge(border.NeighborA, border.NeighborB);
                    yield return border;
                }
            }
        }
    }

    /// <summary>
    /// Gets a <see cref="BuildingStrategy{T}"/> instance that uses the "Hunt and Kill" algorithm to build a maze.
    /// </summary>
    /// <returns>A <see cref="BuildingStrategy{T}"/> instance configured with the "Hunt and Kill" algorithm.</returns>
    /// <remarks>
    /// The "Hunt and Kill" algorithm works by randomly selecting an unvisited cell, marking it as visited, and then
    /// repeatedly moving to an unvisited neighbor until no unvisited neighbors are left. At that point, it hunts for
    /// any remaining unvisited cells and repeats the process until all cells have been visited.
    /// </remarks>
    public static BuildingStrategy<T> HuntAndKill
    {
        get
        {
            return new BuildingStrategy<T>("Hunt and Kill", Build);

            static IEnumerable<T> Build(IReadOnlyList<T> allBorders, Random random)
            {
                var layout = new MazeLayout(allBorders, random);

                var cell = layout.RandomUnvisited;

                cell.HasBeenVisited = true;

                while (!layout.IsComplete)
                {
                    var stepToNeighbor =
                        cell.UnvisitedNeighbors.FirstOrDefault() ??
                        layout.VisitedCells.SelectMany(cell => cell.UnvisitedNeighbors).First();

                    cell = stepToNeighbor.End;  // Unvisited neighbor
                    cell.HasBeenVisited = true;

                    yield return stepToNeighbor.Border;
                }
            }
        }
    }

    /// <summary>
    /// Gets a new instance of the <see cref="BuildingStrategy{T}"/> class using Prim's algorithm.
    /// </summary>
    /// <returns>
    /// A <see cref="BuildingStrategy{T}"/> instance configured to use Prim's algorithm.
    /// </returns>
    public static BuildingStrategy<T> Prim
    {
        get => new("Prim's", (allBorders, random) => GrowingTree(allBorders, random, x => x.SelectRandom(random)));
    }

    /// <summary>
    /// Gets a variation of Prim's algorithm that randomly chooses between selecting the next cell randomly or the last cell.
    /// </summary>
    public static BuildingStrategy<T> PrimMixed
    {
        get => new("Prim's (Mixed)", (allBorders, random) => GrowingTree(allBorders, random, x => random.Next(2) > 0 ? x.SelectRandom(random) : x.Last()));
    }

    /// <summary>
    /// Gets a variation of Prim's algorithm that always selects the oldest cell.
    /// </summary>
    public static BuildingStrategy<T> PrimOldest
    {
        get => new("Prim's (Oldest)", (allBorders, random) => GrowingTree(allBorders, random, x => x.First()));
    }

    /// <summary>
    /// Generates a maze layout using the Growing Tree algorithm, which involves visiting cells and selecting the next
    /// cell based on a provided function.
    /// </summary>
    /// <param name="allBorders">A read-only collection of all borders in the maze.</param>
    /// <param name="random">An instance of Random used for randomization.</param>
    /// <param name="selectCell">A function that selects the next cell to visit from a collection of cells.</param>
    /// <returns>An enumerable of borders representing the steps taken to generate the maze.</returns>
    private static IEnumerable<T> GrowingTree(IReadOnlyCollection<T> allBorders, Random random, Func<IReadOnlyCollection<ICell>, ICell> selectCell)
    {
        var layout = new MazeLayout(allBorders, random);

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
            yield return stepToNeighbor.Border;
        }

        void Visit(ICell cell)
        {
            pool.Add(cell);
            cell.HasBeenVisited = true;
        }
    }

    /// <summary>
    /// Gets the "Recursive Backtracker" building strategy.
    /// </summary>
    /// <returns>A <see cref="BuildingStrategy{T}"/> instance that uses the "Recursive Backtracker" algorithm.</returns>
    /// <remarks>
    /// It uses a stack to keep track of the path and backtracks when no unvisited neighbors are left.
    /// </remarks>
    public static BuildingStrategy<T> RecursiveBacktracker
    {
        get
        {
            return new BuildingStrategy<T>("Recursive Backtracker", Build);

            static IEnumerable<T> Build(IReadOnlyList<T> allBorders, Random random)
            {
                var layout = new MazeLayout(allBorders, random);

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
                    yield return stepToNeighbor.Border;
                }

                void Visit(ICell cell)
                {
                    stack.Push(cell);
                    cell.HasBeenVisited = true;
                }
            }
        }
    }

    /// <summary>
    /// Gets a <see cref="BuildingStrategy{T}"/> that uses Wilson's algorithm to generate a maze.
    /// </summary>
    /// <returns>A <see cref="BuildingStrategy{T}"/> that generates a maze using Wilson's algorithm.</returns>
    /// <remarks>
    /// Wilson's algorithm is a maze generation algorithm that generates a uniform spanning tree.
    /// It works by randomly walking through the maze and carving out paths until all cells have been visited.
    /// </remarks>
    public static BuildingStrategy<T> Wilson
    {
        get
        {
            return new BuildingStrategy<T>("Wilson's", Build);

            static IEnumerable<T> Build(IReadOnlyList<T> allBorders, Random random)
            {
                var layout = new MazeLayout(allBorders, random)
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
                        yield return step.Border;
                    }
                }
            }
        }
    }
}
