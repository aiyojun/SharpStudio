using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SharpStudioAvalonia.Editor;

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
    
    public static readonly StyledProperty<string> CurrentModuleNameProperty = AvaloniaProperty.Register<MainWindow, string>(nameof(CurrentModuleName), "");

    public string CurrentModuleName
    {
        get => GetValue(CurrentModuleNameProperty);
        set => SetValue(CurrentModuleNameProperty, value);
    }

    private void OnDockButtonClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine($"sender {sender}");
        if (Equals(sender, ModuleInspection))
        {
            ContentControl.Content = new Palette { Source = new Bitmap(AssetLoader.Open(new Uri("avares://SharpStudioAvalonia/Assets/chessboard.png"))) };  // new TextBlock { Text = "检测模块" };
        }
        else if (Equals(sender, ModuleHistory))
        {
            // ContentControl.Content = new TextBlock { Text = "历史模块" };
            ContentControl.Content = new Tablet() { Source = new Bitmap(AssetLoader.Open(new Uri("avares://SharpStudioAvalonia/Assets/chessboard.png"))) };
            
            
        }
        else if (Equals(sender, ModulePostprocess))
        {
            ContentControl.Content = new Flowchart();
        }
    }
    
}