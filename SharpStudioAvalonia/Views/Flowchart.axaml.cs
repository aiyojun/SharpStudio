using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public Component CreateComponent(double x = 0, double y = 0)
    {
        var component = Component.Create(x, y);
        Console.WriteLine($"- Add a component : {component.Identifier}");
        NodeLayer.Children.Add(component);
        _components.Add(component);
        return component;
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

    public Component? HitTest(PointerPressedEventArgs e)
    {
        for (var i = _components.Count - 1; i >= 0; i--)
        {
            var component = _components[i];
            // var view = component.View!;
            if (component.InputHitTest(e.GetPosition(component)) != null)
                return component;
        }
        return null;
    }

    // public FlowchartViewModel? FlowchartViewModel => DataContext as FlowchartViewModel;
    
    public Flowchart()
    {
        InitializeComponent();
        NodeLayer.RenderTransform = new MatrixTransform { Matrix = Matrix.Identity };
        EdgeLayer.RenderTransform = new MatrixTransform { Matrix = Matrix.Identity };
        Camera.PropertyChanged += HandleCameraChange;
        {
            // var source = CreateComponent(100, 150);
            // var target = CreateComponent(500, 200);
            var source = ParseComponent( @"{ 'id': 'xxxxxx0111', 'name': 'Read', 'x': 100, 'y': 150, 'form': [{'type': 'text', 'label': 'Input'}] }");
            var target = ParseComponent( @"{ 'id': 'xxxxxx0111', 'name': 'Write', 'x': 500, 'y': 200, 'form': [{'type': 'text-right', 'label': 'Output'}, {'type': 'text', 'label': 'Input'}, {'type': 'input', 'label': 'Filepath'}] }");
            Connect(source, target);
        }
        // Dispatcher.UIThread.Invoke(Test, DispatcherPriority.Background, CancellationToken.None, TimeSpan.FromSeconds(2));
    }

    public void Connect(Component source, Component target)
    {
        var edge = new Edge(new WeakReference<Component>(source), new WeakReference<Component>(target));
        EdgeLayer.Children.Add(edge);
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
            if (hitTarget is { } pressedComponent)
            {
                _selectedComponent = pressedComponent;
                pressedComponent.Save(Camera.ConvertToWorld(cursorPosition));
                // Console.WriteLine($"- Hit a component : {_selectedComponent.Identifier}");
                _action = UserAction.DragNode;
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
    }

    private void OnMouseUp(object? sender, PointerReleasedEventArgs e)
    {
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
}

