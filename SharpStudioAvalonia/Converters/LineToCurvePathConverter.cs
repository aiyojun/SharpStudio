using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Mathematics.d2;

namespace SharpStudioAvalonia.Converters;

public class LineToCurvePathConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Line line) return "M5 5L15 15M15 5L5 15";
        double x1 = line.Start.X, y1 = line.Start.Y, x2 = line.End.X, y2 = line.End.Y;
        var deltaX = x2 - x1;
        var delta = Math.Abs(deltaX) >= 200 ? deltaX * 0.5 : 100;
        var r = $"M{x1} {y1} C{x1 + delta} {y1} {x2 - delta} {y2} {x2} {y2}";
        Console.WriteLine($"Convert {r}");
        return r;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // if (value is not string stroke) return new Line(new Point(0, 0), new Point(0, 0));
        // var numbers = (from e in stroke.Replace("M", " ").Replace("L", " ").Replace(",", " ").Split(' ').ToList()
        //     where e != ""
        //     select double.Parse(e)).ToList();
        // return new Line(new Point(numbers[0], numbers[1]), new Point(numbers[^2], numbers[^1]));
        throw new NotImplementedException();
    }
}