namespace Fovero.UI;

public sealed class ActionPlayer
{
    public bool IsAnimated { get; set; } = true;

    public int AnimationSpeed { get; set; } = 18;

    public int MaximumSpeed { get; } = 20;

    public async Task Play<T>(IEnumerable<T> source, Action<T> doWork)
    {
        foreach (T item in source)
        {
            doWork(item);

            if (!IsAnimated)
            {
                continue;
            }

            // REVISIT: figure out a power series for calculating the delay
            var delay = TimeSpan.FromMilliseconds((MaximumSpeed - AnimationSpeed) * 10);
            await Task.Delay(delay);
        }
    }
}
