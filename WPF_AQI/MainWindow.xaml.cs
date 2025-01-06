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
        List<Record> records = new List<Record>();//所有資料
        List<Record> selectedRecords = new List<Record>();//選擇的資料
        //SeriesCollection序列的集合
        SeriesCollection seriesCollection = new SeriesCollection();
        public MainWindow()
        {
            InitializeComponent();
            UrlTextBox.Text = defaultURL;
        }

        private async void GetAQIButton_Click(object sender, RoutedEventArgs e) //async非同步
        {
            ContentTextBox.Text = "抓取資料中...";

            string data = await FetchContentAsync(defaultURL);//await等待非同步方法完成
            ContentTextBox.Text = data;
            aqiData = JsonSerializer.Deserialize<AQIData>(data);//將JSON字串反序列化為AQIData物件
            fields = aqiData.fields.ToList();//將fields轉換為List<Field>
            records = aqiData.records.ToList();
            selectedRecords = records;//選擇的資料為所有資料

            statusTextBlock.Text = $"共有{records.Count}筆資料";//顯示資料筆數
            DisplayAQIData();
        }

        private void DisplayAQIData()
        {
            RecordDataGrid.ItemsSource = records;//將資料繫結到DataGrid

            Record record = records[0];//分析第一筆資料 記錄欄位名稱

            //假設數值可轉成浮點數 則顯示欄位CheckBox

            foreach (Field field in fields)
            {
               var propertyInfo = record.GetType().GetProperty(field.id);//取得field的id
                if (propertyInfo != null)//propertyInfo有值
                {
                   var value = propertyInfo.GetValue(record) as string;//取得值
                    if (double.TryParse(value, out double v))//如果value可轉成浮點數
                    {
                        CheckBox cb = new CheckBox //建立CheckBox並設定相關的屬性
                        {
                            Content = field.info.label,//顯示欄位名稱
                            Tag = field.id,//標籤
                            Margin = new Thickness(3),//設定邊距上下左右
                            FontSize = 14,
                            FontFamily = new System.Windows.Media.FontFamily("標楷體"),
                            FontWeight = FontWeights.Bold,
                            Width = 220
                        };
                        //用來產生圖表的方法
                        cb.Checked += UpdataChart;
                        cb.Unchecked += UpdataChart;
                        DataWrapPanel.Children.Add(cb);//將CheckBox加入DataWrapPanel
                    }
                }
            }
        }

        private void UpdataChart(object sender, RoutedEventArgs e)//UpdataChart用來產生圖表
        {
            seriesCollection.Clear();//清除所有的Series

            foreach (CheckBox cb in DataWrapPanel.Children)
            {
                if (cb.IsChecked == true)//如果CheckBox被勾選
                {
                    List<string> labels = new List<string>();//記下Label
                    string tag = cb.Tag.ToString();//取得指標
                    ColumnSeries columnSeries = new ColumnSeries();//ColumnSeries 行的資料
                    ChartValues<double> values = new ChartValues<double>();//顯示圖的內容

                    foreach (Record r in selectedRecords)//選擇的資料
                    {
                        var propertyInfo = r.GetType().GetProperty(tag);//取得tag的值
                        if (propertyInfo != null)
                        {
                            var value = propertyInfo.GetValue(r) as string;
                            if (double.TryParse(value, out double v))
                            {
                                labels.Add(r.sitename);//記下站名
                                values.Add(v);//記下值
                            }
                        }
                    }
                    columnSeries.Values = values;
                    columnSeries.Title = tag;
                    columnSeries.LabelPoint = point => $"{labels[(int)point.X]}:{point.Y.ToString()}";//$"" 插補字串
                    seriesCollection.Add(columnSeries);//將columnSeries加入seriesCollection
                }
            }
            AQIChart.Series = seriesCollection;//將seriesCollection繫結到AQIChart
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
            e.Row.Header = (e.Row.GetIndex()+1).ToString();//載入列時 每次+1顯示行號
        }

        private void RecordDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRecords = RecordDataGrid.SelectedItems.Cast<Record>().ToList();//選擇的資料為RecordDataGrid的選擇項目
            statusTextBlock.Text = $"共選擇{selectedRecords.Count}筆資料";//顯示選擇的資料筆數
            UpdataChart(null, null);//更新圖表
        }
    }
}