using System.Windows;

namespace SharpStudioWpf.Editor;

public class CursorState
{
    public int Buttons { get; set; } = -1;
    public Point? Start { get; set; }
}