namespace Fovero.Model.DataStructures;

public class ExtendedQueue<T>
{
    private readonly Lazy<Queue<T>> _queue = new();
    private readonly Lazy<PriorityQueue<T, float>> _priorityQueue = new();

    private int QueueCount => _queue.IsValueCreated ? _queue.Value.Count : 0;
    private int PriorityQueueCount => _priorityQueue.IsValueCreated ? _priorityQueue.Value.Count : 0;

    public int Count => QueueCount + PriorityQueueCount;

    public void Enqueue(T element, float priority)
    {
        if (priority < 0)
            throw new InvalidOperationException($"{nameof(priority)} parameter cannot be negative.");

        if (priority == 0)
            _queue.Value.Enqueue(element);
        else
            _priorityQueue.Value.Enqueue(element, priority);
    }

    public T Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException("The queue is empty.");

        return QueueCount > 0
            ? _queue.Value.Dequeue()
            : _priorityQueue.Value.Dequeue();
    }

    public T Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("The queue is empty.");

        return QueueCount > 0
            ? _queue.Value.Peek()
            : _priorityQueue.Value.Peek();
    }

    public void Clear()
    {
        if (_queue.IsValueCreated)
            _queue.Value.Clear();

        if (_priorityQueue.IsValueCreated)
            _priorityQueue.Value.Clear();
    }
}
