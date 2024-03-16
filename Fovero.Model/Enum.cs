using System.ComponentModel;
using System.Reflection;

namespace Fovero.Model;

public static class Enum<T> where T : Enum
{
    public static IReadOnlyDictionary<T, string> ToDictionary()
    {
        return Enum
            .GetValues(typeof(T))
            .Cast<T>()
            .ToDictionary(x => x, GetDescriptionOrDefault);
    }

    public static string GetDescriptionOrDefault(T value)
    {
        return typeof(T).GetField(value.ToString())!.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
    }
}
