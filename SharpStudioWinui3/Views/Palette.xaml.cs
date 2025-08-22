using System.Numerics;
using Windows.Foundation;
using Windows.System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SharpStudioWinui3.Editor;
using Newtonsoft.Json;

namespace SharpStudioWinui3.Views;

public sealed partial class Palette
{
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(Palette), new PropertyMetadata(string.Empty, OnSourceChanged));

    public string Source
    {
        get => (string)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(nameof(Layout), typeof(ImageLayout), typeof(Palette), new PropertyMetadata(ImageLayout.None, OnLayoutChanged));
    public ImageLayout Layout
    {
        get => (ImageLayout) GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public Palette()
    {
        InitializeComponent();
        _imageLayer = new ImageLayer(BackgroundLayer, Container, BackgroundImage, _viewport);
        _shapeLayer = new ShapeLayer(ShapeViewsLayer, Container, _viewport);
        Background  = new SolidColorBrush(Colors.Transparent);
        SizeChanged += OnLoaded;
        
        // FocusManager.GotFocus += (sender, e) =>
        // {
        //     Console.WriteLine($"焦点到了: {e.NewFocusedElement}");
        //     if (e.NewFocusedElement is ScrollViewer scrollViewer)
        //     {
        //         Console.WriteLine($"  ScrollViewer {scrollViewer.Name} {scrollViewer.IsTabStop}");
        //     }
        // };
        //
        // FocusManager.LostFocus += (sender, e) =>
        // {
        //     Console.WriteLine($"失去焦点: {e.OldFocusedElement}");
        // };
    }
    
    private readonly ImageLayer _imageLayer;
    private readonly ShapeLayer _shapeLayer;
    private readonly ViewportMatrix2D _viewport = new();
    private readonly CursorState _cursor = new();
    private Tuple<ReactiveShape, ReactiveShape>? _shape;
    private ActionType _actionType = ActionType.None;
    private DrawMode _drawMode = DrawMode.DrawRectangle;
    
    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    
    private void OnMouseWheel(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        _viewport.ZoomTo(_viewport.Scale * (point.Properties.MouseWheelDelta > 0 ? 1.1 : 0.9), point.Position);
    }
    
    private void OnMouseDown(object sender, PointerRoutedEventArgs e)
    {
        // Console.WriteLine($"Visual Tree : {ApplicationHelper.Window.Content}");
        // ApplicationHelper.PrintVisualTree(ApplicationHelper.Window.Content as DependencyObject);
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position;
        var cursorProps = cursorPoint.Properties;
        var coord = _viewport.Absolute(cursor);
        _cursor.Start = cursor;
        var withCtrl = e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control); //(Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        if (cursorProps.IsLeftButtonPressed)  // if (e.LeftButton == MouseButtonState.Pressed)
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
                    _actionType = ActionType.DragAnchor;
                    _shapeLayer.AnchorRelation!.Select(selectedAnchorIndex);
                    _shapeLayer.AnchorRelation!.Save();
                }
                else
                {
                    var shape = _shapeLayer.Shapes[selectedShapeIndex];
                    _shape = new Tuple<ReactiveShape, ReactiveShape>(shape, ShapeTools.Clone(shape));
                    _actionType = ActionType.DragShape;
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
                    _actionType = ActionType.DrawShape;
                }
                else if (_drawMode == DrawMode.DrawCircle)
                {
                    var shape = new ReactiveCircle { X = coord.X, Y = coord.Y, Radius = 0, Label = "", Color = "5555ff" };
                    _shapeLayer.AddShape(shape);
                    _shape = new Tuple<ReactiveShape, ReactiveShape>(shape, new ReactiveCircle{X = shape.X, Y = shape.Y, Radius = shape.Radius});
                    _actionType = ActionType.DrawShape;
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
        else if (cursorProps.IsRightButtonPressed)  // else if (e.RightButton == MouseButtonState.Pressed)
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
        else if (cursorProps.IsMiddleButtonPressed)  // else if (e.MiddleButton == MouseButtonState.Pressed)
        {
            _cursor.Buttons = 4; 
            _viewport.Save(cursor);
            _actionType = ActionType.DragPalette;
        }
        // CapturePointer(e.Pointer); // CaptureMouse();
    }
    
    private void OnMouseMove(object sender, PointerRoutedEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position;
        var coord = _viewport.Absolute(cursor);
        if (_actionType == ActionType.DragPalette)
        {
            _viewport.MoveTo(cursor);
        }
        else if (_actionType == ActionType.DragAnchor)
        {
            _shapeLayer.AnchorRelation!.DragResize(coord);
        }
        else if (_actionType == ActionType.DragShape)
        {
            ShapeTools.MoveShape(_shape!.Item2, _shape.Item1, Vector2D.Subtract(coord, _viewport.Absolute((Point)_cursor.Start!)));
        }
        else if (_actionType == ActionType.DrawShape && _drawMode == DrawMode.DrawRectangle)
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
        else if (_actionType == ActionType.DrawShape && _drawMode == DrawMode.DrawCircle)
        {
            var circle = _shape!.Item1 as ReactiveCircle;
            var copy = _shape.Item2 as ReactiveCircle;
            var start = new Point(copy!.X, copy.Y);
            var end = _viewport.Absolute(cursor);
            circle!.Radius = Vector2D.Subtract(end, start).Length;
        }
        else if (_drawMode == DrawMode.DrawPolygon && _shape != null)
        {
            var polygon = _shape!.Item1 as ReactivePolygon;
            polygon!.UpdateBack(new Point(coord.X, coord.Y));
        }
        _shapeLayer.Render();
    }
    
    private void OnMouseUp(object sender, PointerRoutedEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position;
        if (_cursor.Start == null) return;
        var shake = Vector2D.Subtract(cursor, (Point)_cursor.Start!).Length; 
        if (shake <= 5)
        {
            if (_actionType == ActionType.DrawShape && _drawMode is DrawMode.DrawRectangle or DrawMode.DrawCircle)
            {
                _shapeLayer.RemoveShape(_shape!.Item1);
            }
        }
        
        _shapeLayer.AnchorRelation?.Restore();
        if (_actionType == ActionType.DrawShape)
        {
            _shape = null;
        }
        _actionType = ActionType.None;
        e.Handled = true;
        Container.Focus(FocusState.Programmatic);
        Console.WriteLine($"{Container.FocusState}");
    }
    
    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        _drawMode = e.Key switch
        {
            VirtualKey.Number1 => DrawMode.DrawRectangle,
            VirtualKey.Number2 => DrawMode.DrawCircle,
            VirtualKey.Number3 => DrawMode.DrawPolygon,
            _ => _drawMode
        };
        // Console.WriteLine($"KeyDown : {e.Key} Focus state {Container.FocusState}");
        if (e.Key == VirtualKey.D)
        {
            Console.WriteLine(JsonConvert.SerializeObject(ApplicationHelper.DumpToJson(ApplicationHelper.Window.Content), Formatting.Indented));
        }
        if (e.Key == VirtualKey.F)
        {
            Console.WriteLine(JsonConvert.SerializeObject(ApplicationHelper.DumpToJson(ApplicationHelper.GetCursorUIElement()), Formatting.Indented));
        }

        if (e.Key == VirtualKey.G)
        {
            var win = new DevTool();
            // win.Load();
            win.Activate();
        }
    }
    
    private const int TileSize = 20;
    
    // private void OnChessboardLayerLoaded(object sender, RoutedEventArgs e)
    // {
    //     ChessboardLayer.Children.Clear();
    //     var iWidth = ActualWidth;
    //     var iHeight = ActualHeight;
    //     for (var y = 0; y < iHeight; y += TileSize)
    //     {
    //         for (var x = 0; x < iWidth; x += TileSize)
    //         {
    //             var color = ((x / TileSize + y / TileSize) % 2 == 0) ? Colors.LightGray : Colors.WhiteSmoke;
    //             var rect = new Rectangle
    //             {
    //                 Width = TileSize,
    //                 Height = TileSize,
    //                 Fill = new SolidColorBrush(color)
    //             };
    //             Canvas.SetLeft(rect, x);
    //             Canvas.SetTop(rect, y);
    //             ChessboardLayer.Children.Add(rect);
    //         }
    //     }
    // }
    //
    //
    // private void OnDrawChessboard(CanvasControl sender, CanvasDrawEventArgs args)
    // {
    //     
    // }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Container.Clip = new RectangleGeometry { Rect = new Rect(0, 0, Container.ActualWidth, Container.ActualHeight) };
        _imageLayer.SetupGeometry();
        // Console.WriteLine($"{Container.FocusState}");
        // var scroll = FindScrollViewer(Container);
        // Console.WriteLine(" > " + scroll);
        // Console.WriteLine($"Visual Tree : {ApplicationHelper.Window.Content}");
        // ApplicationHelper.PrintVisualTree(ApplicationHelper.Window.Content as DependencyObject);
    }

    public void OnBackgroundImageOpened(object sender, RoutedEventArgs e)
    {
        _imageLayer.SetupImageLayout(ImageLayout.Contain);
    }
    
    private Microsoft.UI.Composition.ContainerVisual? _root;
    
    private Microsoft.UI.Composition.Compositor? _compositor;
    
    private void OnChessboardLayerLoaded(object sender, RoutedEventArgs e)
    {
        var hostVisual = ElementCompositionPreview.GetElementVisual(ChessboardLayer);
        _compositor ??= hostVisual.Compositor;
        _root ??= _compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(ChessboardLayer, _root);
        var iWidth = ActualWidth;
        var iHeight = ActualHeight;
        for (var y = 0; y < iHeight; y += TileSize)
        {
            for (var x = 0; x < iWidth; x += TileSize)
            {
                var color = ((x / TileSize + y / TileSize) % 2 == 0) ? Colors.LightGray : Colors.WhiteSmoke;
                var rect = _compositor.CreateSpriteVisual();
                rect.Brush = _compositor.CreateColorBrush(color);
                rect.Size = new Vector2(TileSize, TileSize);
                rect.Offset = new Vector3(x, y, 0);
                _root.Children.InsertAtTop(rect);
            }
        }
        ApplicationHelper.PrintVisualTree(ApplicationHelper.Window!.Content);
    }
}