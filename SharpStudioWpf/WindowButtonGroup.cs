using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SharpStudioWpf;

public class WindowButtonGroup : Control
{
    static WindowButtonGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowButtonGroup),
            new FrameworkPropertyMetadata(typeof(WindowButtonGroup)));
    }
    
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Min") is Button minimizeButton)
        {
            minimizeButton.Click += OnWindowMinimizeButtonClicked;
        }
        if (GetTemplateChild("PART_Max") is Button maximizeButton)
        {
            maximizeButton.Click += OnWindowMaximizeButtonClicked;
        }
        if (GetTemplateChild("PART_Close") is Button closeButton)
        {
            closeButton.Click += OnWindowCloseButtonClicked;
        }
    }

    private void OnWindowMinimizeButtonClicked(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null) 
            window.WindowState = WindowState.Minimized;
    }
    
    private void OnWindowMaximizeButtonClicked(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null)
            window.WindowState = window.WindowState == WindowState.Normal
                ? WindowState.Maximized
                : WindowState.Normal;
    }
    
    private void OnWindowCloseButtonClicked(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}