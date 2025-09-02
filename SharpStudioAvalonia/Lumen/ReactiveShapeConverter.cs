using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using Avalonia.Collections;
using Avalonia.Data.Converters;

namespace SharpStudioAvalonia.Lumen;

public class ReactiveShapeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Parse((string)value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Dumps((IList<ReactiveShape>)value);
    }
    
    
    public static IList<ReactiveShape> Parse(string data)
    {
        AvaloniaList<ReactiveShape> shapes = [];
        try
        {
            var obj = JsonNode.Parse(data)!;
            var shapesArray = obj["shapes"]!.AsArray();
            foreach (var shape in shapesArray)
            {
                var type = (string)shape!["type"]!;
                var label = (string?)shape["label"];
                var color = (string?)shape["color"];
                switch (type)
                {
                    case "rect":
                    case "rectangle":
                    {
                        var x = shape["x"]!.GetValue<double>();
                        var y = shape["y"]!.GetValue<double>();
                        var width = shape["width"]!.GetValue<double>();
                        var height = shape["height"]!.GetValue<double>();
                        shapes.Add(new ReactiveRectangle
                            { X = x, Y = y, Width = width, Height = height, Color = color, Label = label });
                        break;
                    }
                    case "circle":
                    {
                        var x = shape["x"]!.GetValue<double>();
                        var y = shape["y"]!.GetValue<double>();
                        var radius = shape["radius"]!.GetValue<double>();
                        shapes.Add(new ReactiveCircle() { X = x, Y = y, Radius = radius, Color = color, Label = label });
                        break;
                    }
                    case "polygon":
                    {
                        List<Mathematics.d2.Point> points = [];
                        for (var j = 0; j < shape!["points"]!.AsArray().Count; j++)
                        {
                            var point = shape["points"]![j]!;
                            points.Add(new Mathematics.d2.Point((double)point["x"]!, (double)point["y"]!));
                        }

                        shapes.Add(new ReactivePolygon { Points = points, Color = color, Label = label });
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return shapes;
    }

    public static string Dumps(IList<ReactiveShape> shapes)
    {
        var shapesArray = new JsonArray();
        foreach (var shape in shapes)
        {
            var obj = new JsonObject
            {
                ["color"] = shape.Color!,
                ["label"] = shape.Label!
            };
            if (shape is ReactiveRectangle rectangle)
            {
                obj["x"] = rectangle.X;
                obj["y"] = rectangle.Y;
                obj["width"] = rectangle.Width;
                obj["height"] = rectangle.Height;
            }
            else if (shape is ReactiveCircle circle)
            {
                obj["x"] = circle.X;
                obj["y"] = circle.Y;
                obj["radius"] = circle.Radius;
            }
            else if (shape is ReactivePolygon polygon)
            {
                var points = new JsonArray();
                foreach (var point in polygon.Points)
                {
                    var p = new JsonObject
                    {
                        ["x"] = point.X,
                        ["y"] = point.Y
                    };
                    points.Add(p);
                }
                obj["points"] = points;
            }
            shapesArray.Add(obj);
        }
        return new JsonObject { ["shapes"] = shapesArray }.ToJsonString();
    }
}