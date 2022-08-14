using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace July.Views.Controls;

public class VirtualizingGridPanel:
    GridPanel,
    IVirtualizingPanel,
    IPanel,
    IControl,
    IVisual,
    IDataTemplateHost,
    ILayoutable,
    IInputElement,
    IInteractive,
    INamed,
    IStyledElement,
    IStyleable,
    IAvaloniaObject,
    IStyleHost,
    ILogical,
    IResourceHost,
    IResourceNode,
    IDataContextProvider,
    ISupportInitialize
{
    private Size _availableSpace;
    private double _takenSpace;
    private int _canBeRemoved;
    private double _averageItemSize;
    private int _averageCount;
    private double _pixelOffset;
    private double _crossAxisOffset;
    private bool _forceRemeasure;
    
    public void ForceInvalidateMeasure()
    {
      InvalidateMeasure();
      _forceRemeasure = true;
    }

    public IVirtualizingController? Controller { get; set; }
    public bool IsFull { get; }
    public int OverflowCount { get; } = 6;
    public Orientation ScrollDirection { get; }
    public double AverageItemSize { get; }
    public double PixelOverflow { get; }
    public double PixelOffset { get; set; }
    public double CrossAxisOffset { get; set; }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _availableSpace = finalSize;
        _canBeRemoved = 0;
        _takenSpace = 0.0;
        _averageItemSize = 0.0;
        _averageCount = 0;
        Size size = base.ArrangeOverride(finalSize);
        _takenSpace += _pixelOffset;
        IVirtualizingController? controller = Controller;
        if (controller == null)
            return size;
        controller.UpdateControls();
        return size;
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        if (!_forceRemeasure)
        {
            Size size = availableSize;
            Size? previousMeasure = ((ILayoutable) this).PreviousMeasure;
            if ((previousMeasure.HasValue ? (size != previousMeasure.GetValueOrDefault() ? 1 : 0) : 1) == 0)
                goto label_4;
        }
        _forceRemeasure = false;
        _availableSpace = availableSize;
        Controller?.UpdateControls();
        label_4:
        return base.MeasureOverride(availableSize);
    }
    
    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      base.ChildrenChanged(sender, e);
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          IEnumerator enumerator1 = e.NewItems.GetEnumerator();
          try
          {
            while (enumerator1.MoveNext())
              this.UpdateAdd((IControl) enumerator1.Current);
            break;
          }
          finally
          {
            if (enumerator1 is IDisposable disposable)
              disposable.Dispose();
          }
        case NotifyCollectionChangedAction.Remove:
          IEnumerator enumerator2 = e.OldItems.GetEnumerator();
          try
          {
            while (enumerator2.MoveNext())
              this.UpdateRemove((IControl) enumerator2.Current);
            break;
          }
          finally
          {
            if (enumerator2 is IDisposable disposable)
              disposable.Dispose();
          }
      }
    }

    private void UpdateAdd(IControl child)
    {
      Rect bounds = this.Bounds;
      double spacing = 0;
      child.Measure(this._availableSpace);
      ++this._averageCount;
      double height = child.DesiredSize.Height;
      this._takenSpace += height + spacing;
      this.AddToAverageItemSize(height);
    }

    private void UpdateRemove(IControl child)
    {
      Rect bounds = Bounds;
      double spacing = 0;
      double height = child.DesiredSize.Height;
      _takenSpace -= height + spacing;
      RemoveFromAverageItemSize(height);
      if (_canBeRemoved <= 0)
        return;
      --_canBeRemoved;
    }

    private void AddToAverageItemSize(double value)
    {
      ++this._averageCount;
      this._averageItemSize += (value - this._averageItemSize) / (double) this._averageCount;
    }

    private void RemoveFromAverageItemSize(double value)
    {
      this._averageItemSize = (this._averageItemSize * (double) this._averageCount - value) / (double) (this._averageCount - 1);
      --this._averageCount;
    }
}