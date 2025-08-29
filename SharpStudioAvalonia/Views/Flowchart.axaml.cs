using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Mathematics.d2;
using SharpStudioAvalonia.Quartz;
using DPoint = Mathematics.d2.Point;
using APoint = Avalonia.Point;
using Component = SharpStudioAvalonia.Quartz.Component;

namespace SharpStudioAvalonia.Views;

public static class PointExtensions
{
    public static DPoint ToD2Point(this APoint p)
    {
        return new DPoint(p.X, p.Y);
    }
    
    public static APoint ToAvaloniaPoint(this DPoint p)
    {
        return new APoint(p.X, p.Y);
    }
}

public partial class Flowchart : UserControl
{
    public readonly Camera Camera = new();
    private UserAction _action = UserAction.None;
    private readonly List<Component> _components = [];
    private readonly List<Edge> _edges = [];
    private Component? _selectedComponent;
    private Port? _startPort;
    private Edge? _activeEdge;
    
    public Flowchart()
    {
        InitializeComponent();
        NodeLayer.RenderTransform = new MatrixTransform { Matrix = Matrix.Identity };
        EdgeLayer.RenderTransform = new MatrixTransform { Matrix = Matrix.Identity };
        Camera.PropertyChanged += HandleCameraChange;
        {
            var source = ParseComponent( @"{ 'id': 'xxxxxx0111', 'name': 'Read', 'x': 100, 'y': 150, 'profile': [{'type': 'export', 'field': 'xpo1', 'label': 'Output'}, {'type': 'import', 'field': 'xpi1', 'label': 'Input'}, {'type': 'selection', 'label': 'Options'}, {'type': 'check', 'label': 'Need'}, {'type': 'image', 'label': 'Image'}] }");
            var target = ParseComponent( @"{ 'id': 'xxxxxx0122', 'name': 'Write', 'x': 500, 'y': 200, 'profile': [{'type': 'import', 'field': 'ypi1', 'label': 'Input'}, {'type': 'input', 'label': 'ID'}, {'type': 'button', 'label': 'Filepath', 'tips': 'Browse'}] }");
            Connect(source.GetPort("xpo1"), target.GetPort("ypi1"));
        }
        // Dispatcher.UIThread.Invoke(Test, DispatcherPriority.Background, CancellationToken.None, TimeSpan.FromSeconds(2));
    }
    
    public Component ParseComponent(string json)
    {
        var component = Component.Parse(json);
        Console.WriteLine($"- Add a component : {component.Identifier}");
        NodeLayer.Children.Add(component);
        _components.Add(component);
        return component;
    }

    public void RemoveComponent(Component component)
    {
        NodeLayer.Children.Remove(component);
        _components.Remove(component);
    }

    public void Connect(Port source, Port target)
    {
        var edge = Edge.CreateEdge(source, target, new SolidColorBrush(Colors.White));  //new Edge(source, target);
        EdgeLayer.Children.Add(edge);
        _edges.Add(edge);
    }
    
    private void OnMouseDown(object? sender, PointerPressedEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(Container);
        var cursorProps = cursorPoint.Properties;
        var cursorPosition = cursorPoint.Position.ToD2Point();
        var isPressedCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        if (!isPressedCtrl && cursorProps.IsLeftButtonPressed)
        {
            var hitTarget = HitTest(e);
            if (hitTarget is Port { IsExport: true } port)
            {
                _startPort = port;
                _activeEdge = Edge.CreateActiveEdge(port, Camera.ConvertToWorld(cursorPosition));
                _action = UserAction.ConnectEdge;
                EdgeLayer.Children.Add(_activeEdge);
            }
            else if (hitTarget is Component component)
            {
                _selectedComponent = component;
                component.Save(Camera.ConvertToWorld(cursorPosition));
                _action = UserAction.DragNode;
            }
            else if (hitTarget is Edge edge)
            {
                Console.WriteLine($"hit edge {edge}");
                EdgeLayer.Children.Remove(edge);
                _edges.Remove(edge);
            }
            else
            {
                
            }
        }
        else if (cursorProps.IsMiddleButtonPressed)  // else if (e.MiddleButton == MouseButtonState.Pressed)
        {
            // _cursor.Buttons = 4; 
            Camera.Save(cursorPosition);
            _action = UserAction.DragPalette;
        }
    }

