using MoreLinq;

namespace Fovero.Model.Generators;

public record BuildingStrategy<T>(string Name, Func<IEnumerable<T>, IEnumerable<T>> SelectWallsToBeOpened) where T : IWall
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

            IEnumerable<T> Build(IEnumerable<T> allWalls)
            {
                var shuffledWalls = allWalls.Shuffle().ToList();
                var sets = new DisjointSet<int>(shuffledWalls.SelectMany(wall => (IEnumerable<int>) [wall.NeighborA, wall.NeighborB]));

                foreach (var wall in shuffledWalls.Where(x => sets.AreDisjoint(x.NeighborA, x.NeighborB)))
                {
                    sets.Merge(wall.NeighborA, wall.NeighborB);
                    yield return wall;
                }
            }
        }
    }
}
