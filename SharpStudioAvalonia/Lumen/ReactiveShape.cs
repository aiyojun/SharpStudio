using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharpStudioAvalonia.Lumen;

public class ReactiveShape : INotifyPropertyChanged
{
    public string? Label { get; set; }
    
    public string? Color { get; set; }
    
    public string? Id { get; set; }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}