using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Mathematics.d2;
using SharpStudioAvalonia.Views;

namespace SharpStudioAvalonia.Editor;

public class ShapeLayer
{
    public Canvas Self { get; set; }
    public Canvas Parent { get; set; }
    public Camera Camera { get; set; }
    public readonly List<ReactiveShape> Shapes = [];
    private readonly Dictionary<ReactiveShape, Shape> _views = new();
    public AnchorRelation? AnchorRelation;
    private int _selectedShapeIndex = -1;
    
    public ShapeLayer(Canvas self, Canvas parent, Camera camera)
    {
        Self = self;
        Parent = parent;
        // SetupGeometry();
        Camera = camera;
        Camera.PropertyChanged += HandleCameraChange;
    }

    ~ShapeLayer()
    {
        Camera.PropertyChanged -= HandleCameraChange;
    }
    
    public void Render()
    {
        foreach (var shape in Shapes)
        {
            UpdateViewGeometry(shape);
        }

        AnchorRelation?.Render();
    }

    public int InShape(Point point)
    {
        for (var i = Shapes.Count - 1; i >= 0; i--)
        {
            var shape = Shapes[i];
            var inArea = shape switch
            {
                ReactiveRectangle rectangle => ShapeTools.InRectangle(point, rectangle),
                ReactiveCircle    circle    => ShapeTools.InCircle(point, circle),
                ReactivePolygon   polygon   => ShapeTools.InPolygon(point, polygon),
                _ => false,
            };
            if (inArea) return i;
        }
        return -1;
    }

    public int InAnchor(Point point)
    {
        return AnchorRelation?.InAnchor(point) ?? -1;
    }
    
    public bool IsSelected()
    {
        return _selectedShapeIndex > -1;
    }

    public void Deselect()
    {
        AnchorRelation?.Dispose();
        AnchorRelation = null;
        _selectedShapeIndex = -1;
    }

    public void Select(int index)
    {
        if (index < 0 || index >= Shapes.Count) return;
        Deselect();
        AnchorRelation = new AnchorRelation(Self, Shapes[index], Camera);
        _selectedShapeIndex = index;
        Render();
    }

    public void AddShape(ReactiveShape shape)
    {
        var color = shape.Color ?? "00ff00";
        Shape? view = null;
        if (shape is ReactiveCircle)
        {
            view = new Ellipse
            {
                Fill = new SolidColorBrush(Color.FromArgb(0x22, Convert.ToByte(color[..2], 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16))), //new BrushConverter().ConvertFrom("#22" + color) as Brush,
                Stroke = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(color[..2], 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16))), //new BrushConverter().ConvertFrom("#" + color) as Brush,
                StrokeThickness = 1
            };
        }
        else if (shape is ReactivePolygon or ReactiveRectangle)
        {
            view = new Polygon
            {
                Fill = new SolidColorBrush(Color.FromArgb(0x22, Convert.ToByte(color[..2], 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16))), //new BrushConverter().ConvertFrom("#22" + color) as Brush,
                Stroke = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(color[..2], 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16))), //new BrushConverter().ConvertFrom("#" + color) as Brush,
                StrokeThickness = 1
            };
        }
        if (view == null) return;
        Shapes.Add(shape);
        Self.Children.Add(view);
        _views.Add(shape, view);
    }

    public void RemoveShape(ReactiveShape shape)
    {
        if (!_views.Remove(shape, out var view)) return;
        Self.Children.Remove(view);
        Shapes.Remove(shape);
    }

    private void HandleCameraChange(object? sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        Render();
    }

    private void UpdateViewGeometry(ReactiveShape shape)
    {
        var view = _views[shape];
        if (shape is ReactiveRectangle rectangle)
        {
            var polygonView = view as Polygon;
            for (var i = 0; i < 4; i++)
            {
                var p = Camera.ConvertToScreen(ShapeTools.GetRectangleAnchor(rectangle, i * 2)).ToAvaloniaPoint();
                if (i >= polygonView!.Points.Count)
                    polygonView.Points.Add(p);
                else 
                    polygonView.Points[i] = p;
            }
        }
        else if (shape is ReactiveCircle circle)
        {
            view.Width = circle.Radius * 2 * Camera.Scale;
            view.Height = circle.Radius * 2 * Camera.Scale;
            Canvas.SetLeft(view, (circle.X - circle.Radius) * Camera.Scale + Camera.X);
            Canvas.SetTop(view, (circle.Y - circle.Radius) * Camera.Scale + Camera.Y);
        } 
        else if (shape is ReactivePolygon polygon)
        {
            var polygonView = view as Polygon;
            for (var i = 0; i < polygon.Points.Count; i++)
            {
                var abs = polygon.Points[i];
                var rel = Camera.ConvertToScreen(abs).ToAvaloniaPoint();
                var collection = polygonView!.Points;
                if (i < collection.Count)
                    collection[i] = rel;
                else
                    collection.Add(rel);
            }
            if (polygon.Points.Count < polygonView!.Points.Count)
            {
                for (var i = 0; i < polygonView.Points.Count - polygon.Points.Count; i++) 
                    polygonView.Points.RemoveAt(polygon.Points.Count);
            }
        }
    }
    
    public void SetupGeometry()
    {
        // Parent.Children.Add(this); 
        Canvas.SetLeft(Self, 0);
        Canvas.SetTop(Self, 0); 
        // Canvas.SetZIndex(Self, 1);
        Self.Width = Parent.Width; 
        Self.Height = Parent.Height;
    }
}