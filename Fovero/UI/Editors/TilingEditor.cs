using System.Runtime.CompilerServices;
using Caliburn.Micro;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public abstract class TilingEditor(string name) : PropertyChangedBase, ITilingEditor
{
    public event System.Action FormatChanged;

    public string Name { get; } = name;

    public abstract ITiling CreateTiling();

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
