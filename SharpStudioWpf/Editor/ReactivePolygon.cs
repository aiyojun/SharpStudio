using System.Windows;

namespace SharpStudioWpf.Editor;

public class ReactivePolygon : ReactiveShape
{
    private List<Point> _points = [];

    public List<Point> Points
    {
        get => _points;
        set
        {
            _points = value;
            OnPropertyChanged();
        }
    }

    public void AddPoint(Point point)
    {
        _points.Add(point);
        OnPropertyChanged(nameof(Points));
    }

    public void RemovePointAt(int index)
    {
        _points.RemoveAt(index);
        OnPropertyChanged(nameof(Points));
    }

    public void UpdateAt(int index, Point point)
    {
        _points[index] = point;
        OnPropertyChanged(nameof(Points));
    }

    public void PopBack()
    {
        _points.RemoveAt(_points.Count - 1);
        OnPropertyChanged(nameof(Points));
    }
    
    public void UpdateBack(Point point)
    {
        _points[^1] = point;
        OnPropertyChanged(nameof(Points));
    }
}