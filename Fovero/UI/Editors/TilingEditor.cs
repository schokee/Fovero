using System.Runtime.CompilerServices;
using Caliburn.Micro;

namespace Fovero.UI.Editors;

public abstract class FormatEditor(string name) : PropertyChangedBase, IFormatEditor
{
    public event System.Action FormatChanged;

    public string Name { get; } = name;

    public abstract Maze CreateLayout();

    public override string ToString()
    {
        return Name;
    }

    protected bool SetFormat<T>(ref T newValue, T oldValue, [CallerMemberName] string propertyName = null)
    {
        if (Set(ref newValue, oldValue, propertyName))
        {
            FormatChanged?.Invoke();
            return true;
        }

        return false;
    }
}
