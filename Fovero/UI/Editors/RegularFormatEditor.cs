﻿using Fovero.Model.Presentation;
using Fovero.Model.Tiling;

namespace Fovero.UI.Editors;

public class RegularFormatEditor(string name, Func<ushort, ushort, ITiling> createTiling) : FormatEditor(name)
{
    private int _columns;
    private int _rows;

    private Func<ushort, ushort, ITiling> TilingMethod { get; } = createTiling;

    public int Columns
    {
        get => _columns;
        set => SetFormat(ref  _columns, value);
    }

    public int Rows
    {
        get => _rows;
        set => SetFormat(ref _rows, value);
    }

    public override Maze CreateLayout()
    {
        return new Maze(TilingMethod((ushort)Columns, (ushort)Rows));
    }
}
