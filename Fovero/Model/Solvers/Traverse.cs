﻿namespace Fovero.Model.Solvers;

internal static class Traverse
{
    public static IEnumerable<T> Prioritised<T, TPriority>(T startingFrom, Func<T, IEnumerable<T>> selectNeighbors, Func<T, TPriority> prioritise)
    {
        var queue = new PriorityQueue<T, TPriority>();

        queue.Enqueue(startingFrom, default);

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
}