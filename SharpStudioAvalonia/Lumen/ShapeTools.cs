using System;
using System.Linq;
using Mathematics.d2;

namespace SharpStudioAvalonia.Lumen;

public static class ShapeTools
{
    public static bool InCircle(Point point, ReactiveCircle circle)
    {
        return (new Point(circle.X, circle.Y) - point).Length <= circle.Radius;
    }

    public static bool InPolygon(Point point, ReactivePolygon polygon)
    {
        var c = false;
        var l = polygon.Points.Count;

        for (int i = 0, j = l - 1; i < l; j = i++)
        {
            var pi = polygon.Points[i];
            var pj = polygon.Points[j];

            var intersect =
                ((pi.Y <= point.Y && point.Y < pj.Y) || (pj.Y <= point.Y && point.Y < pi.Y)) &&
                (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y) + pi.X);

            if (intersect)
            {
                c = !c;
            }
        }

        return c;
    }

    public static bool InRectangle(Point point, ReactiveRectangle rectangle)
    {
        var halfWidth = rectangle.Width * 0.5;
        var halfHeight = rectangle.Height * 0.5;

        return point.X >= rectangle.X - halfWidth &&
               point.X <= rectangle.X + halfWidth &&
               point.Y >= rectangle.Y - halfHeight &&
               point.Y <= rectangle.Y + halfHeight;
    }

    public static ReactiveShape Clone(ReactiveShape shape)
    {
        return shape switch
        {
            ReactiveRectangle rectangle => new ReactiveRectangle
            {
                Width = rectangle.Width, Height = rectangle.Height, X = rectangle.X, Y = rectangle.Y,
                Rotation = rectangle.Rotation,
                Color = rectangle.Color, Id = rectangle.Id, Label = rectangle.Label
            },
            ReactiveCircle circle => new ReactiveCircle
            {
                Radius = circle.Radius, X = circle.X, Y = circle.Y, Color = circle.Color, Id = circle.Id,
                Label = circle.Label
            },
            ReactivePolygon polygon => new ReactivePolygon
                { Points = polygon.Points.Select(p => new Point(p.X, p.Y)).ToList() },
            _ => throw new NotImplementedException()
        };
    }

    public static void MoveShape(ReactiveShape start, ReactiveShape shape, Vector delta)
    {
        if (start is ReactiveRectangle startRectangle && shape is ReactiveRectangle rectangle)
        {
            rectangle.X = startRectangle.X + delta.X;
            rectangle.Y = startRectangle.Y + delta.Y;
        }
        else if (start is ReactiveCircle startCircle && shape is ReactiveCircle circle)
        {
            circle.X = startCircle.X + delta.X;
            circle.Y = startCircle.Y + delta.Y;
        }
        else if (start is ReactivePolygon startPolygon && shape is ReactivePolygon polygon)
        {
            for (var i = 0; i < polygon.Points.Count; i++)
            {
                var ps = startPolygon.Points[i];
                polygon.UpdateAt(i, new Point(ps.X + delta.X, ps.Y + delta.Y));
            }
        }
    }

    public static Vector GetAngleVector(double degree)
    {
        var radian = degree * Math.PI / 180.0;
        return new Vector(Math.Sin(radian), -Math.Cos(radian));
    }

    public static (Vector projection, Vector perpendicular) Decompose(Vector direction, Vector diagonal)
    {
        var distance = direction.Length;
        if (distance == 0)
            throw new Exception("invalid vector");
        var u0 = direction / distance;
        var dotProduct = diagonal * u0;
        var projection = u0 * dotProduct;
        var perpendicular = diagonal - projection;
        return (projection, perpendicular);
    }

    public static Point GetRectangleAnchor(ReactiveRectangle rectangle, int index)
    {
        const int far = 15;
        var rotation = rectangle.Rotation;
        double rw = rectangle.Width * 0.5, rh = rectangle.Height * 0.5;
        double cx = rectangle.X, cy = rectangle.Y;
        var theta = -rotation * Math.PI / 180.0;
        double cos = Math.Cos(theta), sin = Math.Sin(theta);
        return index switch
        {
            0 => new Point(cx - rw * cos - rh * sin, cy + rw * sin - rh * cos),
            1 => new Point(cx - rh * sin, cy - rh * cos),
            2 => new Point(cx + rw * cos - rh * sin, cy - rw * sin - rh * cos),
            3 => new Point(cx + rw * cos, cy - rw * sin),
            4 => new Point(cx + rw * cos + rh * sin, cy - rw * sin + rh * cos),
            5 => new Point(cx + rh * sin, cy + rh * cos),
            6 => new Point(cx - rw * cos + rh * sin, cy + rw * sin + rh * cos),
            7 => new Point(cx - rw * cos, cy + rw * sin),
            8 => new Point(cx - (rh + far) * sin, cy - (rh + far) * cos),
            _ => throw new Exception("invalid index"),
        };
    }

    public static Point GetCircleAnchor(ReactiveCircle circle, int index)
    {
        return index switch
        {
            0 => new Point(circle.X, circle.Y - circle.Radius),
            1 => new Point(circle.X + circle.Radius, circle.Y),
            2 => new Point(circle.X, circle.Y + circle.Radius),
            3 => new Point(circle.X - circle.Radius, circle.Y),
            _ => throw new Exception("invalid index"),
        };
    }

    public static double NormalizeAngle(double degree)
    {
        var deg = degree;
        while (deg >= 360) deg -= 360;
        while (deg < 0) deg += 360;
        return deg;
    }

    public static double CalculateVectorClockwiseAngle(Vector v1, Vector v2)
    {
        return NormalizeAngle((Math.Atan2(v2.Y, v2.X) - Math.Atan2(v1.Y, v1.X)) / Math.PI * 180);
    }
}