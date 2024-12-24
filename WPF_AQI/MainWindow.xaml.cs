using System.Windows;

namespace WPF_AQI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string defaultURL = "https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=e8dd42e6-9b8b-43f8-991e-b3dee723a52d&limit=1000&sort=ImportDate%20desc&format=JSON";
        public MainWindow()
        {
            InitializeComponent();
            UrlTextBox.Text = defaultURL;
        }

        private async void GetAQIButton_Click(object sender, RoutedEventArgs e)
        {
            ContentTextBox.Text = "抓取資料中...";
        }
    }
}