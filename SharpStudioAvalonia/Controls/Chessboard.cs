using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SharpStudioAvalonia.Controls;

public class Chessboard : Control
{
    public static readonly StyledProperty<int> TileProperty = AvaloniaProperty.Register<Chessboard, int>(nameof (Tile), 10);
    
    public int Tile
    {
        get => GetValue(TileProperty);
        set => SetValue(TileProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var iWidth = Bounds.Width;
        var iHeight = Bounds.Height;
        for (var y = 0; y < iHeight; y += Tile)
        {
            for (var x = 0; x < iWidth; x += Tile)
            {
                var color = (x / Tile + y / Tile) % 2 == 0 
                    ? Color.FromRgb(0x33, 0x33, 0x33) 
                    : Color.FromRgb(0x66, 0x66, 0x66);
                context.FillRectangle(new SolidColorBrush(color), new Rect(x, y, Tile, Tile));
            }
        }
    }
}