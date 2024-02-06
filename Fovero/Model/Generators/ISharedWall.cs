namespace Fovero.Model.Generators;

public interface ISharedWall
{
    IEnumerable<ushort> Neighbors { get; }

    ushort NeighborA => Neighbors.ElementAt(0);
    ushort NeighborB => Neighbors.ElementAt(1);
}
