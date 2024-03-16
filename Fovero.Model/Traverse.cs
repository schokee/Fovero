namespace Fovero.Model;

internal static class Traverse
{
    public static IEnumerable<T> Prioritised<T, TPriority>(T startingFrom, TPriority startPriority, Func<T, IEnumerable<T>> selectNeighbors, Func<T, TPriority> prioritise)
    {
        var queue = new PriorityQueue<T, TPriority>();

        queue.Enqueue(startingFrom, startPriority);

        while (queue.Count > 0)
        {
            var next = queue.Dequeue();

            yield return next;

            foreach (var neighbor in selectNeighbors(next))
            {
                queue.Enqueue(neighbor, prioritise(neighbor));
            }
        }
    }

    public static IEnumerable<T> BreadthFirst<T>(T startingFrom, Func<T, IEnumerable<T>> selectNeighbors)
    {
        var queue = new Queue<T>();

        queue.Enqueue(startingFrom);

        while (queue.Count > 0)
        {
            var next = queue.Dequeue();

            yield return next;

            foreach (var neighbor in selectNeighbors(next))
            {
                queue.Enqueue(neighbor);
            }
        }
    }

    public static IEnumerable<T> DepthFirst<T>(T startingFrom, Func<T, IEnumerable<T>> selectNeighbors)
    {
        var stack = new Stack<T>();

        stack.Push(startingFrom);

        while (stack.Count > 0)
        {
            var next = stack.Pop();

            yield return next;

            foreach (var neighbor in selectNeighbors(next))
            {
                stack.Push(neighbor);
            }
        }
    }
}
