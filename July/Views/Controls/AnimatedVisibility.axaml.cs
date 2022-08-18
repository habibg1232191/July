using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using HarfBuzzSharp;
using ReactiveUI;

namespace July.Views.Controls;

[PseudoClasses("visible")]
public class AnimatedVisibility : ContentControl
{
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;

    public static readonly StyledProperty<Animation?> ShowAnimationProperty =
        AvaloniaProperty.Register<AnimatedVisibility, Animation?>(nameof(ShowAnimation));
    
    public static readonly StyledProperty<Animation?> HideAnimationProperty =
        AvaloniaProperty.Register<AnimatedVisibility, Animation?>(nameof(HideAnimation));
    
    public static readonly StyledProperty<bool> VisibilityProperty =
        AvaloniaProperty.Register<AnimatedVisibility, bool>(nameof(Visibility));
    
    public Animation? ShowAnimation
    {
        get => GetValue(ShowAnimationProperty);
        set => SetValue(ShowAnimationProperty, value);
    }
    
    public Animation? HideAnimation
    {
        get => GetValue(HideAnimationProperty);
        set => SetValue(HideAnimationProperty, value);
    }

    public bool Visibility
    {
        get => GetValue(VisibilityProperty);
        set => SetValue(VisibilityProperty, value);
    }

    public AnimatedVisibility()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        this.WhenAnyValue(x => x.Visibility).Subscribe(OnVisibilityChange);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (Transitions != null)
            foreach (var transition in Transitions)
            {
                var st = 0d;
                transition.Property.Changed.Subscribe(onNext: args =>
                {
                    if (args.Priority == BindingPriority.StyleTrigger)
                    {
                        st = (double) args.NewValue!;
                    }
                    else if ((double) args.NewValue! == st)
                    {
                        Console.WriteLine("Finish Transition");
                        IsVisible = Visibility;
                    }
                });
                break;
            }
    }

    private async void OnVisibilityChange(bool newValue)
    {
        _cancellationTokenSource.Cancel();
        
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        
        if (newValue)
        {
            if (ShowAnimation == null) return;
            try
            {
                // IsVisible = true;
                PseudoClasses.Set("visible", true);
                // await ShowAnimation?.RunAsync(this, new Clock(), _cancellationToken)!;
            }
            catch (Exception)
            {
                // ignored
            }
        }
        else
        {
            if (HideAnimation == null) return;
            try
            {
                // await HideAnimation?.RunAsync(this, new Clock(), _cancellationToken)!;
                // IsVisible = false;
                PseudoClasses.Set("visible", false);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}