using System;
using Avalonia;
using Avalonia.Controls;
using Point = Mathematics.d2.Point;

namespace SharpStudioAvalonia.Quartz;

public class Port
{
    public Component Component { get; }

    public Border View { get; }

    public readonly bool IsImport;

    public readonly string Identifier;

    public bool IsExport => !IsImport;

    private Port(string identifier, Component component, Border view, bool isImport)
    {
        Identifier = identifier;
        IsImport = isImport;
        Component = component;
        View = view;
    }

    public static Port CreateImport(string identifier, Component component, Border view)
    {
        return new Port(identifier, component, view, true);
    }
    
    public static Port CreateExport(string identifier, Component component, Border view)
    {
        return new Port(identifier, component, view, false);
    }

    public Point GetPosition()
    {
        // var x = View.TryGetTarget(out var t) ? t : null;
        // View.TryGetTarget(out var view);
        // Component.TryGetTarget(out var component);
        var matrix = View.TransformToVisual(Component)!.Value;
        var p0 = matrix.Transform(View.Bounds.TopLeft);
        var p1 = matrix.Transform(View.Bounds.BottomRight);
        return new Point((p0.X + p1.X) * 0.5, (p0.Y + p1.Y) * 0.5);
    }
}