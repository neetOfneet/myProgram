using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ContentText = "";
        public MainWindow()
        {
            InitializeComponent();
            SetWindow();
            ShowToast();
        }
        private void SetWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
        }
        public void ShowToast()
        {
            try
            {
                string path = @"C:\programs\WpfApp1\Alarm\toastData.TXT";
                if (File.Exists(path))
                {
                    var now = DateTime.Now;
                    string month = "", day = "", hour = "", minute = "", nextmonth = "", tomorrow = "";
                    this.ContentText = "";
                    int count = 0;
                    string[] lines = System.IO.File.ReadAllLines(path);
                    var alarmList = new List<string>();
                    alarmList.AddRange(lines);
                    alarmList.RemoveAll(s => s == "");
                    month = ConvertTime(now.Month, false);
                    day = ConvertTime(now.Day, false);
                    hour = ConvertTime(now.Hour, true);
                    minute = ConvertTime(now.Minute, true);
                    nextmonth = ConvertTime(now.AddDays(1).Month, false);
                    tomorrow = ConvertTime(now.AddDays(1).Day, false);

                    while (count < alarmList.Count)
                    {
                        if (alarmList[count].Contains(","))
                        {
                            string[] letters = alarmList[count].Split(',');
                            // 完全一致の場合
                            if (letters[0] == now.Year.ToString() + month + day + hour + minute)
                            {
                                // テキストをセット
                                this.ContentText = letters[1];
                                alarmList.RemoveAt(count);
                                // 1回のみ実行
                                if (letters[2] != "False")
                                {
                                    string newLine = now.Year.ToString() + nextmonth + tomorrow + hour + minute + "," + letters[1] + "," + letters[2];
                                    // 処理済みを明日の日付に差し替え
                                    alarmList.Add(newLine);
                                }
                                lines = alarmList.ToArray();
                                System.IO.File.WriteAllLines(path, lines);
                                break;
                            }
                            else if ((letters[0].Substring(letters[0].Length - 4) == hour + minute) && letters[2] == "True")
                            {
                                string newLine = now.Year.ToString() + nextmonth + tomorrow + hour + minute + "," + letters[1] + "," + letters[2];
                                alarmList.RemoveAt(count);
                                alarmList.Add(newLine);
                                lines = alarmList.ToArray();
                                System.IO.File.WriteAllLines(path, lines);
                                this.ContentText = letters[1];
                            }
                            // 最後まで該当なしの場合
                            if (count == alarmList.Count - 1 && this.ContentText == "")
                            {
                                this.ContentText = "見つからなかったよん";
                            }
                        }
                        count++;
                    }
                }
                this.ContentTextBlock.Text = this.ContentText;
                this.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return;
        }

        private string ConvertTime(int datetime,bool isTime)
        {
            // 日付の場合
            if (!isTime)
            {
                if(datetime < 10)
                {
                    return datetime.ToString().Replace("0", "");
                }
            }
            else // 時間の場合:00分を除外
            {
                if(0<datetime && datetime < 10)
                {
                    return datetime.ToString().Replace("0", "");
                }else if(datetime == 0)
                {
                    return ("00");
                }
            }
            return datetime.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}