using LiveCharts;
using LiveCharts.Wpf;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace WPF_AQI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string defaultURL = "https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=e8dd42e6-9b8b-43f8-991e-b3dee723a52d&limit=1000&sort=ImportDate%20desc&format=JSON";
        AQIData aqiData = new AQIData();
        List<Field> fields = new List<Field>();
        List<Record> records = new List<Record>();
        List<Record> selectedRecords = new List<Record>();

        SeriesCollection seriesCollection = new SeriesCollection();
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
            aqiData = JsonSerializer.Deserialize<AQIData>(data);
            fields = aqiData.fields.ToList();
            records = aqiData.records.ToList();
            selectedRecords = records;

            statusTextBlock.Text = $"共有{records.Count}筆資料";
            DisplayAQIData();
        }

        private void DisplayAQIData()
        {
            RecordDataGrid.ItemsSource = records;

            Record record = records[0];

            foreach (Field field in fields)
            {
               var propertyInfo = record.GetType().GetProperty(field.id);
                if (propertyInfo != null)
                {
                   var value = propertyInfo.GetValue(record) as string;
                   if(double.TryParse(value, out double v))
                    {
                        CheckBox cb = new CheckBox 
                        {
                            Content = field.info.label,
                            Tag = field.id,
                            Margin = new Thickness(3),
                            FontSize = 14,
                            FontFamily = new System.Windows.Media.FontFamily("標楷體"),
                            FontWeight = FontWeights.Bold,
                            Width = 220
                        };
                        cb.Checked += UpdataChart;
                        cb.Unchecked += UpdataChart;
                        DataWrapPanel.Children.Add(cb);
                    }
                }
            }
        }

        private void UpdataChart(object sender, RoutedEventArgs e)
        {
            seriesCollection.Clear();

            foreach (CheckBox cb in DataWrapPanel.Children)
            {
                if (cb.IsChecked == true)
                {
                    List<string> labels = new List<string>();
                    string tag = cb.Tag.ToString();
                    ColumnSeries columnSeries = new ColumnSeries();
                    ChartValues<double> values = new ChartValues<double>();//顯示圖的內容

                    foreach (Record r in selectedRecords)
                    {
                        var propertyInfo = r.GetType().GetProperty(tag);
                        if (propertyInfo != null)
                        {
                            var value = propertyInfo.GetValue(r) as string;
                            if (double.TryParse(value, out double v))
                            {
                                labels.Add(r.sitename);
                                values.Add(v);
                            }
                        }
                    }
                    columnSeries.Values = values;
                    columnSeries.Title = tag;
                    columnSeries.LabelPoint = point => $"{labels[(int)point.X]}:{point.Y.ToString()}";//$"" 插補字串
                    seriesCollection.Add(columnSeries);
                }
            }
            AQIChart.Series = seriesCollection;
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

        private void RecordDataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()+1).ToString();
        }

        private void RecordDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRecords = RecordDataGrid.SelectedItems.Cast<Record>().ToList();
            statusTextBlock.Text = $"共選擇{selectedRecords.Count}筆資料";
            UpdataChart(null, null);
        }
    }
}