using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Newtonsoft.Json.Linq;

namespace SharpStudioAvalonia.Quartz;

public static class TemplateGenerator
{
    public static object? Parse(JObject obj)
    {
        var type = obj.GetValue("type")!.Value<string>();
        var label = obj.GetValue("label")!.Value<string>();
        var r = type switch
        {
            // "import" => GetImport(label!),
            // "export" => GetExport(label!),
            "text" => GetText(label!),
            "text-right" => GetText(label!, false),
            "input" => GetTextInput(label!),
            "number-input" => GetNumberInput(label!),
            "check" => GetCheckBox(label!),
            "selection" => GetSelection(label!, ["OptionA", "OptionB"]),
            "button" => GetButton(label!, obj.GetValue("tips")!.Value<string>()!),
            "image" => GetImage(label!),
            _ => null
        };
        return r;
    }

    public static object GetHeader(string label)
    {
        var border = new Border
        {
            Classes = { "TextBorder" },
            Child = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border { Classes = { "NodeFold" } },
                    new TextBlock { Text = label }
                }
            }
        };
        
        return border;
    }
    
    // public static object GetHeader(string label)
    // {
    //     var border = new Border
    //     {
    //         Classes = { "TextBorder" },
    //         Child = new TextBlock { Text = label }
    //     };
    //     
    //     return border;
    // }

    public static object GetImport(string label, out Border view)
    {
        var border = new Border
        {
            Classes = { "TextBorder" },
            Child = new RelativePanel
            {
                Children =
                {
                    new TextBlock { Text = label },
                    new Border { Classes = { "Port", "Import" } }
                }
            }
        };
        var panel = (border.Child as RelativePanel)!;
        panel.ClipToBounds = false;
        var textBlock = (panel.Children[0] as TextBlock)!;
        var circle = (panel.Children[1] as Border)!;
        RelativePanel.SetAlignLeftWithPanel(textBlock, true);
        RelativePanel.SetAlignVerticalCenterWithPanel(textBlock, true);
        RelativePanel.SetLeftOf(circle, textBlock);
        RelativePanel.SetAlignVerticalCenterWithPanel(circle, true);
        view = circle;
        return border;
    }
    
    public static object GetExport(string label, out Border view)
    {
        var border = new Border
        {
            Classes = { "TextBorder" },
            Child = new RelativePanel
            {
                Children =
                {
                    new TextBlock { Text = label },
                    new Border { Classes = { "Port", "Export" } }
                }
            }
        };
        var panel = (border.Child as RelativePanel)!;
        panel.ClipToBounds = false;
        var textBlock = (panel.Children[0] as TextBlock)!;
        var circle = (panel.Children[1] as Border)!;
        RelativePanel.SetAlignRightWithPanel(textBlock, true);
        RelativePanel.SetAlignVerticalCenterWithPanel(textBlock, true);
        RelativePanel.SetRightOf(circle, textBlock);
        RelativePanel.SetAlignVerticalCenterWithPanel(circle, true);
        view = circle;
        return border;
    }
    
    public static object GetText(string label, bool left=true, bool port=false)
    {
        var border = new Border { Classes = { "TextBorder" } };
        if (!port)
        {
            border.Child = new TextBlock { Text = label, TextAlignment = left ? TextAlignment.Left : TextAlignment.Right};
            return border;
        }
        border.Child = new RelativePanel
        {
            Children =
            {
                new TextBlock { Text = label },
                new Border
                {
                    Width = 12, Height = 12, CornerRadius = CornerRadius.Parse("6"),
                    Background = new SolidColorBrush(Colors.White)
                }
            }
        };
        var panel = (border.Child as RelativePanel)!;
        panel.ClipToBounds = false;
        var textBlock = (panel.Children[0] as TextBlock)!;
        var circle = (panel.Children[1] as Border)!;
        if (left)
        {
            RelativePanel.SetAlignLeftWithPanel(textBlock, true);
            RelativePanel.SetAlignVerticalCenterWithPanel(textBlock, true);
            RelativePanel.SetLeftOf(circle, textBlock);
            RelativePanel.SetAlignVerticalCenterWithPanel(circle, true);
            circle.Margin = Thickness.Parse("0,0,5,0");
        }
        else
        {
            RelativePanel.SetAlignRightWithPanel(textBlock, true);
            RelativePanel.SetAlignVerticalCenterWithPanel(textBlock, true);
            RelativePanel.SetRightOf(circle, textBlock);
            RelativePanel.SetAlignVerticalCenterWithPanel(circle, true);
            circle.Margin = Thickness.Parse("5,0,0,0");
        }
        return border;
    }
    
    public static object GetTextInput(string label)
    {
        var border = new Border
        {
            Classes = { "WidgetBorder" },
            Child = new Grid
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
                    new TextBox()
                }
            }
        };
        Grid.SetColumn((border.Child as Grid)!.Children[0], 0);
        Grid.SetColumn((border.Child as Grid)!.Children[1], 1);
        return border;
    }
    
    public static object GetNumberInput(string label)
    {
        var border = new Border
        {
            Classes = { "WidgetBorder" },
            Child = new Grid
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
                    new NumericUpDown()
                }
            }
        };
        Grid.SetColumn((border.Child as Grid)!.Children[0], 0);
        Grid.SetColumn((border.Child as Grid)!.Children[1], 1);
        return border;
    }
    
    public static object GetButton(string label, string tips)
    {
        var border = new Border
        {
            Classes = { "WidgetBorder" },
            Child = new Grid
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
                    new Button { Content = tips }
                }
            }
        };
        Grid.SetColumn((border.Child as Grid)!.Children[0], 0);
        Grid.SetColumn((border.Child as Grid)!.Children[1], 1);
        return border;
    }
    
    public static object GetCheckBox(string label)
    {
        var border = new Border
        {
            Classes = { "WidgetBorder" },
            Child = new Grid
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
    
    public static object GetSelection(string label, List<string> options)
    {
        var border = new Border
        {
            Classes = { "WidgetBorder" },
            Child = new Grid
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
                    new ComboBox()
                }
            }
        };
        Grid.SetColumn((border.Child as Grid)!.Children[0], 0);
        Grid.SetColumn((border.Child as Grid)!.Children[1], 1);
        var combobox = (border.Child as Grid)!.Children[1] as ComboBox;
        var items = combobox.Items; 
        items.Clear();
        options.ForEach(e => items.Add(e));
        combobox.AddHandler(Button.PointerPressedEvent, (sender, e) => e.Handled = true, Avalonia.Interactivity.RoutingStrategies.Bubble);
        return border;
    }
    
    public static object GetImage(string label)
    {
        var border = new Border
        {
            Classes = { "ImageBorder" },
            Child = new Image { Source = new Bitmap(AssetLoader.Open(new Uri("avares://SharpStudioAvalonia/Assets/chessboard.png"))) },
            // Child = new Image { Source = new Bitmap("C:\\Users\\jun.dai\\Documents\\GitHub\\SharpStudio\\SharpStudioWpf\\chessboard.png") },
        //     Child = new Grid
        //     {
        //         VerticalAlignment = VerticalAlignment.Center,
        //         RowDefinitions = new RowDefinitions
        //         {
        //             new RowDefinition(GridLength.Auto),
        //             new RowDefinition(new GridLength(1, GridUnitType.Star))
        //         },
        //         Children =
        //         {
        //             new TextBlock { Text = label, Margin = Thickness.Parse("0 0 0 5"), VerticalAlignment = VerticalAlignment.Center },
        //             new Image { Source = new Bitmap("C:\\Users\\jun.dai\\Documents\\GitHub\\SharpStudio\\SharpStudioWpf\\chessboard.png") }
        //         }
        //     }
        };
        // Grid.SetRow((border.Child as Grid)!.Children[0], 0);
        // Grid.SetRow((border.Child as Grid)!.Children[1], 1);
        return border;
    }
}