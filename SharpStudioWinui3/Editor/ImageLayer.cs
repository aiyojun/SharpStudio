using System.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SharpStudioWinui3.Editor;

public class ImageLayer
{
    public ViewportMatrix2D Viewport { get; set; }
    
    public readonly Image BackgroundImage;
    public Canvas Self { get; set; }
    public Canvas Parent { get; set; }
    
    public ImageLayer(Canvas self, Canvas parent, Image bg, ViewportMatrix2D viewport)
    {
        Self = self;
        Parent = parent;
        Viewport = viewport;
        BackgroundImage = bg;
        // SetupGeometry();
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
        (BackgroundImage.Source, BackgroundImage.Width, BackgroundImage.Height) = 
            (bitmap, bitmap.PixelWidth, bitmap.PixelHeight);
        // RenderOptions.SetBitmapScalingMode(_backgroundImage, BitmapScalingMode.NearestNeighbor);
        Canvas.SetLeft(BackgroundImage, 0);
        Canvas.SetTop(BackgroundImage, 0);
        return this;
    }
    
    public void SetupImageLayout(ImageLayout layout)
    {
        var bitmap = (BitmapImage) BackgroundImage.Source;
        if (bitmap == null) return;
        var containerWidth = ((Canvas) Parent).ActualWidth;
        var containerHeight = ((Canvas) Parent).ActualHeight;
        Console.WriteLine($"SetupImageLayout Width: {containerWidth}, Height: {containerHeight}, Layout: {layout}, Background Image Width: {bitmap.DecodePixelWidth}, Background Image Height: {bitmap.PixelHeight} {BackgroundImage.Source}");
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
        BackgroundImage.Width = width;
        BackgroundImage.Height = height;
        Canvas.SetLeft(BackgroundImage, (containerWidth - width) * 0.5);
        Canvas.SetTop(BackgroundImage, (containerHeight - height) * 0.5);
    }
    
    private void HandleViewChange(object? sender, PropertyChangedEventArgs e)
    {
        var transform = (MatrixTransform) Self.RenderTransform;
        // var matrix = new Matrix{ M11 = Viewport.Scale, M22 = Viewport.Scale, OffsetX = Viewport.OffsetX, OffsetY = Viewport.OffsetY };
        // var matrix = new Matrix();
        // matrix.Scale(Viewport.Scale, Viewport.Scale);
        // matrix.Translate(Viewport.OffsetX, Viewport.OffsetY);
        transform.Matrix = new Matrix{ M11 = Viewport.Scale, M22 = Viewport.Scale, OffsetX = Viewport.OffsetX, OffsetY = Viewport.OffsetY };
    }
    
    public void SetupGeometry()
    {
        // parent.Children.Add(this); 
        Canvas.SetLeft(Self, 0);
        Canvas.SetTop(Self, 0); 
        // Canvas.SetZIndex(Self, 0);
        // Self.Width = Parent.Width;
        // Self.Height = Parent.Height;
        // Self.Children.Add(_backgroundImage);
        Self.RenderTransform = new MatrixTransform { Matrix = Matrix.Identity };
    }

    
}