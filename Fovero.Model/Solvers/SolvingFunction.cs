namespace Fovero.Model.Solvers;

public delegate IEnumerable<Path<INode>> SolvingFunction(INode origin, INode goal);
