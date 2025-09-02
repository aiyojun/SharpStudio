using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Mathematics.d2;
using SharpStudioAvalonia.Editor;
using Point = Mathematics.d2.Point;

namespace SharpStudioAvalonia.Views;

public partial class Palette : UserControl
{
    public static readonly StyledProperty<IImage?> SourceProperty = AvaloniaProperty.Register<Palette, IImage?>(nameof (Source));
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private readonly ImageLayer _imageLayer;
    private readonly ShapeLayer _shapeLayer;
    private readonly Camera _camera = new();
    private readonly CursorState _cursor = new();
    private Tuple<ReactiveShape, ReactiveShape>? _shape;
    private DrawAction _drawAction = DrawAction.None;
    private DrawMode _drawMode = DrawMode.DrawRectangle;
    
    public Palette()
    {
        InitializeComponent();
        _imageLayer = new ImageLayer(ImageLayer, Container, TargetImage, _camera);
        _shapeLayer = new ShapeLayer(ShapeLayer, Container, _camera);

        Loaded += (_, _) =>
        {
            if (window != null)
            {
                window!.KeyDown += OnKeyDown;
            }
        };
    }

    private Window? window => this.GetVisualRoot() as Window;
    
    private void OnMouseWheel(object sender, PointerWheelEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        _camera.ZoomTo(_camera.Scale * (e.Delta.Y > 0 ? 1.1 : 0.9), point.Position.ToD2Point());
    }
    
    private void OnMouseDown(object? sender, PointerPressedEventArgs e)
    {
        Container.Focus();
        // Console.WriteLine($"Visual Tree : {ApplicationHelper.Window.Content}");
        // ApplicationHelper.PrintVisualTree(ApplicationHelper.Window.Content as DependencyObject);
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position.ToD2Point();
        var cursorProps = cursorPoint.Properties;
        var coord = _camera.ConvertToWorld(cursor);
        _cursor.Save(cursor);
        var withCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        if (cursorProps.IsLeftButtonPressed)
        {
            var selectedShapeIndex = _shapeLayer.InShape(coord);
            var selectedAnchorIndex = _shapeLayer.InAnchor(coord);
            _cursor.Buttons = 1;
            if (withCtrl)
            {
                var index = _shapeLayer.InShape(coord);
                if (index > -1)
                    _shapeLayer.Select(index);
                else
                    _shapeLayer.Deselect();
            }
            else if (_shapeLayer.IsSelected() && (selectedAnchorIndex != -1 || selectedShapeIndex != -1))
            {
                if (selectedAnchorIndex != -1)
                {
                    _drawAction = DrawAction.DragAnchor;
                    _shapeLayer.AnchorRelation!.Select(selectedAnchorIndex);
                    _shapeLayer.AnchorRelation!.Save();
                }
                else
                {
                    var shape = _shapeLayer.Shapes[selectedShapeIndex];
                    _shape = new Tuple<ReactiveShape, ReactiveShape>(shape, ShapeTools.Clone(shape));
                    _drawAction = DrawAction.DragShape;
                }
            }
            else
            {
                _shapeLayer.Deselect();
                if (_drawMode == DrawMode.DrawRectangle)
                {
                    var shape = new ReactiveRectangle { X = coord.X, Y = coord.Y, Width = 0, Height = 0, Label = "", Color = "5555ff" };
                    _shapeLayer.AddShape(shape);
                    _shape = new Tuple<ReactiveShape, ReactiveShape>(shape, new ReactiveRectangle{X = shape.X, Y = shape.Y, Width = shape.Width, Height = shape.Height});
                    _drawAction = DrawAction.DrawShape;
                }
                else if (_drawMode == DrawMode.DrawCircle)
                {
                    var shape = new ReactiveCircle { X = coord.X, Y = coord.Y, Radius = 0, Label = "", Color = "5555ff" };
                    _shapeLayer.AddShape(shape);
                    _shape = new Tuple<ReactiveShape, ReactiveShape>(shape, new ReactiveCircle{X = shape.X, Y = shape.Y, Radius = shape.Radius});
                    _drawAction = DrawAction.DrawShape;
                }
                else if (_drawMode == DrawMode.DrawPolygon)
                {
                    if (_shape == null)
                    {
                        var polygon = new ReactivePolygon { Points = [coord, new Point(coord.X, coord.Y)], Label = "", Color = "5555ff"};
                        _shapeLayer.AddShape(polygon);
                        _shape = new Tuple<ReactiveShape, ReactiveShape>(polygon, polygon);
                    }
                    else
                    {
                        var polygon = _shape!.Item1 as ReactivePolygon;
                        polygon!.Points.Add(coord);
                    }
                }
            }
            
        }
        else if (cursorProps.IsRightButtonPressed)
        {
            _cursor.Buttons = 2; 
            if (_drawMode == DrawMode.DrawPolygon)
            {
                if (_shape != null)
                {
                    var polygon = _shape!.Item1 as ReactivePolygon;
                    if (polygon!.Points.Count + 1 < 3)
                        _shapeLayer.RemoveShape(polygon);
                    else
                        polygon.PopBack();
                }
                _shape = null;
            }
        }
        else if (cursorProps.IsMiddleButtonPressed)
        {
            _cursor.Buttons = 4; 
            _camera.Save(cursor);
            _drawAction = DrawAction.DragPalette;
        }
    }
    
