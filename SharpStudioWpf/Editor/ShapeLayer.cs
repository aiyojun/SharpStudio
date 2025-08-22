using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SharpStudioWpf.Editor;

namespace SharpStudioWpf.Editor;

public class ShapeLayer : Canvas
{
    public ViewportMatrix2D Viewport { get; set; }
    public readonly List<ReactiveShape> Shapes = [];
    private readonly Dictionary<ReactiveShape, Shape> _views = new();
    public AnchorRelation? AnchorRelation;
    private int _selectedShapeIndex = -1;
    
    public ShapeLayer(Canvas parent, ViewportMatrix2D viewport)
    {
        SetGeometry(parent);
        Viewport = viewport;
        Viewport.PropertyChanged += HandleViewportChange;
    }

    ~ShapeLayer()
    {
        Viewport.PropertyChanged -= HandleViewportChange;
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
        AnchorRelation = new AnchorRelation(this, Shapes[index], Viewport);
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
                Fill = new BrushConverter().ConvertFrom("#22" + color) as Brush,
                Stroke = new BrushConverter().ConvertFrom("#" + color) as Brush,
                StrokeThickness = 1
            };
        }
        else if (shape is ReactivePolygon or ReactiveRectangle)
        {
            view = new Polygon
            {
                Fill = new BrushConverter().ConvertFrom("#22" + color) as Brush,
                Stroke = new BrushConverter().ConvertFrom("#" + color) as Brush,
                StrokeThickness = 1
            };
        }
        if (view == null) return;
        Shapes.Add(shape);
        Children.Add(view);
        _views.Add(shape, view);
    }

    public void RemoveShape(ReactiveShape shape)
    {
        if (!_views.ContainsKey(shape)) return;
        var view = _views[shape];
        _views.Remove(shape);
        Children.Remove(view);
        Shapes.Remove(shape);
    }

    private void HandleViewportChange(object? sender, PropertyChangedEventArgs propertyChangedEventArgs)
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
                var p = Viewport.Relative(ShapeTools.GetRectangleAnchor(rectangle, i * 2));
                if (i >= polygonView!.Points.Count)
                    polygonView.Points.Add(p);
                else 
                    polygonView.Points[i] = p;
            }
        }
        else if (shape is ReactiveCircle circle)
        {
            view.Width = circle.Radius * 2 * Viewport.Scale;
            view.Height = circle.Radius * 2 * Viewport.Scale;
            SetLeft(view, (circle.X - circle.Radius) * Viewport.Scale + Viewport.OffsetX);
            SetTop(view, (circle.Y - circle.Radius) * Viewport.Scale + Viewport.OffsetY);
        } 
        else if (shape is ReactivePolygon polygon)
        {
            var polygonView = view as Polygon;
            for (var i = 0; i < polygon.Points.Count; i++)
            {
                var abs = polygon.Points[i];
                var rel = Viewport.Relative(abs.X, abs.Y);
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
    
    private void SetGeometry(Canvas parent)
    {
        parent.Children.Add(this); 
        SetLeft(this, 0);
        SetTop(this, 0); 
        SetZIndex(this, 1);
        Width = parent.Width; Height = parent.Height;
    }
}