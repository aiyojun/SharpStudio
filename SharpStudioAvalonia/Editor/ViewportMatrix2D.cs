using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;

namespace SharpStudioAvalonia.Editor;

public sealed partial class ViewportMatrix2D : INotifyPropertyChanged
{
    private double _offsetX;
    private double _offsetY;
    private double _scale = 1.0;
    private readonly double[] _cache  = new double[3];
    private readonly double[] _cursor = new double[2];
    
    public double OffsetX
    {
        get => _offsetX;
        set
        {
            _offsetX = value;
            OnPropertyChanged();
        }
    }

    public double OffsetY
    {
        get => _offsetY;
        set
        {
            _offsetY = value;
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
        var targetOffsetX = pivot.X + (OffsetX - pivot.X) * targetScale / Scale;
        var targetOffsetY = pivot.Y + (OffsetY - pivot.Y) * targetScale / Scale;
        (OffsetX, OffsetY, Scale) = (targetOffsetX, targetOffsetY, targetScale);
    }

    public void Save(Point cursor)
    {
        (_cache[0], _cache[1], _cache[2]) = (OffsetX, OffsetY, Scale);
        (_cursor[0], _cursor[1]) = (cursor.X, cursor.Y);
    }

    public void MoveTo(Point end)
    {
        (OffsetX, OffsetY) = (_cache[0] + end.X - _cursor[0], _cache[1] + end.Y - _cursor[1]);
    }

    public void Reset()
    {
        OffsetX = 0;
        OffsetY = 0;
        Scale = 1;
        Array.Fill(_cache, 0);
        Array.Fill(_cursor, 0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Point Absolute(Point point)
    {
        return new Point((point.X - OffsetX) / Scale, (point.Y - OffsetY) / Scale);
    }
    
    public Point Absolute(double x, double y)
    {
        return new Point((x - OffsetX) / Scale, (y - OffsetY) / Scale);
    }

    public Point Relative(Point point)
    {
        return new Point(point.X * Scale + OffsetX, point.Y * Scale + OffsetY);
    }
    
    public Point Relative(double x, double y)
    {
        return new Point(x * Scale + OffsetX, y * Scale + OffsetY);
    }

    public override string ToString()
    {
        return _offsetX + " " + _offsetY + " " + _scale;
    }
}