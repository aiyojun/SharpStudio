using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SharpStudioWpf;

public sealed class LocaleSupport : INotifyPropertyChanged
{
    public static ResourceManager ResManager { get; set; } = zh_CN.Strings.ResourceManager;

    public static LocaleSupport Instance { get; } = new();

    public string? this[string key] => ResManager.GetString(key, CurrentCulture);

    public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;

    public static void ChangeCulture(string cultureName)
    {
        CurrentCulture = new CultureInfo(cultureName);
        // ResManager = cultureName == "en-US" ? SharpStudioWpf.en_US.Strings.ResourceManager : SharpStudioWpf.zh_CN.Strings.ResourceManager;
        Instance.OnPropertyChanged(string.Empty);
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}