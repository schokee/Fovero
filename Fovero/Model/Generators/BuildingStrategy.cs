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
                var allLinks = CreateLookup(allWalls, random);
                var cell = allLinks.ElementAt(random.Next(allLinks.Count)).Key;

                var unvisitedCells = new HashSet<ushort>(allLinks.Select(x => x.Key));

                while (unvisitedCells.Count > 0)
                {
                    var toJoin = allLinks[cell]
                                     .FirstOrDefault(x => unvisitedCells.Contains(x.Neighbor)) ??
                                 allLinks
                                     .Where(x => unvisitedCells.Contains(x.Key))
                                     .SelectMany(neighboringCells => neighboringCells.Where(link => !unvisitedCells.Contains(link.Neighbor)))
                                     .First();

                    unvisitedCells.Remove(toJoin.Cell);
                    cell = toJoin.Neighbor;

                    yield return toJoin.Wall;
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
