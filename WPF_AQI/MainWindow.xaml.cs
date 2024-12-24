using System.Net.Http;
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

        private async void GetAQIButton_Click(object sender, RoutedEventArgs e) //async非同步
        {
            ContentTextBox.Text = "抓取資料中...";

            string data = await FetchContentAsync(defaultURL);
            ContentTextBox.Text = data;

        }

        private async Task<string> FetchContentAsync(string url)
        {
            using (var client = new HttpClient()) 
            {
                client.Timeout = TimeSpan.FromSeconds(100);//設定請求的超時時間為100秒
                try 
                {
                    HttpResponseMessage response = await client.GetAsync(url);//發送GET請求
                    response.EnsureSuccessStatusCode();//EnsureSuccessStatusCode嘗試發送請求並確保回應成功
                    string responseBody = await response.Content.ReadAsStringAsync();//讀取回應內容
                    return responseBody;//回傳內容
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"Request exception:{e.Message}");//顯示錯誤訊息
                    return null;//回傳null
                }
            }
        }
    }
}