using Avalonia.Controls;

namespace SharpStudioAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // this.GetObservable(WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(_ =>
        // {
        //     if (WindowState == WindowState.Maximized)
        //     {
        //         // 给窗口内容加一个边距，避免超出屏幕
        //         Root.Margin = new Thickness(7); // Windows 默认边框大约 7px
        //     }
        //     else
        //     {
        //         Root.Margin = new Thickness(0);
        //     }
        // }));
    }
}