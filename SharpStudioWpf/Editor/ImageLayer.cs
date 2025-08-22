using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SharpStudioWpf.Editor;

public class ImageLayer : Canvas
{
    public ViewportMatrix2D Viewport { get; set; }
    
    private readonly Image _backgroundImage = new();
    
    public ImageLayer(Canvas parent, ViewportMatrix2D viewport)
    {
        Viewport = viewport;
        SetGeometry(parent);
        Viewport.PropertyChanged += HandleViewChange;
        // Loaded += (sender, args) =>
        // {
        //     SetupImageLayout(ImageLayout.Contain);
        // };
    }
    
    ~ImageLayer()
    {
        Viewport.PropertyChanged -= HandleViewChange;
    }

    public ImageLayer Load(string path)
    {
        var bitmap = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        (_backgroundImage.Source, _backgroundImage.Width, _backgroundImage.Height) = 
            (bitmap, bitmap.PixelWidth, bitmap.PixelHeight);
        RenderOptions.SetBitmapScalingMode(_backgroundImage, BitmapScalingMode.NearestNeighbor);
        SetLeft(_backgroundImage, 0);
        SetTop(_backgroundImage, 0);
        return this;
    }
    
    public void SetupImageLayout(ImageLayout layout)
    {
        var bitmap = (BitmapImage) _backgroundImage.Source;
        if (bitmap == null) return;
        var containerWidth = ((Canvas) Parent).ActualWidth;
        var containerHeight = ((Canvas) Parent).ActualHeight;
        double width = containerWidth;
        double height = width * bitmap.PixelHeight / bitmap.PixelWidth;
        if (layout == ImageLayout.Contain)
        {
            if (height > containerHeight)
            {
                height = containerHeight;
                width = height * bitmap.PixelWidth / bitmap.PixelHeight;
            }
        }
        else if (layout == ImageLayout.Cover)
        {
            if (height < containerHeight)
            {
                height = containerHeight;
                width = height * bitmap.PixelWidth / bitmap.PixelHeight;
            }
        }
        else
        {
            height = bitmap.PixelHeight;
        }
        _backgroundImage.Width = width;
        _backgroundImage.Height = height;
        SetLeft(_backgroundImage, (containerWidth - width) * 0.5);
        SetTop(_backgroundImage, (containerHeight - height) * 0.5);
    }
    
    private void HandleViewChange(object? sender, PropertyChangedEventArgs e)
    {
        var transform = (MatrixTransform) RenderTransform;
        var matrix = new Matrix();
        matrix.Scale(Viewport.Scale, Viewport.Scale);
        matrix.Translate(Viewport.OffsetX, Viewport.OffsetY);
        transform.Matrix = matrix;
    }
    
    private void SetGeometry(Canvas parent)
    {
        parent.Children.Add(this); 
        SetLeft(this, 0);
        SetTop(this, 0); 
        SetZIndex(this, 0);
        Width = parent.Width;
        Height = parent.Height;
        Children.Add(_backgroundImage);
        RenderTransform = new MatrixTransform { Matrix = Matrix.Identity };
    }

    
}