    private void OnMouseMove(object? sender, PointerEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(Container);
        var cursorProps = cursorPoint.Properties;
        var cursorPosition = cursorPoint.Position.ToD2Point();
        if (_action == UserAction.DragPalette)
        {
            Camera.MoveTo(cursorPosition);
        }
        else if (_action == UserAction.DragNode)
        {
            _selectedComponent!.MoveTo(Camera.ConvertToWorld(cursorPosition));
        }
        else if (_action == UserAction.ConnectEdge)
        {
            _activeEdge!.UpdateEndpoint(Camera.ConvertToWorld(cursorPosition));
        }
    }

    private void OnMouseUp(object? sender, PointerReleasedEventArgs e)
    {
        if (_action == UserAction.ConnectEdge)
        {
            var hitPort = HitTestPort(e);
            if (hitPort is { IsImport: true })
            {
                Connect(_startPort!, hitPort);
            }
            EdgeLayer.Children.Remove(_activeEdge!);
        }
        _action = UserAction.None;
    }

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        Camera.ZoomTo(Camera.Scale * (e.Delta.Y > 0 ? 1.1 : 0.9), e.GetCurrentPoint(Container).Position.ToD2Point());
    }
    
    private void HandleCameraChange(object? sender, PropertyChangedEventArgs e)
    {
        // var transform = (MatrixTransform) NodeLayer.RenderTransform!;
        var s = Matrix.CreateScale(Camera.Scale, Camera.Scale);
        var t = Matrix.CreateTranslation(Camera.X, Camera.Y);
        var transform = s * t;
        (NodeLayer.RenderTransform as MatrixTransform)!.Matrix = transform;
        (EdgeLayer.RenderTransform as MatrixTransform)!.Matrix = transform;
    }
    
    public object? HitTest(PointerPressedEventArgs e)
    {
        var port = HitTestPort(e);
        if (port != null) return port;
        var component = HitTestComponent(e);
        if (component != null) return component;
        var edge = HitTestEdge(e);
        if (edge != null) return edge;
        return null;
    }

    public Component? HitTestComponent(PointerPressedEventArgs e)
    {
        for (var i = _components.Count - 1; i >= 0; i--)
        {
            var component = _components[i];
            if (component.InputHitTest(e.GetPosition(component)) != null)
                return component;
        }
        return null;
    }

    public Port? HitTestPort(PointerEventArgs e)
    {
        for (var i = _components.Count - 1; i >= 0; i--)
        {
            var component = _components[i];
            for (var j = component.Ports.Count - 1; j >= 0; j--)
            {
                var port = component.Ports[j];
                var view = port.View;
                if (view.InputHitTest(e.GetPosition(view)) != null)
                {
                    return port;
                }
            }
        }
        return null;
    }

    public Edge? HitTestEdge(PointerPressedEventArgs e)
    {
        StringBuilder sb = new StringBuilder("HitTestEdge : ");
        var point = Camera.ConvertToWorld(e.GetPosition(Container).ToD2Point()).ToAvaloniaPoint();
        var pen = new Pen(Brushes.Black, 2);
        for (var i = _edges.Count - 1; i >= 0; i--)
        {
            var edge = _edges[i];
            if (edge.Data!.StrokeContains(pen, point))
            {
                sb.Append(edge.Data);
                return edge;
            }
        }
        sb.Append("; Count : ").Append(_edges.Count);
        Console.WriteLine(sb.ToString());
        return null;
    }
}

