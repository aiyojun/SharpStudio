using Microsoft.UI.Xaml.Media;

namespace SharpStudioWinui3;

public static class ApplicationHelper
{
    public static Window? Window = null;
        
    public static void PrintVisualTree(DependencyObject? parent, int indent = 0)
    {
        if (parent == null || parent is not FrameworkElement el) return;
        // 打印当前控件类型和名称（如果有）
        // string name = (parent as FrameworkElement)?.Name;
        // Console.WriteLine($"{new string('-', indent * 2)}- {parent.GetType().Name}{(string.IsNullOrEmpty(name) ? "" : $" (Name={name})")}");
        Console.WriteLine($"{new string('-', indent * 2)}- {parent}");

        var count = VisualTreeHelper.GetChildrenCount(el);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(el, i);
            PrintVisualTree(child, indent + 1);
        }
    }
    
    public static Dictionary<string, object?>? DumpToJson(DependencyObject? current, int indent = 0)
    {
        if (current == null || current is not FrameworkElement el) return null;
        Func<Dictionary<string, object?>, string, object?, Func<object?, bool>, object?> dictAddIfSatisfied = (d, k, v, condition) => !condition(v) ? null : d.TryAdd(k, v);
        var dict = new Dictionary<string, object?>();
        dict.Add("ID", el.GetHashCode());
        if (!string.IsNullOrEmpty(el.Name)) dict.Add(nameof(el.Name), el.Name);
        if (el.Tag != null) dict.Add(nameof(el.Tag), el.Tag);
        var type = el.GetType();
        dict.Add("Type", type.Name);
        dict.Add("TypeFullName", type.FullName);
        dict.Add(nameof(indent), indent);
        dictAddIfSatisfied(dict,  nameof(el.Visibility), $"{el.Visibility}", e => $"{e}" != "Visible" && $"{e}" != "");
        
        if (Window != null)
        {
            var coordinationRelativeToWindow = el.TransformToVisual(Window.Content);
            Windows.Foundation.Point offset = coordinationRelativeToWindow.TransformPoint(new Windows.Foundation.Point(0, 0));
            dict.Add("ClientX", offset.X);
            dict.Add("ClientY", offset.Y);
        }
        
        if (!double.IsNaN(el.Width)) dict.Add(nameof(el.Width), el.Width);
        if (!double.IsNaN(el.Height)) dict.Add(nameof(el.Height), el.Height);
        if (!double.IsNaN(el.ActualWidth)) dict.Add(nameof(el.ActualWidth), el.ActualWidth);
        if (!double.IsNaN(el.ActualHeight)) dict.Add(nameof(el.ActualHeight), el.ActualHeight);
        dictAddIfSatisfied(dict,  nameof(el.HorizontalAlignment), $"{el.HorizontalAlignment}", e => $"{e}" != "Stretch" && $"{e}" != "");
        dictAddIfSatisfied(dict,  nameof(el.VerticalAlignment), $"{el.VerticalAlignment}", e => $"{e}" != "Stretch" && $"{e}" != "");
        if (el.IsTabStop) dict.Add(nameof(el.IsTabStop), $"{el.IsTabStop}");
        if (el is Panel panel)
        {
            dictAddIfSatisfied(dict, nameof(panel.Background), $"{panel.Background}", e => !string.IsNullOrEmpty(e as string));
        }
        if (el is Control control)
        {
            dictAddIfSatisfied(dict, nameof(control.Background), $"{control.Background}", e => !string.IsNullOrEmpty(e as string));
            dict.Add(nameof(control.FontFamily), $"{control.FontFamily.Source}");
            dictAddIfSatisfied(dict, nameof(control.FontSize), $"{control.FontSize}", e => e as string != "14");
            dictAddIfSatisfied(dict, nameof(control.FontWeight), $"{control.FontWeight.Weight}", e => e as string != "400");
            dictAddIfSatisfied(dict, nameof(control.FontStyle), $"{control.FontStyle}", e => $"{e}" != "Normal");
            dictAddIfSatisfied(dict, nameof(control.FontStretch), $"{control.FontStretch}", e => $"{e}" != "Normal");
        }
        if (el is Border border)
        {
            dictAddIfSatisfied(dict, nameof(border.Background), $"{border.Background}", e => !string.IsNullOrEmpty(e as string));
            dict.Add(nameof(border.BorderBrush), $"{border.BorderBrush}");
            dict.Add(nameof(border.BorderThickness), $"{border.BorderThickness}");
            dict.Add(nameof(border.CornerRadius), $"{border.CornerRadius}");
            dict.Add(nameof(border.Padding), $"{border.Padding}");
        }

        var parent = VisualTreeHelper.GetParent(el);
        if (parent is Canvas)
        {
            dict.Add("X", Canvas.GetLeft(el));
            dict.Add("Y", Canvas.GetTop(el));
        }
        var count = VisualTreeHelper.GetChildrenCount(el);
        var list = new List<Dictionary<string, object?>?>();
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(el, i);
            list.Add(DumpToJson(child, indent + 1));
        }
        dict.Add("children", list);
        return dict;
    }

    public static UIElement? GetCursorUIElement()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(Window!);
        POINT p;
        LibraryUser32.GetCursorPos(out p);              // 获取屏幕坐标
        LibraryUser32.ScreenToClient(hwnd, ref p);      // 转换为窗口客户区坐标
        var root = (FrameworkElement)Window.Content;
        var point = new Windows.Foundation.Point(p.X, p.Y);
        var elements = VisualTreeHelper.FindElementsInHostCoordinates(point, root);
        Console.WriteLine($"Grab Elements {string.Join(",", elements.ToList().Select(e => $"{e.GetHashCode()}"))}");
        return elements.FirstOrDefault();
    }

    public static TreeViewNode? ConvertJsonToTree(object? json)
    {
        if (json == null) return null;
        var temp = json as Dictionary<string, object?>;
        var node = new TreeViewNode { Content = temp?["Type"] as string };
        if (temp?["children"] is not List<object?> { Count: > 0 } children) return node;
        foreach (var child in children) node.Children.Add(ConvertJsonToTree(child));
        return node;
    }

    public static void Traverse(
        Dictionary<string, object?>? current, 
        Dictionary<string, object?>? parent=null, 
        Action<Dictionary<string, object?>?, Dictionary<string, object?>?>? callback=null
    )
    {
        if (current == null) return;
        callback?.Invoke(current, parent);
        if (current?["children"] is not List<Dictionary<string, object?>?> { Count: > 0 } children) return;
        foreach (var child in children) Traverse(child, current, callback);
    }

    public static ScrollViewer FindScrollViewer(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ScrollViewer sv)
                return sv;
            var result = FindScrollViewer(child);
            if (result != null)
                return result;
        }
        return null;
    }

}