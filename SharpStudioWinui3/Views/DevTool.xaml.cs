using Windows.Graphics;

namespace SharpStudioWinui3.Views;

public sealed partial class DevTool : Window
{
    public DevTool()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.Title = "DevTools";
        AppWindow.Resize(new SizeInt32(800, (int)(ApplicationHelper.Window?.Content as FrameworkElement)!.ActualHeight));
    }
    
    private Dictionary<int, Dictionary<string, object?>> _jsonMapper = new();
    private Dictionary<int, TreeViewNode> _nodeMapper = new();
    private Dictionary<TreeViewNode, int> _reverseMapper = new();

    public void Load()
    {
        Tree.RootNodes.Clear();
        var root = ApplicationHelper.DumpToJson(ApplicationHelper.Window?.Content as FrameworkElement);
        // var jsonMapper = new Dictionary<int, Dictionary<string, object?>>();
        // var nodeMapper = new Dictionary<int, TreeViewNode>();
        // var reverseMapper = new Dictionary<TreeViewNode, int>();
        _jsonMapper.Clear();
        _nodeMapper.Clear();
        _reverseMapper.Clear();
        ApplicationHelper.Traverse(root, null, (jCurr, jParent) =>
        {
            if (jCurr == null) return;
            _jsonMapper.Add((int)jCurr["ID"], jCurr);
            Console.WriteLine($"-> node {(int)jCurr["ID"]}");
            var nCurr = new TreeViewNode
            {
                Content = $"{jCurr["Type"]}" + (jCurr.ContainsKey("Name") ? $" ({jCurr["Name"]})" : ""),
            };
            _nodeMapper.Add((int)jCurr["ID"], nCurr);
            _reverseMapper.Add(nCurr, (int)jCurr["ID"]);
            if (jParent is null) return;
            var nParent = _nodeMapper[(int)jParent["ID"]];
            nParent.Children.Add(nCurr);
        });
        Tree.RootNodes.Add(_nodeMapper[(int)root["ID"]]);
        // Tree.RootNodes.Add(ApplicationHelper.ConvertJsonToTree());
    }

    private void OnFreshTree(object sender, RoutedEventArgs e)
    {
        Load();
    }

    private void OnItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        var item = args.InvokedItem as TreeViewNode;
        var properties = _jsonMapper[_reverseMapper[item]];
        ShowProperties(properties);
    }

    private void ShowProperties(Dictionary<string, object?>? obj)
    {
        Properties.Children.Clear();
        if (obj == null) return;

        obj.ToList().Where(e => e.Key != "children").ToList().ForEach(kv =>
        {
            var value = kv.Key == "ID" ? Convert.ToHexString(BitConverter.GetBytes((int)kv.Value)) : kv.Value;
            var textBlock = new TextBlock { Text = $"{kv.Key} : {value}" };
            Properties.Children.Add(textBlock);
        });
    }
}