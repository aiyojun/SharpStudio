using System.Collections.Generic;
using System.Linq;
using Mathematics.d2;

namespace SharpStudioAvalonia.Lumen;

public class AnchorGroup(int index, ReactiveShape shape)
{
    public readonly int Index = index;
    public readonly ReactiveShape Shape = shape;
    private int _selectedIndex = -1;
    public ReactiveShape? CachedShape;

    public List<Point> Anchors => Shape switch
    {
        ReactiveRectangle rectangle => Enumerable.Range(0, 9)
            .Select(e => ShapeTools.GetRectangleAnchor(rectangle, e))
            .ToList(),
        ReactiveCircle circle => Enumerable.Range(0, 4).Select(e => ShapeTools.GetCircleAnchor(circle, e)).ToList(),
        ReactivePolygon polygon => polygon.Points.Select(p => new Point(p.X, p.Y)).ToList(),
        _ => []
    };

    public void Save(int index = -1)
    {
        _selectedIndex = index;
        CachedShape = ShapeTools.Clone(Shape);
    }
    
    public void Restore()
    {
        _selectedIndex = -1;
        CachedShape = null;
    }

    public void DragResize(Point point)
    {
        if (CachedShape == null) return;
        switch (Shape)
        {
            case ReactiveRectangle rectangle when _selectedIndex == 8:
                RotateRectangle((CachedShape as ReactiveRectangle)!, rectangle, point);
                break;
            case ReactiveRectangle rectangle:
                ResizeRectangle((CachedShape as ReactiveRectangle)!, rectangle, _selectedIndex, point);
                break;
            case ReactiveCircle circle:
                circle.Radius = (point - new Point(circle.X, circle.Y)).Length;
                break;
            case ReactivePolygon polygon:
                polygon.Points[_selectedIndex] = new Point(point.X, point.Y);
                break;
        }
    }
    
    private static void ResizeRectangle(ReactiveRectangle start, ReactiveRectangle rectangle, int fixedPointIndex, Point point)
    {
        if (fixedPointIndex is < 0 or > 7) return;
        int[] indices = [4, 5, 6, 7, 0, 1, 2, 3];
        var fixedPoint = ShapeTools.GetRectangleAnchor(rectangle, indices[fixedPointIndex]);
        var diagonalVector = point - fixedPoint;
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
        var deltaAngle = ShapeTools.CalculateVectorClockwiseAngle(baseVector, point - new Point(start.X, start.Y));
        rectangle.Rotation = ShapeTools.NormalizeAngle(startAngle + deltaAngle);
    }
}