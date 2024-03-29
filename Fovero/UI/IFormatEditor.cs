﻿using Fovero.Model.Presentation;

namespace Fovero.UI;

public interface IFormatEditor
{
    event Action FormatChanged;

    Maze CreateLayout();
}
