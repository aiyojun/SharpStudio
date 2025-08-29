using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
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
        var style = (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://SharpStudioAvalonia/Quartz/Styles.axaml"));
        ClipToBounds = false;
        Focusable = true;
        Styles.Add(style);
        Width = 200;
        CornerRadius = new CornerRadius(6);
        Identifier = identifier;
        Content = new Grid()
        {
            Children =
            {
                new TextBlock {Text = "Hello, world!"}
            }
        };
        XProperty.Changed.AddClassHandler<Component>((_, _) => SetGeometry());
        YProperty.Changed.AddClassHandler<Component>((_, _) => SetGeometry());
        SetGeometry();
    }
    
    public static Component Parse(string data)
    {
        var json = JObject.Parse(data);
        var vStack  = new StackPanel { Orientation = Orientation.Vertical };
        var id      = json.GetValue("id"  )!.Value<string>()!;
        var name    = json.GetValue("name")!.Value<string>()!;
        var x       = json.GetValue("x"   )!.Value<double>() ;
        var y       = json.GetValue("y"   )!.Value<double>() ;
        var profile = json["profile"] as JArray;
        var component = new Component(id);
        vStack.Children.Add((TemplateGenerator.GetHeader(name) as Control)!);
        var ports = new Dictionary<string, Port>();
        for (var i = 0; i < profile!.Count; i++)
        {
            var widget = (profile[i] as JObject)!;
            var type = widget.GetValue("type")!.Value<string>()!;
            var label = widget.GetValue("label")!.Value<string>()!;
            if (type == "import")
            {
                var field = widget.GetValue("field")!.Value<string>()!;
                vStack.Children.Add((TemplateGenerator.GetImport(label, out var view) as Control)!);
                ports.Add(field, Port.CreateImport(field, component, view));
            }
            else if (type == "export")
            {
                var field = widget.GetValue("field")!.Value<string>()!;
                vStack.Children.Add((TemplateGenerator.GetExport(label, out var view) as Control)!);
                ports.Add(field, Port.CreateExport(field, component, view));
            }
            else
            {
                vStack.Children.Add((TemplateGenerator.Parse((profile[i] as JObject)!) as Control)!);
            }
        }
        var border = new Border { Classes = { "Node" }, Child = vStack };
        component.Content = border;
        component.X = x;
        component.Y = y;
        component._ports = ports;
        return component;
    }
    
    private Dictionary<string, Port> _ports = new();

    public Port GetPort(string name)
    {
        return _ports[name];
    }

    public List<Port> Ports => _ports.Values.ToList();

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