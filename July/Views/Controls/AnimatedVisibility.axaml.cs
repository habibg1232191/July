using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using ReactiveUI;

namespace July.Views.Controls;

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
                IsVisible = true;
                await ShowAnimation?.RunAsync(this, new Clock(), _cancellationToken)!;
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
                await HideAnimation?.RunAsync(this, new Clock(), _cancellationToken)!;
                IsVisible = false;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}