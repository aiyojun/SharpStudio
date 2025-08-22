namespace SharpStudioWinui3.Views
{
    /// <summary>
    /// A simple page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page
    {
        // int count = 0;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void OnCountClicked(object sender, RoutedEventArgs e)
        {
            // txtCount.Text = $"Current count: {count++}";
            Console.WriteLine($"[OnClicked] Window : {Window.Current}");
            Console.WriteLine($"[OnClicked] Theme  : {RequestedTheme}");
            Console.WriteLine($"[OnClicked] Theme  : {Application.Current.RequestedTheme}(Application)");
        }
    }
}
