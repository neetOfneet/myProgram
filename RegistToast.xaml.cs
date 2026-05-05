using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TaskScheduler;

namespace WpfApp1.Alarm
{
    /// <summary>
    /// RegistToast.xaml の相互作用ロジック
    /// </summary>
    public partial class RegistToast : Window
    {
        public RegistToast()
        {
            InitializeComponent();
            SetDate();
        }
        public void SetDate()
        {
            var now = DateTime.Now;
            this.textbox_year.Text = now.Year.ToString();
            this.textbox_month.Text = now.Month.ToString();
            this.textbox_day.Text = now.Day.ToString();
            this.textbox_time_hh.Text = now.Hour.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool checkResult = CheckData();
            if (checkResult)
            {
                Environment.Exit(0);
            }
        }
        private void SendData(bool daily)
        {
            try
            {
                string foldername = Year.ToString() + Month.ToString() + Day.ToString() + Time.ToString();
                string dir = @"C:\programs\WpfApp1\Alarm\";
                System.IO.File.AppendAllText(System.IO.Path.Combine(dir, "toastData.TXT"), foldername + "," + ContentText + "," + daily + "\n");
            }
            catch(Exception ex)
            {
                MessageBox.Show("例外:" + ex.Message);
            }
        }
        private bool CheckData()
        {
            try
            {
                var now = DateTime.Now;
                // 判定
                if (Year != -1 && Year >= now.Year
                    && Month != -1 && (Month>=now.Month || Year>now.Year)
                    && Day !=-1 && (Day>=now.Day || Month> now.Month)
                    && Time != -1)
                {
                    string sdate = $"{textbox_year.Text}-{textbox_month.Text}-{textbox_day.Text}T{textbox_time_hh.Text}:{textbox_time_mm.Text}:00";
                    string edate = $"{textbox_year.Text}-{textbox_month.Text}-{textbox_day.Text}T{textbox_time_hh.Text}:{textbox_time_mm.Text}:50";

                    if (MessageBox.Show(sdate + ":これで登録するよ", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        TaskMake(sdate, edate);
                        SendData((bool)check_DailyOrNot.IsChecked);
                        return true;
                    }
                    return false;

                }
                else
                {
                    MessageBox.Show("日時が正しくありません");
                    return false;
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("例外:" + ex.Message);
                return false;
            }
        }
        public void TaskMake(string sdate, string edate)
        {
            ITaskService taskService = null;
            ITaskFolder rootfolder = null;
            try
            {
                // TaskServiceを生成して接続する
                // 接続時のUser,Domain,Passwordを必要に応じて設定する
                taskService = new TaskScheduler.TaskScheduler();
                taskService.Connect(null, null, null, null);

                // タスクスケジューラのフォルダを指定する
                rootfolder = taskService.GetFolder("\\");
                var path = this.textBox_Content.Text;

                // 新規登録用のタスクを定義
                ITaskDefinition taskDefinition = taskService.NewTask(0);

                // 設定に使うもろもろ
                IRegistrationInfo registrationInfo = taskDefinition.RegistrationInfo;
                IActionCollection actionCollection = taskDefinition.Actions;
                IExecAction execAction = (IExecAction)actionCollection.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                ITriggerCollection triggerCollection = taskDefinition.Triggers;
                ITaskSettings taskSettings = taskDefinition.Settings;
                IPrincipal principal = taskDefinition.Principal;

                // 実行タイミングについて
                if(check_DailyOrNot.IsChecked == true)
                {
                    IDailyTrigger dailyTrigger = (IDailyTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY);
                    dailyTrigger.Enabled = true;

                    // 毎日
                    dailyTrigger.DaysInterval = 1;
                    // 時間指定必須
                    dailyTrigger.StartBoundary = sdate;
                }
                else
                {
                    ITimeTrigger timeTrigger = (ITimeTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME); // 規定時間で実行
                    timeTrigger.Enabled = true;
                    // トリガー条件
                    timeTrigger.StartBoundary = sdate;
                    timeTrigger.EndBoundary = edate;
                    // timeTrigger.Repetition.Interval = "PT1M"; // 1分間隔で実行
                    taskSettings.DeleteExpiredTaskAfter = "PT5M";
                }
                // IBootTrigger eventTrigger = (IBootTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_BOOT);
                // ILogonTrigger logonTrigger = (ILogonTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);

                // タスクの作成者と概要
                registrationInfo.Author = "Admin";
                registrationInfo.Description = "タスクスケジューラへの登録がしたい";

                // タスクの実行時に使うユーザーアカウント
                // principal.UserId = $@"{Environment.UserDomainName}\{Environment.UserName}"; // SSL\snumata　で実行
                // principal.GroupId = "S-1-5-32-545"; // Usersで実行
                principal.GroupId = "Users";

                // ユーザがログオンしているかどうかにかかわらず実行
                // 最上位特権で実行する
                principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_S4U;
                principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;

                // ログオン後一時間してから
                //logonTrigger.Delay "PT1H";

                // トリガー有効化(スタートアップ時に実行)
                // eventTrigger.Enabled = true;

                // 以下、各設定項目

                // [新しいインスタンス開始しない]
                taskSettings.MultipleInstances = _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW;
                // [コンピュータをAC電源で使用している場合のみタスクを開始する]
                taskSettings.DisallowStartIfOnBatteries = false;
                // [コンピュータの電源がバッテリに切り替わった場合は停止する]
                taskSettings.StopIfGoingOnBatteries = false;
                // [要求時に実行中のタスクが終了しない場合、タスクを強制的に終了する]
                taskSettings.AllowHardTerminate = true;
                // [スケジュールされた時刻にタスクを開始できなかった場合、すぐタスクを実行する]
                taskSettings.StartWhenAvailable = true;
                // [次のネットワーク接続が使用可能な場合のみタスクを開始する]
                taskSettings.RunOnlyIfNetworkAvailable = false;
                // [コンピュータがアイドル状態でなくなった場合は停止する]
                taskSettings.IdleSettings.StopOnIdleEnd = false;
                // [再びアイドル状態になったら再開する]
                taskSettings.IdleSettings.RestartOnIdle = false;
                // [タスクを要求時に実行する]
                taskSettings.AllowDemandStart = true; // falseにすると手動実行できなくなる(定期実行のみ)

                //状態＝準備完了/無効
                taskSettings.Enabled = true;
                // [非表示になっているタスクを表示]をチェックするまで非表示にするかどうか
                taskSettings.Hidden = false;
                // 次の時間アイドル状態である場合のみタスクを開始する
                taskSettings.RunOnlyIfIdle = false;
                // タスクを実行するためにスリープを解除する
                taskSettings.WakeToRun = false;
                // タスクを停止するまでの時間(0sでチェック外れる)
                taskSettings.ExecutionTimeLimit = "PT0S";   // n秒後に自動で閉じるように設定可

                // タスクの優先度レベル(既定値は7) 0(リアルタイム)や1(高)にするとWindowsシステムの動きが遅くなる場合がある
                taskSettings.Priority = 3;

                // タスクが失敗した場合の再起動の間隔
                taskSettings.RestartInterval = "PT1M";
                // 再起動試行の最大数
                taskSettings.RestartCount = 3;

                // 動作確認用のバッチファイルを実行するように設定
                //execAction.Path = $@"{AppDomain.CurrentDomain.BaseDirectory}test.bat";
                execAction.Path = $@"C:\programs\WpfApp1\bin\Release\net8.0-windows\WpfApp1.exe"; // 実行するファイル

                // タスク登録(要管理者権限)
                rootfolder.RegisterTaskDefinition(
                    path,
                    taskDefinition,
                    (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                    null,
                    null,
                    _TASK_LOGON_TYPE.TASK_LOGON_NONE,
                    null
                    );
                ContentText = this.textBox_Content.Text;
                MessageBox.Show("タスクを登録したよ！");
            }
            finally
            {
                if(taskService != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(taskService);
                }
                if (rootfolder != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rootfolder);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            textbox_year.Text = (int.Parse(textbox_year.Text) + 1).ToString();
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (textbox_year.Text != DateTime.Now.Year.ToString())
            {
                textbox_year.Text = (int.Parse(textbox_year.Text) - 1).ToString();
            }
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if(textbox_month.Text != "12")
            {
                textbox_month.Text = (int.Parse(textbox_month.Text) + 1).ToString();
            }
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (textbox_month.Text != "1")
            {
                textbox_month.Text = (int.Parse(textbox_month.Text) - 1).ToString();
            }
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (textbox_day.Text != "31")
            {
                textbox_day.Text = (int.Parse(textbox_day.Text) + 1).ToString();
            }
        }
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (textbox_day.Text != "1")
            {
                textbox_day.Text = (int.Parse(textbox_day.Text) - 1).ToString();
            }
        }
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (textbox_time_hh.Text != "24")
            {
                textbox_time_hh.Text = (int.Parse(textbox_time_hh.Text) + 1).ToString();
            }
        }
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (int.Parse(textbox_time_hh.Text) != 0)
            {
                textbox_time_hh.Text = (int.Parse(textbox_time_hh.Text) - 1).ToString();
            }
        }
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            if (textbox_time_mm.Text != "55")
            {
                textbox_time_mm.Text = (int.Parse(textbox_time_mm.Text) + 5).ToString();
            }
        }
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (int.Parse(textbox_time_mm.Text) != 0)
            {
                textbox_time_mm.Text = (int.Parse(textbox_time_mm.Text) - 5).ToString();
            }
        }

        public string ContentText { get; set; }
        private int Year
        {
            set { if (int.TryParse(textbox_year.Text, out int year)) { year = value; } }
            get { if (int.TryParse(textbox_year.Text, out int year)) return year; else return -1; }
        }
        private int Month
        {
            set { if (int.TryParse(textbox_month.Text, out int month)) { month = value; } }
            get { if (int.TryParse(textbox_month.Text, out int month)) return month; else return -1; }
        }
        private int Day
        {
            set { if (int.TryParse(textbox_day.Text, out int day)) { day = value; } }
            get { if (int.TryParse(textbox_day.Text, out int day)) return day; else return -1; }
        }
        private int Time
        {
            set { if (int.TryParse(textbox_time_hh.Text, out int time)) { time = value; } }
            get { if (int.TryParse(textbox_time_hh.Text, out int time)) return time; else return -1; }
        }
    }
}
