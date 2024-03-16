namespace Fovero.Model;

public interface IVisitable
{
    bool HasBeenVisited { get; }

    uint VisitCount { get; set; }
}
