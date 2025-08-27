using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Newtonsoft.Json.Linq;

namespace SharpStudioAvalonia.Quartz;

public class Component : UserControl
{
    public static readonly StyledProperty<bool> SelectedProperty = AvaloniaProperty.Register<Component, bool>(nameof(Selected));

    public bool Selected
    {
        get => GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }
    
    public static readonly StyledProperty<string> IdentifierProperty = AvaloniaProperty.Register<Component, string>(nameof(Identifier));

    public string Identifier
    {
        get => GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }
    
    public static readonly StyledProperty<double> XProperty = AvaloniaProperty.Register<Component, double>(nameof(X));
    
    public double X
    {
        get => GetValue(XProperty);
        set => SetValue(XProperty, value);
    }
    
    public static readonly StyledProperty<double> YProperty = AvaloniaProperty.Register<Component, double>(nameof(Y));

    public double Y
    {
        get => GetValue(YProperty);
        set => SetValue(YProperty, value);
    }
    
    private Component(string identifier)
    {
        
        var style = (IStyle)AvaloniaXamlLoader.Load(
            new Uri("avares://SharpStudioAvalonia/Quartz/Preset.axaml")
        );

        // 添加到当前控件资源
        Styles.Add(style);
        
        Width = 200;
        // Height = 40;
        BorderThickness = new Thickness(1);
        // BorderBrush = new SolidColorBrush(Colors.White);
        // Background = Brushes.Coral;
        CornerRadius = new CornerRadius(6);
        Identifier = identifier;

        Content = new Grid()
        {
            Children =
            {
                new TextBlock {Text = "Hello, world!"}
            }
        };
        
        XProperty.Changed.AddClassHandler<Component>((x, e) => SetGeometry());
        YProperty.Changed.AddClassHandler<Component>((x, e) => SetGeometry());
        SetGeometry();
    }
    
    public static Component Create(double x = 0, double y = 0)
    {
        return new Component(Guid.NewGuid().ToString().Replace("-", "")) { X = x, Y = y };
    }

    public static Component Parse(string data)
    {
        var json = JObject.Parse(data);
        var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
        var id =  json["id"].ToString();
        var form = json["form"] as JArray;
        var x = json["x"].ToObject<double>();
        var y = json["y"].ToObject<double>();
        var name = json["name"].ToString();
        stackPanel.Children.Add(TemplateGenerator.GetHeader(name) as Control);
        for (var i = 0; i < form.Count; i++)
        {
            stackPanel.Children.Add(TemplateGenerator.Parse(form[i] as JObject) as Control);
            // var input = form[i];
            // var type = input["type"].ToString();
            // var label = input["label"].ToString();
            // if (type == "text")
            // {
            //     stackPanel.Children.Add(new TextBlock() { Text = label, FontSize = 10 });
            // }
            // else if (type == "input")
            // {
            //     stackPanel.Children.Add(
            //         new StackPanel
            //         {
            //             HorizontalAlignment = HorizontalAlignment.Stretch,
            //             Orientation = Orientation.Horizontal,
            //             Children =
            //             {
            //                 new TextBlock { Text = label, FontSize = 10 },
            //                 new TextBox() { HorizontalAlignment = HorizontalAlignment.Stretch },
            //             }
            //         });
            // }
        }

        var border = new Border
        {
            // BorderThickness = new Thickness(1),
            // BorderBrush = Brushes.DimGray,
            Background = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            CornerRadius = new CornerRadius(6),
            Padding = Thickness.Parse("5"),
            Child = stackPanel,
        };
        return new Component(id) { Content = border, X = x, Y = y };
    }

    public void Save(Mathematics.d2.Point cursor)
    {
        _cachedPosition[0] = X;
        _cachedPosition[1] = Y;
        _cachedCursor[0] = cursor.X;
        _cachedCursor[1] = cursor.Y;
    }

    public void MoveTo(Mathematics.d2.Point cursor)
    {
        X = _cachedPosition[0] + cursor.X - _cachedCursor[0];
        Y = _cachedPosition[1] + cursor.Y - _cachedCursor[1];
    }

    private double[] _cachedPosition = [0, 0];
    
    private double[] _cachedCursor = [0, 0];
    
    public event EventHandler<Mathematics.d2.Point>? Moved;
    
    private void SetGeometry()
    {
        Canvas.SetLeft(this, X);
        Canvas.SetTop (this, Y);
        Moved?.Invoke(this, new Mathematics.d2.Point(X, Y));
    }

}