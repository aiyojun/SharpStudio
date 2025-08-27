using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Mathematics.d2;

namespace SharpStudioAvalonia.Quartz;

public class Edge : Path
{
    // public string Identifier;
    // public string SourceNodeIdentifier;
    // public string SourcePortIdentifier;
    // public string TargetNodeIdentifier;
    // public string TargetPortIdentifier;

    public Edge(WeakReference<Component> source, WeakReference<Component> target)
    {
        StrokeThickness = 1.5;
        Stroke = new SolidColorBrush(Colors.DodgerBlue);
        Source = source;
        Target = target;
        Bind();
        Loaded += ((sender, args) => UpdatePathData());
    }

    ~Edge()
    {
        Unbind();
    }

    private void UpdatePathData()
    {
        Source.TryGetTarget(out var source);
        Target.TryGetTarget(out var target);
        double x1 = source!.X + source.Bounds.Width, y1 = source.Y, x2 = target!.X, y2 = target.Y;
        double delta = x2 - x1 >= 200 ? (x2 - x1) * .5 : 100;
        Data = Geometry.Parse($"M{x1} {y1} C{x1+delta} {y1} {x2-delta} {y2} {x2} {y2}"); 
    }

    private void OnNodeMoved(object? sender, Point point)
    {
        UpdatePathData();
    }

    private void Bind()
    {
        Source.TryGetTarget(out var source);
        Target.TryGetTarget(out var target);
        source!.Moved += OnNodeMoved;
        target!.Moved += OnNodeMoved;
    }
    
    private void Unbind()
    {
        Source.TryGetTarget(out var source);
        Target.TryGetTarget(out var target);
        source!.Moved -= OnNodeMoved;
        target!.Moved -= OnNodeMoved;
    }
    
    public WeakReference<Component> Source;
    public WeakReference<Component> Target;
    
    
}