using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SharpStudioWpf;

public partial class TitleBar : UserControl
{
    public TitleBar()
    {
        InitializeComponent();
    }
    
    public void OnDraggingTitleBar(object sender, MouseButtonEventArgs e)
    {
        var window = Window.GetWindow(this)!;
        if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
        {
            window.WindowState = window.WindowState == WindowState.Normal
                ? WindowState.Maximized
                : WindowState.Normal;
        }
        else if (e.LeftButton == MouseButtonState.Pressed)
        {
            window.DragMove();
        }
    }
}