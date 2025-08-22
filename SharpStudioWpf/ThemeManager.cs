using System.Windows;

namespace SharpStudioWpf;

public sealed class ThemeManager
{
    public static ThemeManager Instance { get; } = new ThemeManager();

    public ThemeManager ApplyDarkTheme()
    {
        return ApplyTheme("pack://application:,,,/SharpStudioWpf;component/Themes/ThemeDark.xaml");
    }
    
    public ThemeManager ApplyLightTheme()
    {
        return ApplyTheme("pack://application:,,,/SharpStudioWpf;component/Themes/ThemeLight.xaml");
    }
    
    public ResourceDictionary ThemeResource { get; set; } = new ResourceDictionary();

    public ThemeManager ApplyTheme(string filepath)
    {
        Console.WriteLine($"Application {Application.Current}");
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var oldThemeDict = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.StartsWith("Themes/"));
        var newThemeDict = new ResourceDictionary { Source = new Uri(filepath) };
        if (oldThemeDict != null)
        {
            var index = dictionaries.IndexOf(oldThemeDict);
            dictionaries[index] = newThemeDict;
        }
        else
        {
            dictionaries.Add(newThemeDict);
        }
        return this;
    }
}