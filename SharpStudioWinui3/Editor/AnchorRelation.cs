using System.ComponentModel;
using Windows.Foundation;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace SharpStudioWinui3.Editor;

// Anchor Representation
public sealed class AnchorRelation : INotifyPropertyChanged, IDisposable
{
    private readonly Canvas _parent;
    
    private readonly ViewportMatrix2D _viewport;

    private readonly ReactiveShape _shape;
    
    private readonly List<Point> _anchors = [];
    
    private readonly List<Ellipse> _views = [];
    
    private const int AnchorRadius = 5;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private int _selectedAnchorIndex = -1;
    
    private Point? _cachedAnchor;
    private ReactiveShape? _cachedShape;

    public AnchorRelation(Canvas parent, ReactiveShape shape, ViewportMatrix2D viewport)
    {
        _parent = parent;
        _shape = shape;
        _viewport = viewport;
        Bind();
    }

    public void Dispose()
    {
        Clear();
        Unbind();
    }

    private void Clear()
    {
        foreach (var view in _views)
        {
            _parent.Children.Remove(view);
        }
        _views.Clear();
        _anchors.Clear();
    }

    public void Select(int index)
    {
        if (index < 0 || index >= _anchors.Count) return;
        _selectedAnchorIndex = index;
    }

    public void Save()
    {
        if (_selectedAnchorIndex > -1)
        {
            var anchor = _anchors[_selectedAnchorIndex];
            _cachedAnchor = new Point(anchor.X, anchor.Y);
        }
        _cachedShape = ShapeTools.Clone(_shape);
    }

    public void Restore()
    {
        _cachedAnchor = null;
        _cachedShape = null;
    }

    public void DragResize(Point point)
    {
        if (_cachedShape == null) throw new Exception("Without saving states");
        if (_shape is ReactiveRectangle rectangle)
        {
            if (_selectedAnchorIndex == 8)
                RotateRectangle((_cachedShape as ReactiveRectangle)!, rectangle, point);
            else
                ResizeRectangle((_cachedShape as ReactiveRectangle)!, rectangle, _selectedAnchorIndex, point);
        }
        else if (_shape is ReactiveCircle circle)
        {
            circle.Radius = Vector2D.Subtract(point, new Point(circle.X, circle.Y)).Length;
        }
        else if (_shape is ReactivePolygon polygon)
        {
            polygon.Points[_selectedAnchorIndex] = new Point(point.X, point.Y);
        }
    }

    public int InAnchor(Point point)
    {
        for (var i = _anchors.Count - 1; i >= 0; i--)
        {
            var anchor = _anchors[i];
            var inArea = ShapeTools.InCircle(point, new ReactiveCircle { X = anchor.X, Y = anchor.Y, Radius = AnchorRadius });
            if (inArea) return i;
        }
        return -1;
    }
    
