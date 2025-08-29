using System;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Mathematics.d2;

namespace SharpStudioAvalonia.Quartz;

public class Edge : Path
{
    public readonly bool IsActive;
    private double[] _cursor = [0, 0];
    
    private Edge(Port source, Port target)
    {
        StrokeThickness = 1;
        IsActive = false;
        Source = source;
        Target = target;
        Bind();
        Loaded += (_, _) => UpdatePathData();
    }

    private Edge(Port source, Point point)
    {
        StrokeThickness = 1;
        IsActive = true;
        Source = source;
        Target = null;
        Bind();
        Loaded += (_, _) => UpdatePathData();
        _cursor[0] = point.X;
        _cursor[1] = point.Y;
    }

    public static Edge CreateEdge(Port source, Port target, IBrush color)
    {
        return new Edge(source, target) { Stroke = color };
    }

    public static Edge CreateActiveEdge(Port source, Point point)
    {
        return new Edge(source, point) { Stroke = new SolidColorBrush(Colors.DodgerBlue) };
    }

    public void UpdateEndpoint(Point point)
    {
        _cursor[0] = point.X;
        _cursor[1] = point.Y;
        if (IsActive)
            UpdatePathData();
    }

    ~Edge()
    {
        Unbind();
    }

    private void UpdatePathData()
    {
        Point p0 = Source.GetPosition(), p1 = !IsActive ? Target!.GetPosition() : new Point(_cursor[0], _cursor[1]);
        double x1 = Source.Component.X + Source.Component.Bounds.Width, y1 = Source.Component.Y + p0.Y - 2.75;
        double x2 = !IsActive ? Target!.Component.X : p1.X, y2 = !IsActive ? Target!.Component.Y + p1.Y - 2.75 : p1.Y;
        var delta = x2 - x1 > 0 ? (x2 - x1) * .5 : Math.Abs((x2 - x1) * .5);
        // double delta = x2 - x1 >= 200 ? (x2 - x1) * .5 : 100;
        // double delta = (x2 - x1) * .5;
        Data = Geometry.Parse($"M{x1} {y1} C{x1+delta} {y1} {x2-delta} {y2} {x2} {y2}"); 
    }

    private void OnNodeMoved(object? sender, Point point)
    {
        UpdatePathData();
    }

    private void Bind()
    {
        Source.Component.Moved += OnNodeMoved;
        if (!IsActive)
            Target!.Component.Moved += OnNodeMoved;
    }
    
    private void Unbind()
    {
        Source.Component.Moved -= OnNodeMoved;
        if (!IsActive)
            Target!.Component.Moved -= OnNodeMoved;
    }
    
    public Port Source { get; }
    public Port? Target { get; }
}