using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace July.Views.Controls;

public class GridPanel : Panel
{
    private IEnumerable<IControl[]> _chunkItems = new List<IControl[]>();
    private int _columnsCount;
    
    protected override Size MeasureOverride(Size availableSize)
    {
        _chunkItems = GetChunkItemsInOneColumn(out _columnsCount, availableSize);
        var height = 0d;
        foreach (var columnChild in _chunkItems)
        {
            height += columnChild.Max(x => x.DesiredSize.Height);
        }
        return new Size(availableSize.Width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var y = 0d;
        var isFirst = true;
        var spaceWidthColumn = 0d;
        foreach (var columnChild in _chunkItems)
        {
            if (isFirst)
            {
                var widthColumn = finalSize.Width - columnChild.Sum(child => child.DesiredSize.Width);
                spaceWidthColumn = widthColumn / _columnsCount;
                isFirst = false;
            }
            var x = columnChild.Length > 1 ? 0d : finalSize.Width/2 - columnChild[0].DesiredSize.Width/2;
            columnChild.ToList().ForEach(child =>
            {
                child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
                x += child.DesiredSize.Width + spaceWidthColumn;
            });
            y += columnChild.Max(child => child.DesiredSize.Height);
        }
        
        return finalSize;
    }

    private IEnumerable<IControl[]> GetChunkItemsInOneColumn(out int columnsCount, Size parentSize)
    {
        var measureChildren = Children.ToList();
        var width = 0d;
        columnsCount = 1;
        
        for (int i = 1; i < measureChildren.Count + 1; i++)
        {
            var child = measureChildren[i - 1];
            child.Measure(parentSize);
            width += child.DesiredSize.Width;
            if (width + measureChildren[i - (i != measureChildren.Count ? 0 : 1)].DesiredSize.Width < parentSize.Width)
                columnsCount++;
        }

        return measureChildren.Chunk(columnsCount);
    }
}