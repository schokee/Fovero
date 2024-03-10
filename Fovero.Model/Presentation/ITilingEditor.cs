namespace Fovero.Model.Presentation;

public interface IFormatEditor
{
    event Action FormatChanged;

    Maze CreateLayout();
}