    private void OnMouseMove(object? sender, PointerEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position.ToD2Point();
        var coord = _camera.ConvertToWorld(cursor);
        if (_drawAction == DrawAction.DragPalette)
        {
            _camera.MoveTo(cursor);
        }
        else if (_drawAction == DrawAction.DragAnchor)
        {
            _shapeLayer.AnchorRelation!.DragResize(coord);
        }
        else if (_drawAction == DrawAction.DragShape)
        {
            ShapeTools.MoveShape(_shape!.Item2, _shape.Item1, coord - _camera.ConvertToWorld(_cursor.Start));
        }
        else if (_drawAction == DrawAction.DrawShape && _drawMode == DrawMode.DrawRectangle)
        {
            var rectangle = _shape!.Item1 as ReactiveRectangle;
            var copy = _shape.Item2 as ReactiveRectangle;
            var start = new Point(copy!.X - copy.Width * 0.5, copy.Y - copy.Height * 0.5);
            var end = _camera.ConvertToWorld(cursor);
            var width = Math.Abs(start.X - end.X);
            var height = Math.Abs(start.Y - end.Y);
            rectangle!.X = Math.Min(start.X, end.X) + width * .5;
            rectangle.Y = Math.Min(start.Y, end.Y) + height * .5;
            rectangle.Width = width;
            rectangle.Height = height;
        }
        else if (_drawAction == DrawAction.DrawShape && _drawMode == DrawMode.DrawCircle)
        {
            var circle = _shape!.Item1 as ReactiveCircle;
            var copy = _shape.Item2 as ReactiveCircle;
            var start = new Point(copy!.X, copy.Y);
            var end = _camera.ConvertToWorld(cursor);
            circle!.Radius = (end - start).Length;
        }
        else if (_drawMode == DrawMode.DrawPolygon && _shape != null)
        {
            var polygon = _shape!.Item1 as ReactivePolygon;
            polygon!.UpdateBack(new Point(coord.X, coord.Y));
        }
        _shapeLayer.Render();
    }
    
    private void OnMouseUp(object? sender, PointerReleasedEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position.ToD2Point();
        // if (_cursor.Start == null) return;
        var shake = (cursor - _cursor.Start).Length; 
        if (shake <= 5)
        {
            if (_drawAction == DrawAction.DrawShape && _drawMode is DrawMode.DrawRectangle or DrawMode.DrawCircle)
            {
                _shapeLayer.RemoveShape(_shape!.Item1);
            }
        }
        
        _shapeLayer.AnchorRelation?.Restore();
        if (_drawAction == DrawAction.DrawShape)
        {
            _shape = null;
        }
        _drawAction = DrawAction.None;
        e.Handled = true;
    }
    
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        _drawMode = e.Key switch
        {
            Key.D1 => DrawMode.DrawRectangle,
            Key.D2 => DrawMode.DrawCircle,
            Key.D3 => DrawMode.DrawPolygon,
            _ => _drawMode
        };
        if (e.Key == Key.D)
        {
            // Console.WriteLine(JsonConvert.SerializeObject(ApplicationHelper.DumpToJson(ApplicationHelper.Window.Content), Formatting.Indented));
        }
        if (e.Key == Key.F)
        {
            // Console.WriteLine(JsonConvert.SerializeObject(ApplicationHelper.DumpToJson(ApplicationHelper.GetCursorUIElement()), Formatting.Indented));
        }

        if (e.Key == Key.G)
        {
            // var win = new DevTool();
            // win.Load();
            // win.Activate();
        }
    }
    
    private void TargetImage_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _imageLayer.SetupGeometry();
        _shapeLayer.SetupGeometry();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine($"TargetImage_OnLoaded {Source}");
        if (Source != null)
        {
            TargetImage.Source = Source;
            _imageLayer.SetupImageLayout(ImageLayout.Cover);
        }
    }
}