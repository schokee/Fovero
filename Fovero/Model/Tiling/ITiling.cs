namespace Fovero.Model.Tiling;

public interface ITiling
{
    IEnumerable<ITile> Generate();
}
