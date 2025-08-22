using System.Windows;

namespace SharpStudioWpf;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        // Console.WriteLine($"[0] Background {Application.Current.Resources["App.Brushes.Background"]}");
        ThemeManager.Instance.ApplyDarkTheme();
        // Console.WriteLine($"[1] Background {Application.Current.Resources["App.Brushes.Background"]}");
        
        InitializeComponent();
        
        LocaleSupport.Instance.PropertyChanged += (_, _) =>
        {
            LocaleSupport.ResManager = LocaleSupport.CurrentCulture.Name == "en-US" ? SharpStudioWpf.en_US.Strings.ResourceManager : SharpStudioWpf.zh_CN.Strings.ResourceManager;
        };
        // TitleBar.Background = PlatformTools.IsSystemInDarkMode()
        //     ? new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22)) 
        //     : Brushes.White; 

        // var chrome = new WindowChrome
        // {
        //     CaptionHeight = 0, // 顶部标题栏高度为 0
        //     ResizeBorderThickness = new Thickness(8), // 可调边框厚度
        //     GlassFrameThickness = new Thickness(0),
        //     CornerRadius = new CornerRadius(0),
        //     UseAeroCaptionButtons = false
        // };
        // WindowChrome.SetWindowChrome(this, chrome);

    }

    public void OnToggleLanguageClicked(object sender, RoutedEventArgs e)
    {
        // LocaleSupport.ChangeCulture("zh-CN");
        LocaleSupport.ChangeCulture(LocaleSupport.CurrentCulture.Name == "en-US" ? "zh-CN" : "en-US");
        // Console.WriteLine("CurrentCulture Name : " + LocaleSupport.CurrentCulture.Name);
        // Console.WriteLine("CurrentCulture      : " + LocaleSupport.CurrentCulture);
        // Console.WriteLine("ApplicationName     : " + LocaleSupport.ResManager.GetString("ApplicationName", new CultureInfo("en-US")));
        // Console.WriteLine("ResManager          : " + LocaleSupport.ResManager);
        // Console.WriteLine("Instance            : " + LocaleSupport.Instance);
        // Console.WriteLine("ApplicationName     : " + LocaleSupport.Instance["ApplicationName"]);
    }
    
}