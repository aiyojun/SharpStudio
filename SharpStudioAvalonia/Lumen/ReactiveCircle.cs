namespace SharpStudioAvalonia.Lumen;

public class ReactiveCircle : ReactiveShape
{
    private double _x;
    private double _y;
    private double _radius;

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

    public double Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            OnPropertyChanged();
        }
    }
}