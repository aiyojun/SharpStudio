using Mathematics.d2;

namespace SharpStudioAvalonia.Editor;

public class CursorState
{
    public int Buttons { get; set; } = -1;
    private double _x;
    private double _y;
    public double X { get => _x; }
    public double Y { get => _y; }
    public Point Start => new(_x, _y);
    public void Save(Point p) => (_x, _y) = (p.X, p.Y);
}