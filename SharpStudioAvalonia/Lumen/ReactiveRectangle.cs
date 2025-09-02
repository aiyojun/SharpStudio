namespace SharpStudioAvalonia.Lumen;

public class ReactiveRectangle : ReactiveShape
{
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private double _rotation;
    
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

    public double Width
    {
        get => _width;
        set
        {
            _width = value;
            OnPropertyChanged();
        }
    }

    public double Height
    {
        get => _height;
        set
        {
            _height = value;
            OnPropertyChanged();
        }
    }
    
    public double Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            OnPropertyChanged();
        }
    }
    
    public override string ToString()
    {
        return "ReactiveRectangle(" + _x + ", " + _y + ", " + _width + ", " + _height +  ", " + _rotation + ")";
    }
}