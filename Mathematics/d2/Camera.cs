using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mathematics.d2;

public class Camera : INotifyPropertyChanged
{
    private double _x;
    private double _y;
    private double _scale = 1.0;
    private readonly double[] _cache  = new double[3];
    private readonly double[] _cursor = new double[2];
    
    public double X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get => _y;
        set
        {
            _y = value;
            OnPropertyChanged();
        }
    }

    public double Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            OnPropertyChanged();
        }
    }

    public void ZoomTo(double targetScale, Point pivot)
    {
        var targetOffsetX = pivot.X + (X - pivot.X) * targetScale / Scale;
        var targetOffsetY = pivot.Y + (Y - pivot.Y) * targetScale / Scale;
        (X, Y, Scale) = (targetOffsetX, targetOffsetY, targetScale);
    }

    public void Save(Point cursor)
    {
        (_cache[0], _cache[1], _cache[2]) = (X, Y, Scale);
        (_cursor[0], _cursor[1]) = (cursor.X, cursor.Y);
    }

    public void MoveTo(Point end)
    {
        (X, Y) = (_cache[0] + end.X - _cursor[0], _cache[1] + end.Y - _cursor[1]);
    }

    public void Reset()
    {
        (X, Y,  Scale) = (0, 0, 1.0);
        Array.Fill(_cache, 0);
        Array.Fill(_cursor, 0);
    }
    
    public Point ConvertToWorld(Point point) => new((point.X - X) / Scale, (point.Y - Y) / Scale);
    
    public Point ConvertToScreen(Point point) => new(point.X * Scale + X, point.Y * Scale + Y);

    public override string ToString() => $"Camera({_x}, {_y}, {_scale})";
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}