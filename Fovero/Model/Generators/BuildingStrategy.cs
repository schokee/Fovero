using MoreLinq;

namespace Fovero.Model.Generators;

public record BuildingStrategy<T>(string Name, Func<IReadOnlyList<T>, Random, IEnumerable<T>> SelectWallsToBeOpened) where T : ISharedWall
{
    public override string ToString()
    {
        return Name;
    }

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

            IEnumerable<T> Build(IReadOnlyList<T> allWalls, Random random)
            {
                if (allWalls.Count == 0)
                {
                    yield break;
                }

                var allLinks = CreateLookup(allWalls, random);
                var cell = allLinks.First().Key;

                var unvisitedCells = new HashSet<ushort>(allLinks.Select(x => x.Key).Where(x => x != cell));

                while (unvisitedCells.Any())
                {
                    var toJoin = allLinks[cell]
                                     .FirstOrDefault(x => unvisitedCells.Contains(x.Neighbor)) ??
                                 allLinks
                                     .Where(x => !unvisitedCells.Contains(x.Key))
                                     .SelectMany(neighboringCells => neighboringCells)
                                     .First(link => unvisitedCells.Contains(link.Neighbor));

                    unvisitedCells.Remove(toJoin.Neighbor);
                    cell = toJoin.Neighbor;

                    yield return toJoin.Wall;
                }
            }
        }
    }

    public static BuildingStrategy<T> Wilson
    {
        get
        {
            return new BuildingStrategy<T>("Wilson's", Build);

            IEnumerable<T> Build(IReadOnlyList<T> allWalls, Random random)
            {
                if (allWalls.Count == 0)
                {
                    yield break;
                }

                var allLinks = CreateLookup(allWalls, random);
                var unvisitedCells = new HashSet<ushort>(allLinks.Skip(1).Select(x => x.Key));

                while (unvisitedCells.Count > 0)
                {
                    var step = unvisitedCells.ElementAt(random.Next(unvisitedCells.Count));
                    var path = new List<ushort> { step };

                    // Walk
                    while (unvisitedCells.Contains(step))
                    {
                        var directions = allLinks[step];
                        step = directions.ElementAt(random.Next(directions.Count())).Neighbor;

                        var truncateAt = path.IndexOf(step) + 1;

                        if (truncateAt > 0)
                        {
                            // Loop detected - truncate the path
                            path.RemoveRange(truncateAt, path.Count - truncateAt);
                        }
                        else
                        {
                            path.Add(step);
                        }
                    }

                    // Carve
                    foreach (var link in path.Pairwise((cell, neighbor) => allLinks[cell].First(x => x.Neighbor == neighbor)))
                    {
                        unvisitedCells.Remove(link.Cell);
                        yield return link.Wall;
                    }
                }
            }
        }
    }

    public static BuildingStrategy<T> RecursiveBacktracker
    {
        get
        {
            return new BuildingStrategy<T>("Recursive Backtracker", Build);

            IEnumerable<T> Build(IReadOnlyList<T> allWalls, Random random)
            {
                if (allWalls.Count == 0)
                {
                    yield break;
                }

                var allLinks = CreateLookup(allWalls, random);

                var startingCell = allWalls[0].NeighborA;
                var visitedCells = new HashSet<ushort> { startingCell };

                var stack = new Stack<ushort>();
                stack.Push(startingCell);

                while (stack.Count > 0)
                {
                    var cell = stack.Peek();
                    var link = allLinks[cell].FirstOrDefault(x => !visitedCells.Contains(x.Neighbor));

                    if (link is null)
                    {
                        stack.Pop();
                        continue;
                    }

                    visitedCells.Add(link.Neighbor);
                    yield return link.Wall;

                    stack.Push(link.Neighbor);
                }
            }
        }
    }

    private record Link(ushort Cell, ushort Neighbor, T Wall);

    private static ILookup<ushort, Link> CreateLookup(IEnumerable<T> allWalls, Random random)
    {
        return allWalls
            .Shuffle(random)
            .SelectMany(sharedWall => new[]
            {
                new Link(sharedWall.NeighborA, sharedWall.NeighborB, sharedWall),
                new Link(sharedWall.NeighborB, sharedWall.NeighborA, sharedWall)
            })
            .Distinct()
            .ToLookup(x => x.Cell);
    }
}
