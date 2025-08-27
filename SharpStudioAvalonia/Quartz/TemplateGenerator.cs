using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Newtonsoft.Json.Linq;

namespace SharpStudioAvalonia.Quartz;

public static class TemplateGenerator
{
    public static object? Parse(JObject obj)
    {
        var type = obj.GetValue("type")!.Value<string>();
        var label = obj.GetValue("label")!.Value<string>();
        object? r = null;
        switch (type)
        {
            case "text":
                r = GetText(label!);
                break;
            case "text-right":
                r = GetText(label!, false);
                break;
            case "input":
                r = GetTextInput(label!);
                break;
            case "number-input":
                r = GetNumberInput(label!);
                break;
            case "check":
                r = GetCheckBox(label!);
                break;
            case "selection":
                r = GetSelection(label!, ["OptionA", "OptionB"]);
                break;
        }
        return r;
    }

    public static object? GetHeader(string label)
    {
        var border = new Border
        {
            Margin = Thickness.Parse("2"),
            Padding = Thickness.Parse("5 2"),
            CornerRadius = CornerRadius.Parse("4"),
            // Background = new SolidColorBrush(Color.FromArgb(0x88, 0x00, 0x00, 0x00)),
            Child = new TextBlock { Text = label }
        };
        
        return border;
    }
    
    public static object? GetText(string label, bool left=true)
    {
        var border = new Border
        {
            Margin = Thickness.Parse("2"),
            Padding = Thickness.Parse("5 2"),
            CornerRadius = CornerRadius.Parse("4"),
            // Background = new SolidColorBrush(Color.FromArgb(0x88, 0x00, 0x00, 0x00)),
            Child = new TextBlock { Text = label, TextAlignment = left ? TextAlignment.Left : TextAlignment.Right }
        };
        return border;
    }
    
    public static object? GetTextInput(string label)
    {
        var border = new Border
        {
            Margin = Thickness.Parse("2"),
            Padding = Thickness.Parse("5 2"),
            CornerRadius = CornerRadius.Parse("4"), 
            Background = new SolidColorBrush(Color.FromArgb(0x88, 0x00, 0x00, 0x00)),
            Child = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Center,
                ColumnDefinitions = new ColumnDefinitions
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star))
                },
                Children =
                {
                    new TextBlock { Text = label, Margin = Thickness.Parse("0 0 5 0"), VerticalAlignment = VerticalAlignment.Center },
                    new TextBox() { TextAlignment = TextAlignment.Right }
                }
            }
        };
        Grid.SetColumn((border.Child as Grid)!.Children[0], 0);
        Grid.SetColumn((border.Child as Grid)!.Children[1], 1);
        return border;
    }
    
    public static object? GetNumberInput(string label)
    {
        var border = new Border
        {
            Margin = Thickness.Parse("2"),
            Padding = Thickness.Parse("5 2"),
            CornerRadius = CornerRadius.Parse("4"),
            Background = new SolidColorBrush(Color.FromArgb(0x88, 0x00, 0x00, 0x00)),
            Child = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Center,
                ColumnDefinitions = new ColumnDefinitions
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star))
                },
                Children =
                {
                    new TextBlock { Text = label, Margin = Thickness.Parse("0 0 5 0"), VerticalAlignment = VerticalAlignment.Center },
                    new NumericUpDown() { TextAlignment = TextAlignment.Right }
                }
            }
        };
        Grid.SetColumn((border.Child as Grid)!.Children[0], 0);
        Grid.SetColumn((border.Child as Grid)!.Children[1], 1);
        return border;
    }
    
    public static object? GetCheckBox(string label)
    {
        var border = new Border
        {
            Margin = Thickness.Parse("2"),
            Padding = Thickness.Parse("5 2"),
            CornerRadius = CornerRadius.Parse("4"),
            Background = new SolidColorBrush(Color.FromArgb(0x88, 0x00, 0x00, 0x00)),
            Child = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Center,
                ColumnDefinitions = new ColumnDefinitions
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star))
                },
                Children =
                {
                    new TextBlock { Text = label, Margin = Thickness.Parse("0 0 5 0"), VerticalAlignment = VerticalAlignment.Center },
                    new CheckBox()
                }
            }
        };
        Grid.SetColumn((border.Child as Grid)!.Children[0], 0);
        Grid.SetColumn((border.Child as Grid)!.Children[1], 1);
        return border;
    }
    
    public static object? GetSelection(string label, List<string> options)
    {
        var border = new Border
        {
            Margin = Thickness.Parse("2"),
            Padding = Thickness.Parse("5 2"),
            CornerRadius = CornerRadius.Parse("4"),
            Background = new SolidColorBrush(Color.FromArgb(0x88, 0x00, 0x00, 0x00)),
            Child = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Center,
                ColumnDefinitions = new ColumnDefinitions
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star))
                },
                Children =
                {
                    new TextBlock { Text = label, Margin = Thickness.Parse("0 0 5 0"), VerticalAlignment = VerticalAlignment.Center },
                    // new ComboBox
                    // {
                    //     Items = new ItemCollection(),
                    // }
                }
            }
        };
        Grid.SetColumn((border.Child as Grid)!.Children[0], 0);
        Grid.SetColumn((border.Child as Grid)!.Children[1], 1);
        return border;
    }
}