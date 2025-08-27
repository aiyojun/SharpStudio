using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace SharpStudioAvalonia.Converters;

public class WindowStateToPathConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is WindowState.Maximized
            ? "M256 0v256H0v768h768V768h256V0H256z m426.667 938.667H85.333V341.333h597.334v597.334z m256-256H768V256H341.333V85.333h597.334v597.334z"
            : "M1,1L1,11 11,11 11,1z M0,0L12,0 12,12 0,12z";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}