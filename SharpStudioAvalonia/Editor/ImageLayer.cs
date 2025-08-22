using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SharpStudioAvalonia.Editor;

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
        var bitmap = new Bitmap(path);
        BackgroundImage.Source = bitmap;
        // BackgroundImage.VisualBitmapScalingMode = System.Windows.Media.BitmapScalingMode.NearestNeighbor;
        // var bitmap = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        // (BackgroundImage.Source, BackgroundImage.Width, BackgroundImage.Height) = 
        //     (bitmap, bitmap.PixelWidth, bitmap.PixelHeight);
        // RenderOptions.SetBitmapInterpolationMode();
        // RenderOptions.SetBitmapScalingMode(_backgroundImage, BitmapScalingMode.NearestNeighbor);
        // RenderOptions.BitmapInterpolationMode = "Default";
        Canvas.SetLeft(BackgroundImage, 0);
        Canvas.SetTop(BackgroundImage, 0);
        return this;
    }
    
    public void SetupImageLayout(ImageLayout layout)
    {
        var bitmap = (Bitmap) BackgroundImage.Source;
        if (bitmap == null) return;
        var containerWidth = Parent.Bounds.Width;
        var containerHeight = Parent.Bounds.Height;
        Console.WriteLine($"SetupImageLayout Width: {containerWidth}, Height: {containerHeight}, Layout: {layout} {BackgroundImage.Source}");
        double width = containerWidth;
        double height = width * bitmap.PixelSize.Height / bitmap.PixelSize.Width;
        if (layout == ImageLayout.Contain)
        {
            if (height > containerHeight)
            {
                height = containerHeight;
                width = height * bitmap.PixelSize.Width / bitmap.PixelSize.Height;
            }
        }
        else if (layout == ImageLayout.Cover)
        {
            if (height < containerHeight)
            {
                height = containerHeight;
                width = height * bitmap.PixelSize.Width / bitmap.PixelSize.Height;
            }
        }
        else
        {
            height = bitmap.PixelSize.Height;
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
        var s = Matrix.CreateScale(Viewport.Scale, Viewport.Scale);
        var t = Matrix.CreateTranslation(Viewport.OffsetX, Viewport.OffsetY);
        // matrix.Scale(Viewport.Scale, Viewport.Scale);
        // matrix.Translate(Viewport.OffsetX, Viewport.OffsetY);
        transform.Matrix = s * t;
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
        Self.RenderTransformOrigin = new RelativePoint(new Point(0, 0), RelativeUnit.Relative);
        Self.RenderTransform = new MatrixTransform { Matrix = Matrix.Identity };
    }

    
}