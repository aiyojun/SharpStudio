using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SharpStudioWpf.Editor;

public class Palette : Canvas
{
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof (Source), typeof (string), typeof (Palette), new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(nameof (Layout), typeof (ImageLayout), typeof (Palette), new FrameworkPropertyMetadata(ImageLayout.None, OnLayoutChanged));
    
    public string Source
    {
        get => (string) GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    public ImageLayout Layout
    {
        get => (ImageLayout) GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }
    
    private readonly ImageLayer _imageLayer;
    private readonly ShapeLayer _shapeLayer;
    private readonly ViewportMatrix2D _viewport = new();
    private readonly CursorState _cursor = new();
    private Tuple<ReactiveShape, ReactiveShape>? _shape;
    private DrawAction _drawAction = DrawAction.None;
    private DrawMode _drawMode = DrawMode.DrawRectangle;
    
    public Palette()
    {
        SetStyle();

        _imageLayer = new ImageLayer(this, _viewport);
        _shapeLayer = new ShapeLayer(this, _viewport);
        
        MouseWheel += OnMouseWheel;
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        KeyDown += OnKeyDown;
        
        Loaded += (sender, args) =>
        {
            _imageLayer.SetupImageLayout(Layout);
        };

    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var palette = (Palette) d;
        try
        {
            palette._imageLayer.Load((string) e.NewValue);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var palette = (Palette) d;
        palette._imageLayer.SetupImageLayout((ImageLayout) e.NewValue);
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        _viewport.ZoomTo(_viewport.Scale * (e.Delta > 0 ? 1.1 : 0.9), e.GetPosition(this));
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        var cursor = e.GetPosition(this);
        var coord = _viewport.Absolute(cursor);
        _cursor.Start = cursor;
        var withCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        if (e.LeftButton == MouseButtonState.Pressed)
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
        else if (e.RightButton == MouseButtonState.Pressed)
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
        else if (e.MiddleButton == MouseButtonState.Pressed)
        {
            _cursor.Buttons = 4; 
            _viewport.Save(cursor);
            _drawAction = DrawAction.DragPalette;
        }
        CaptureMouse();
    }
    
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var cursor = e.GetPosition(this);
        var coord = _viewport.Absolute(cursor);
        if (_drawAction == DrawAction.DragPalette)
        {
            _viewport.MoveTo(cursor);
        }
        else if (_drawAction == DrawAction.DragAnchor)
        {
            _shapeLayer.AnchorRelation!.DragResize(coord);
        }
        else if (_drawAction == DrawAction.DragShape)
        {
            ShapeTools.MoveShape(_shape!.Item2, _shape.Item1, coord - _viewport.Absolute((Point)_cursor.Start!));
        }
        else if (_drawAction == DrawAction.DrawShape && _drawMode == DrawMode.DrawRectangle)
        {
            var rectangle = _shape!.Item1 as ReactiveRectangle;
            var copy = _shape.Item2 as ReactiveRectangle;
            var start = new Point(copy!.X - copy.Width * 0.5, copy.Y - copy.Height * 0.5);
            var end = _viewport.Absolute(cursor);
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
            var end = _viewport.Absolute(cursor);
            circle!.Radius = (end - start).Length;
        }
        else if (_drawMode == DrawMode.DrawPolygon && _shape != null)
        {
            var polygon = _shape!.Item1 as ReactivePolygon;
            polygon!.UpdateBack(new Point(coord.X, coord.Y));
        }
        _shapeLayer.Render();
    }
    
    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var cursor = e.GetPosition(this);
        if (_cursor.Start == null) return;
        var shake = (cursor - (Point)_cursor.Start!).Length; 
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
        ReleaseMouseCapture();
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
    }

    public void Test()
    {
        _shapeLayer.AddShape(new ReactivePolygon { Points = [new Point(10, 10), new Point(80, 80), new Point(10, 80)], Label = ""});
        _shapeLayer.AddShape(new ReactiveRectangle { X = 100, Y = 100, Width = 100, Height = 80, Label = "" });
        // Task.Run(async () =>
        // {
        //     await Task.Delay(2000);
        //     Application.Current.Dispatcher.Invoke(() =>
        //     {
        //         RemoveShapeAt(0);
        //     });
        // });
    }

    private void SetStyle()
    {
        // // Background = Brushes.LightGray;
        // var gradientBrush = new LinearGradientBrush
        // {
        //     StartPoint = new Point(0.5, 0),
        //     EndPoint = new Point(0.5, 1)
        // };
        // gradientBrush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
        // gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(241, 232, 248), 1.0));
        // // gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0x32, 0x32, 0x37), 1.0));
        // // gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0x1e, 0x1e, 0x23), 1.0));
        // // 323237 1E1E23
        Background = Brushes.Transparent;
        ClipToBounds = true;
        Focusable = true;
        FocusVisualStyle = null;
    }
}