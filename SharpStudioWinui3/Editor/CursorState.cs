using Windows.Foundation;

namespace SharpStudioWinui3.Editor;

public class CursorState
{
    public int Buttons { get; set; } = -1;
    public Point? Start { get; set; }
}