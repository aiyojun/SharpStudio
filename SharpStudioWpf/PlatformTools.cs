using System.Reflection;
using System.Windows.Media;
using Microsoft.Win32;

namespace SharpStudioWpf;

public class PlatformTools
{
    public static List<Delegate> GetEventSubscribers(object target, string eventName)
    {
        var eventInfo = target.GetType().GetEvent(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var field = target.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
    
        if (field == null) return new List<Delegate>();

        var eventDelegate = field.GetValue(target) as MulticastDelegate;
        return eventDelegate?.GetInvocationList().ToList() ?? new List<Delegate>();
    }

    public static Color GetSystemAccentColor()
    {
        var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
        var value = key?.GetValue("ColorizationColor");
        if (value != null)
        {
            int colorValue = (int)value;
            byte[] bytes = BitConverter.GetBytes(colorValue);
            return Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
        }
        return Colors.Gray;
    }

    public static bool IsSystemInDarkMode()
    {
        var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return (int)(key?.GetValue("AppsUseLightTheme") ?? 1) == 0;
    }
    
    public static void ApplySystemTheme()
    {
        // GetSystemAccentColor();
        Console.WriteLine("System Color : " + GetSystemAccentColor() + " Dark mode : " + IsSystemInDarkMode());
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            if (e.Category == UserPreferenceCategory.General ||
                e.Category == UserPreferenceCategory.Color)
            {
                Console.WriteLine("System Color : " + GetSystemAccentColor() + " Dark mode : " + IsSystemInDarkMode());
                // Dispatcher.Invoke(() =>
                // {
                // });
            }
        };
    }
}