    public void Render()
    {
        for (var i = 0; i < _anchors.Count; i++)
        {
            var point = _viewport.Relative(_anchors[i]);
            if (i < _views.Count)
            {
                var view = _views[i];
                Canvas.SetLeft(view, point.X - AnchorRadius);
                Canvas.SetTop(view, point.Y - AnchorRadius);
                Canvas.SetZIndex(view, 1000);
            }
            else
            {
                var view = new Ellipse 
                { 
                    Width = AnchorRadius * 2, 
                    Height = AnchorRadius * 2, 
                    Fill = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0xff))  // new BrushConverter().ConvertFrom("#0000ff") as Brush,
                };
                _parent.Children.Add(view);
                _views.Add(view);
                Canvas.SetLeft(view, point.X - AnchorRadius);
                Canvas.SetTop(view, point.Y - AnchorRadius);
                Canvas.SetZIndex(view, 1000);
            }
        }
    
        if (_anchors.Count < _views.Count)
        {
            for (var i = 0; i < _views.Count - _anchors.Count; i++)
            {
                var view = _views[^1];
                _parent.Children.Remove(view);
                _views.Remove(view);
            }
        }
    }

    private void Bind()
    {
        _anchors.Clear();
        if (_shape is ReactiveRectangle rectangle)
        {
            for (var i = 0; i < 9; i++)
                _anchors.Add(ShapeTools.GetRectangleAnchor(rectangle, i));
        }
        else if (_shape is ReactiveCircle circle)
        {
            _anchors.Add(new Point(circle.X, circle.Y - circle.Radius));
            _anchors.Add(new Point(circle.X + circle.Radius, circle.Y));
            _anchors.Add(new Point(circle.X, circle.Y + circle.Radius));
            _anchors.Add(new Point(circle.X - circle.Radius, circle.Y));
        }
        else if (_shape is ReactivePolygon polygon)
        {
            _anchors.AddRange(polygon.Points.Select(p => new Point(p.X, p.Y)));
        }

        _shape.PropertyChanged += HandleShapeChange;
    }

    private void HandleShapeChange(object? sender, PropertyChangedEventArgs e)
    {
        if (_shape is ReactiveRectangle rectangle)
        {
            for (var i = 0; i < 9; i++)
                _anchors[i] = ShapeTools.GetRectangleAnchor(rectangle, i);
        }
        else if (_shape is ReactiveCircle circle)
        {
            _anchors[0] = new Point(circle.X, circle.Y - circle.Radius);
            _anchors[1] = new Point(circle.X + circle.Radius, circle.Y);
            _anchors[2] = new Point(circle.X, circle.Y + circle.Radius);
            _anchors[3] = new Point(circle.X - circle.Radius, circle.Y);
        }
        else if (_shape is ReactivePolygon polygon)
        {
            for (var i = 0; i < _anchors.Count; i++)
            {
                _anchors[i] = new Point(polygon.Points[i].X, polygon.Points[i].Y);
            }
        }
        OnPropertyChanged(nameof(_anchors));
    }

    private void Unbind()
    {
        _shape.PropertyChanged -= HandleShapeChange;
    }

    private void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static void ResizeRectangle(ReactiveRectangle start, ReactiveRectangle rectangle, int fixedPointIndex, Point point)
    {
        if (fixedPointIndex is < 0 or > 7) return;
        int[] indices = [4, 5, 6, 7, 0, 1, 2, 3];
        var fixedPoint = ShapeTools.GetRectangleAnchor(rectangle, indices[fixedPointIndex]);
        var diagonalVector = Vector2D.Subtract(point, fixedPoint);
        if (fixedPointIndex % 2 == 0)
        {
            var directionVector = ShapeTools.GetAngleVector(start.Rotation);
            var (projection, perpendicular) = ShapeTools.Decompose(directionVector, diagonalVector);
            rectangle.X = (point.X + fixedPoint.X) * 0.5;
            rectangle.Y = (point.Y + fixedPoint.Y) * 0.5;
            rectangle.Width = perpendicular.Length;
            rectangle.Height = projection.Length;;
        }
        else
        {
            var directionVector = ShapeTools.GetAngleVector(start.Rotation + fixedPointIndex switch{ 1 or 5 => 0, _ => 90 });
            var (projection, _) = ShapeTools.Decompose(directionVector, diagonalVector);
            var dynamicPoint = new Point(fixedPoint.X + projection.X, fixedPoint.Y + projection.Y);
            rectangle.X = (dynamicPoint.X + fixedPoint.X) * 0.5;
            rectangle.Y = (dynamicPoint.Y + fixedPoint.Y) * 0.5;
            if (fixedPointIndex is 1 or 5) rectangle.Height = projection.Length; else rectangle.Width = projection.Length;
        }
    }

    private static void RotateRectangle(ReactiveRectangle start, ReactiveRectangle rectangle, Point point)
    {
        var startAngle = start.Rotation;
        var baseVector = ShapeTools.GetAngleVector(start.Rotation);
        var deltaAngle = ShapeTools.CalculateVectorClockwiseAngle(baseVector, Vector2D.Subtract(point, new Point(start.X, start.Y)));
        rectangle.Rotation = ShapeTools.NormalizeAngle(startAngle + deltaAngle);
    }
}