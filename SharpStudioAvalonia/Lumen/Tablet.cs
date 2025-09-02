using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Mathematics.d2;
using SharpStudioAvalonia.Views;
using Point = Avalonia.Point;

namespace SharpStudioAvalonia.Lumen;

public class Tablet : Control
{
    public static readonly StyledProperty<int> TileProperty = AvaloniaProperty.Register<Tablet, int>(nameof(Tile), 15);

    public static readonly StyledProperty<IList<ReactiveShape>> ShapesProperty = AvaloniaProperty.Register<Tablet, IList<ReactiveShape>>(nameof(Shapes), new AvaloniaList<ReactiveShape>());
    
    public static readonly StyledProperty<IImage?> SourceProperty = AvaloniaProperty.Register<Tablet, IImage?>(nameof(Source));
    
    public static readonly StyledProperty<ImageLayout> ImageLayoutProperty = AvaloniaProperty.Register<Tablet, ImageLayout>(nameof(ImageLayout), ImageLayout.Cover);
    
    public int Tile
    {
        get => GetValue(TileProperty);
        set => SetValue(TileProperty, value);
    }
    
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    public ImageLayout ImageLayout
    {
        get => GetValue(ImageLayoutProperty);
        set => SetValue(ImageLayoutProperty, value);
    }
    
    public IList<ReactiveShape> Shapes
    {
        get => GetValue(ShapesProperty);
        set => SetValue(ShapesProperty, value);
    }

    private double _cursorStartX;
    private double _cursorStartY;
    private const double AnchorRadius = 5;
    private readonly Camera _camera = new();
    private AnchorGroup? _anchorGroup;
    private Tuple<ReactiveShape, ReactiveShape>? _shape;
    private DrawAction _drawAction = DrawAction.None;
    private DrawMode _drawMode = DrawMode.DrawRectangle;

    static Tablet()
    {
        AffectsRender<Tablet>(SourceProperty, TileProperty, ShapesProperty, ImageLayoutProperty);
    }

    public Tablet()
    {
        Loaded += (_, _) =>
        {
            if (window != null)
            {
                window!.KeyDown += OnKeyDown;
            }
        };
    }

    private Window? window => this.GetVisualRoot() as Window;

