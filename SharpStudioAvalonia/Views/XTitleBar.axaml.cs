using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace SharpStudioAvalonia.Views;

public partial class XTitleBar : UserControl
{
    public XTitleBar()
    {
        InitializeComponent();
        AttachedToVisualTree += (sender, e) =>
        {
            window!.GetObservable(Window.WindowStateProperty).Subscribe(state =>
            {
                MiddleButton = GetMiddleWindowButtonPath(state);
                Console.WriteLine($"Window state: {state} {MiddleButton}");
            });
            MiddleButton = GetMiddleWindowButtonPath(window?.WindowState);
        };
        Loaded += (sender, e) =>
        {
            Console.WriteLine($"> button: {MiddleButton}");
        };
    }
    
    public static readonly StyledProperty<string> MiddleButtonProperty =
        AvaloniaProperty.Register<XTitleBar, string>(nameof(MiddleButton));

    public string MiddleButton
    {
        get => GetValue(MiddleButtonProperty);
        set => SetValue(MiddleButtonProperty, value);
    }

    private static string GetMiddleWindowButtonPath(WindowState? state)
    {
        const string normal = "M1,1L1,11 11,11 11,1z M0,0L12,0 12,12 0,12z";
        if (state == null) return normal;
        return state != WindowState.Maximized ? normal
            : "M256 0v256H0v768h768V768h256V0H256z m426.667 938.667H85.333V341.333h597.334v597.334z m256-256H768V256H341.333V85.333h597.334v597.334z";
    }

    private Window? window => this.GetVisualRoot() as Window;
    
    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(window).Properties;
        if (props.IsLeftButtonPressed && e.ClickCount == 2)
        {
            window!.WindowState = window.WindowState == WindowState.Normal
                ? WindowState.Maximized
                : WindowState.Normal;
            return;
        }
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            window!.BeginMoveDrag(e);
        }
    }

    // private string WindowButtonPath { get; set; }
    // =>
        // (this.GetVisualRoot() as Window)!.WindowState == WindowState.Normal
        //     ? "M1,1L1,11 11,11 11,1z M0,0L12,0 12,12 0,12z" 
        //     : "M1,4.56L1,14.56 11,14.56 11,4.56z M4,1L4,3.56 12,3.56 12,11 14,11 14,1z M3,0L15,0 15,12 12,12 12,15.56 0,15.56 0,3.56 3,3.56z";

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine($"[Event] OnMinimizeClicked");
        // var properties = e.GetCurrentPoint(null).Properties;
        // if (!properties.IsLeftButtonPressed) return;
        var window = this.GetVisualRoot() as Window;
        window!.WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClicked(object? sender, RoutedEventArgs routedEventArgs)
    {
        Console.WriteLine($"[Event] OnMaximizeClicked");
        // var properties = e.GetCurrentPoint(null).Properties;
        // if (!properties.IsLeftButtonPressed) return;
        var window = this.GetVisualRoot() as Window;
        window!.WindowState = window.WindowState == WindowState.Normal
            ? WindowState.Maximized
            : WindowState.Normal;
    }
    
    private void OnCloseClicked(object? sender, RoutedEventArgs routedEventArgs)
    {
        Console.WriteLine($"[Event] OnCloseClicked");
        // var properties = e.GetCurrentPoint(null).Properties;
        // if (!properties.IsLeftButtonPressed) return;
        var window = this.GetVisualRoot() as Window;
        window!.Close();
    }
}