namespace Mathematics.d2;

public readonly struct Point(double x, double y)
{
    public double X { get; } = x;
    public double Y { get; } = y;
    public static implicit operator Vector(Point p) => new(p.X, p.Y);
    public static Point operator -(Point a) => new(-a.X, -a.Y);
    public static Point operator +(Point a, Vector b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    public static Point operator -(Point a, Vector b) => new(a.X - b.X, a.Y - b.Y);
    public static double Distance(Point value1, Point value2) => Math.Sqrt((value2.X - value1.X) * (value2.X - value1.X) + (value2.Y - value1.Y) * (value2.Y - value1.Y));
    public override string ToString() => $"Point({X}, {Y})";
}