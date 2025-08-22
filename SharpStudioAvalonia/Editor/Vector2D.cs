using System;
using Avalonia;

namespace SharpStudioAvalonia.Editor;

public struct Vector2D(double x, double y)
{
    public double X = x;
    public double Y = y;

    public static Vector2D operator +(Vector2D a, Vector2D b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2D operator -(Vector2D a, Vector2D b) => new(a.X - b.X, a.Y - b.Y);
    public static double operator *(Vector2D a, Vector2D b) => a.X * b.X + a.Y * b.Y;
    public static Vector2D operator *(Vector2D v, double scalar) => new(v.X * scalar, v.Y * scalar);
    public static Vector2D operator /(Vector2D v, double scalar) => new(v.X / scalar, v.Y / scalar);
    public static Vector2D Subtract(Point p1, Point p2) => new(p1.X - p2.X, p1.Y - p2.Y);
    public static Point Add(Point p, Vector2D v) => new(p.X + v.X, p.Y + v.Y);
    
    public Point ToPoint() => new(X, Y);
    public static Vector2D FromPoint(Point p) => new(p.X, p.Y);
    
    public double Length => Math.Sqrt(X * X + Y * Y);
    public Vector2D Normalize() => this / Length;
    
    public override string ToString() => $"({X}, {Y})";
}