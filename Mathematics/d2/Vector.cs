namespace Mathematics.d2;

public readonly struct Vector(double x, double y)
{
    public double X { get; } = x;
    public double Y { get; } = y;
    public static implicit operator Point(Vector v) => new(v.X, v.Y);
    public static Vector operator +(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector operator -(Vector a, Vector b) => new(a.X - b.X, a.Y - b.Y);
    public static double operator *(Vector a, Vector b) => a.X * b.X + a.Y * b.Y;
    public static Vector operator *(Vector v, double scalar) => new(v.X * scalar, v.Y * scalar);
    public static Vector operator /(Vector v, double scalar) => new(v.X / scalar, v.Y / scalar);
    public double Length => Math.Sqrt(X * X + Y * Y);
    public Vector Normalize() => this / Length;
    public override string ToString() => $"Vector({X}, {Y})";
}