    public Matrix Transform => Matrix.CreateScale(_camera.Scale, _camera.Scale) * Matrix.CreateTranslation(_camera.X, _camera.Y);

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(
            Math.Clamp(double.IsNaN(Width) ? availableSize.Width : Width, MinWidth, MaxWidth),
            Math.Clamp(double.IsNaN(Height) ? availableSize.Height : Height, MinHeight, MaxHeight)
        );
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }

    private RenderTargetBitmap? _chessboard;

    private void RenderChessboard(PixelSize size)
    {
        if (_chessboard != null && _chessboard.PixelSize.Equals(size)) return;
        _chessboard?.Dispose();
        _chessboard = new RenderTargetBitmap(size);
        using var context = _chessboard.CreateDrawingContext(true);
        for (var y = 0; y < size.Height; y += Tile)
        {
            for (var x = 0; x < size.Width; x += Tile)
            {
                var color = (x / Tile + y / Tile) % 2 == 0
                    ? Color.FromRgb(0x33, 0x33, 0x33)
                    : Color.FromRgb(0x66, 0x66, 0x66);
                context.FillRectangle(new SolidColorBrush(color), new Rect(x, y, Tile, Tile));
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        int chessboardWidth = (int)Bounds.Width + 1, chessboardHeight = (int)Bounds.Height + 1;
        RenderChessboard(new PixelSize(chessboardWidth, chessboardHeight));
        context.DrawImage(_chessboard!, new Rect(0, 0, chessboardWidth, chessboardHeight));

        var matrix = Transform;
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
        if (Source is Bitmap bitmap)
        {
            var point = matrix.Transform(new Point(0, 0));
            using (context.PushRenderOptions(new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.None }))
            {
                context.DrawImage(Source, new Rect(point.X, point.Y, bitmap.PixelSize.Width * _camera.Scale, bitmap.PixelSize.Height * _camera.Scale));
            }
        }

        const string defaultColor = "#eeeeee";
        foreach (var shape in Shapes)
        {
            var pen = new Pen(ConvertColor(shape.Color ?? defaultColor));
            if (shape is ReactiveCircle circle)
            {
                var center = matrix.Transform(new Point(circle.X, circle.Y));
                var radius = _camera.Scale * circle.Radius;
                context.DrawEllipse(ConvertColor(shape.Color ?? defaultColor, 0x22), pen, center, radius, radius);
            }
            else if (shape is ReactiveRectangle rectangle)
            {
                var points = Enumerable.Range(0, 4).ToArray()
                    .Select(e => matrix.Transform(ShapeTools.GetRectangleAnchor(rectangle, e * 2).ToAvaloniaPoint()))
                    .ToList();
                var polyline = new PolylineGeometry { Points = points, IsFilled = true };
                context.DrawGeometry(ConvertColor(shape.Color ?? defaultColor, 0x22), pen, polyline);
            }
            else if (shape is ReactivePolygon polygon)
            {
                var points = polygon.Points.Select(e => matrix.Transform(e.ToAvaloniaPoint())).ToList();
                var polyline = new PolylineGeometry { Points = points, IsFilled = true };
                context.DrawGeometry(ConvertColor(shape.Color ?? defaultColor, 0x22), pen, polyline);
            }
        }

        if (_anchorGroup == null) return;
        var anchors = _anchorGroup.Anchors;
        foreach (var anchor in anchors)
        {
            context.DrawEllipse(new SolidColorBrush(Colors.DodgerBlue), null, matrix.Transform(new Point(anchor.X, anchor.Y)), AnchorRadius, AnchorRadius);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position;
        var cursorProps = cursorPoint.Properties;
        var withCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        (_cursorStartX, _cursorStartY) = (cursor.X, cursor.Y);
        var matrix = Transform;
        var inv = matrix.Invert();
        var coord = inv.Transform(new Point(_cursorStartX, _cursorStartY)).ToD2Point();
        if (cursorProps.IsLeftButtonPressed)
        {
            var selectedShapeIndex = InShape(coord);
            var selectedAnchorIndex = InAnchor(coord);
            if (withCtrl)
            {
                var index = InShape(coord);
                if (index > -1 && index < Shapes.Count)
                    _anchorGroup = new AnchorGroup(index, Shapes[index]);
                else
                    _anchorGroup = null;
            }
            else if (_anchorGroup != null && (selectedAnchorIndex != -1 || selectedShapeIndex != -1))
            {
                if (selectedAnchorIndex != -1)
                {
                    _anchorGroup!.Save(selectedAnchorIndex);
                    _drawAction = DrawAction.DragAnchor;
                }
                else if (selectedShapeIndex == _anchorGroup!.Index)
                {
                    _anchorGroup!.Save();
                    _drawAction = DrawAction.DragShape;
                }
            }
            else
            {
                _anchorGroup = null;
                if (_drawMode == DrawMode.DrawRectangle)
                {
                    var shape = new ReactiveRectangle
                        { X = coord.X, Y = coord.Y, Width = 0, Height = 0, Label = "", Color = "5555ff" };
                    Shapes.Add(shape);
                    _shape = new Tuple<ReactiveShape, ReactiveShape>(shape,
                        new ReactiveRectangle { X = shape.X, Y = shape.Y, Width = shape.Width, Height = shape.Height });
                    _drawAction = DrawAction.DrawShape;
                }
                else if (_drawMode == DrawMode.DrawCircle)
                {
                    var shape = new ReactiveCircle
                        { X = coord.X, Y = coord.Y, Radius = 0, Label = "", Color = "5555ff" };
                    Shapes.Add(shape);
                    _shape = new Tuple<ReactiveShape, ReactiveShape>(shape,
                        new ReactiveCircle { X = shape.X, Y = shape.Y, Radius = shape.Radius });
                    _drawAction = DrawAction.DrawShape;
                }
                else if (_drawMode == DrawMode.DrawPolygon)
                {
                    if (_shape == null)
                    {
                        var polygon = new ReactivePolygon
                        {
                            Points = [coord, new Mathematics.d2.Point(coord.X, coord.Y)], Label = "", Color = "5555ff"
                        };
                        Shapes.Add(polygon);
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
            if (_drawMode == DrawMode.DrawPolygon)
            {
                if (_shape != null)
                {
                    var polygon = _shape!.Item1 as ReactivePolygon;
                    if (polygon!.Points.Count - 1 < 3)
                        Shapes.Remove(polygon);
                    else
                        polygon.PopBack();
                }

                _shape = null;
            }
        }
        else if (cursorProps.IsMiddleButtonPressed)
        {
            _camera.Save(cursor.ToD2Point());
            _drawAction = DrawAction.DragPalette;
        }

        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position;
        var matrix = Transform;
        var inv = matrix.Invert();
        var coord = inv.Transform(cursor).ToD2Point();
        if (_drawAction == DrawAction.DragPalette)
        {
            _camera.MoveTo(cursor.ToD2Point());
        }
        else if (_drawAction == DrawAction.DragAnchor)
        {
            _anchorGroup!.DragResize(coord);
        }
        else if (_drawAction == DrawAction.DragShape)
        {
            ShapeTools.MoveShape(_anchorGroup!.CachedShape!, _anchorGroup.Shape, coord - inv.Transform(new Point(_cursorStartX, _cursorStartY)).ToD2Point());
        }
        else if (_drawAction == DrawAction.DrawShape && _drawMode == DrawMode.DrawRectangle)
        {
            var rectangle = _shape!.Item1 as ReactiveRectangle;
            var copy = _shape.Item2 as ReactiveRectangle;
            var start = new Point(copy!.X - copy.Width * 0.5, copy.Y - copy.Height * 0.5);
            var end = inv.Transform(cursor);
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
            var start = new Mathematics.d2.Point(copy!.X, copy.Y);
            var end = inv.Transform(cursor).ToD2Point();
            circle!.Radius = (end - start).Length;
        }
        else if (_drawMode == DrawMode.DrawPolygon && _shape != null)
        {
            var polygon = _shape!.Item1 as ReactivePolygon;
            polygon!.UpdateBack(new Mathematics.d2.Point(coord.X, coord.Y));
        }
        else
        {
            return;
        }
        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var cursorPoint = e.GetCurrentPoint(this);
        var cursor = cursorPoint.Position.ToD2Point();
        var shake = (cursor - new Mathematics.d2.Point(_cursorStartX, _cursorStartY)).Length;
        if (shake <= 5)
        {
            if (_drawAction == DrawAction.DrawShape && _drawMode is DrawMode.DrawRectangle or DrawMode.DrawCircle)
            {
                Shapes.Remove(_shape!.Item1);
            }
        }

        if (_drawAction == DrawAction.DrawShape)
        {
            _shape = null;
        }

        _drawAction = DrawAction.None;
        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        _camera.ZoomTo(_camera.Scale * (e.Delta.Y > 0 ? 1.1 : 0.9), e.GetCurrentPoint(this).Position.ToD2Point());
        InvalidateVisual();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _drawMode = e.Key switch
        {
            Key.D1 => DrawMode.DrawRectangle,
            Key.D2 => DrawMode.DrawCircle,
            Key.D3 => DrawMode.DrawPolygon,
            _ => _drawMode
        };
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetupImageLayout(ImageLayout);
    }

    private int InShape(Mathematics.d2.Point point)
    {
        for (var i = Shapes.Count - 1; i >= 0; i--)
        {
            var shape = Shapes[i];
            var inArea = shape switch
            {
                ReactiveRectangle rectangle => ShapeTools.InRectangle(point, rectangle),
                ReactiveCircle circle => ShapeTools.InCircle(point, circle),
                ReactivePolygon polygon => ShapeTools.InPolygon(point, polygon),
                _ => false,
            };
            if (inArea) return i;
        }

        return -1;
    }

    public int InAnchor(Mathematics.d2.Point point)
    {
        if (_anchorGroup == null) return -1;
        var anchors = _anchorGroup.Anchors;
        for (var i = anchors.Count - 1; i >= 0; i--)
        {
            var anchor = anchors[i];
            var inArea = ShapeTools.InCircle(point, new ReactiveCircle { X = anchor.X, Y = anchor.Y, Radius = AnchorRadius / _camera.Scale });
            if (inArea) return i;
        }

        return -1;
    }

    public void SetupImageLayout(ImageLayout layout)
    {
        if (Source is not Bitmap bitmap) return;
        double width = Bounds.Width, height = width * bitmap.PixelSize.Height / bitmap.PixelSize.Width;
        if (layout == ImageLayout.Cover)
        {
            if (height < Bounds.Height)
            {
                height = Bounds.Height;
                width = height * bitmap.PixelSize.Width / bitmap.PixelSize.Height;
            }
        }
        else if (layout == ImageLayout.Contain)
        {
            if (height > Bounds.Height)
            {
                height = Bounds.Height;
                width = height * bitmap.PixelSize.Width / bitmap.PixelSize.Height;
            }
        }
        double x = (Bounds.Width - width) * 0.5, y = (Bounds.Height - height) * 0.5;
        _camera.X = x;
        _camera.Y = y;
        _camera.Scale = width / bitmap.PixelSize.Width;
        InvalidateVisual();
    }

    private static IBrush ConvertColor(string color, byte alpha = 0xff)
    {
        color = color.Replace("#", "");
        if (color.Length != 6) color = "eeeeee";
        return new SolidColorBrush(Color.FromArgb(alpha, Convert.ToByte(color[..2], 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16)));
    }
}