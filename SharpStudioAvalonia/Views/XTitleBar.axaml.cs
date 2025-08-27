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
    }
    
    private Window? window => this.GetVisualRoot() as Window;
    
    private void OnMouseDown(object? sender, PointerPressedEventArgs e)
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

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        window!.WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClicked(object? sender, RoutedEventArgs routedEventArgs)
    {
        window!.WindowState = window.WindowState == WindowState.Normal
            ? WindowState.Maximized
            : WindowState.Normal;
    }
    
    private void OnCloseClicked(object? sender, RoutedEventArgs routedEventArgs)
    {
        window!.Close();
    }
}