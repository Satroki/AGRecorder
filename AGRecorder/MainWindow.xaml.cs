using Microsoft.Win32;
using RadioRecorder.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Xml;

using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace RadioRecorder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 字段
        private String[] days = { "日曜日", "月曜日", "火曜日", "水曜日", "木曜日", "金曜日", "土曜日" };
        private String[] HiBiKiTabs ={"全部","月曜日","火曜日","水曜日","木曜日","金曜日","土·日曜日",
                       "Anime","Game","Culture","Archive","新番組"};
        private ObservableCollection<RecordTask> taskList;
        private ObservableCollection<Bangumi> bgmList;
        private Dictionary<int, RecordTask> runningTask;
        private bool taskChanged = false;
        private bool bgmChanged = false;
        private bool cmdChanged = false;
        private DispatcherTimer timer;
        private ObservableCollection<CMDArgs> cmdList;
        private NotifyIcon notifyIcon;
        private int pre = 120, fol = 60;
        private Log logWin;
        private FlowDocument log;
        private Paragraph pgh;
        #endregion

        #region 基础方法
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height = Settings.Default.窗体高;
            this.Width = Settings.Default.窗体宽;

            var taskxml = Settings.Default.TaskPath;
            if (File.Exists(taskxml))
            {
                var xs = new XmlSerializer(typeof(RecordTask[]));
                using (var fs = new FileStream(taskxml, FileMode.Open))
                    taskList = new ObservableCollection<RecordTask>(
                        ((RecordTask[])xs.Deserialize(fs)).OrderBy(t => t.Day).ThenBy(t => t.Time)
                        );
            }
            else
                taskList = new ObservableCollection<RecordTask>();
            taskList.CollectionChanged += (ss, ee) => { taskChanged = true; };

            LV_Task.ItemsSource = taskList;

            CB_DOW.ItemsSource = days;
            var now = DateTime.UtcNow.AddHours(9);
            if (now.Hour < 6)
                now = now.AddDays(-1);
            CB_DOW.SelectedIndex = (int)now.DayOfWeek;
            TB_ST.Text = now.ToString("HH:mm");
            TB_Dir.Text = Settings.Default.默认路径;
            TB_NF.Text = Settings.Default.默认格式;
            runningTask = new Dictionary<int, RecordTask>();
            L_Now.Content = "现在时间：" + now.ToString("HH:mm");

            CB_HKBFilter.ItemsSource = HiBiKiTabs;
            CB_HKBFilter.SelectedIndex = 0;
            TB_HBKDir.Text = Settings.Default.默认路径;
            TB_HBKFormat.Text = Settings.Default.HBKFormat;

            setNotifyIcon();
            loadSettings();
            loadLog();
        }

        private void setNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Text = "已停止";
            notifyIcon.BalloonTipText = "录制开始……";
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += (ss, ee) =>
            {
                if (ee.Button == System.Windows.Forms.MouseButtons.Left)
                    if (this.Visibility == Visibility.Visible)
                        this.Visibility = Visibility.Hidden;
                    else
                    {
                        this.Visibility = Visibility.Visible;
                        this.Show();
                        this.WindowState = WindowState.Normal;
                    }
            };
            var cm = new System.Windows.Forms.ContextMenu();
            cm.MenuItems.Add("开始任务", (s, e) => B_开始_Click(null, null));
            cm.MenuItems.Add("停止", (s, e) => B_停止_Click(null, null));
            cm.MenuItems.Add("退出", (s, e) => { B_停止_Click(null, null); this.Close(); });
            notifyIcon.ContextMenu = cm;
            this.Closed += (ss, ee) => notifyIcon.Dispose();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.窗体宽 = this.ActualWidth;
            Settings.Default.窗体高 = this.ActualHeight;
            Settings.Default.Save();

            if (bgmChanged)
                saveList(Settings.Default.BgmPath, typeof(Bangumi[]), bgmList);
            if (cmdChanged)
                saveList(Settings.Default.CmdPath, typeof(CMDArgs[]), cmdList);
            if (taskChanged)
                saveList(Settings.Default.TaskPath, typeof(RecordTask[]), taskList);

            Application.Current.Shutdown();
        }

        private void saveList<T>(string path, Type type, ObservableCollection<T> list)
        {
            if (list.Count > 0)
            {
                var xs = new XmlSerializer(type);
                using (var fs = new FileStream(path, FileMode.Create))
                    xs.Serialize(fs, list.ToArray());
            }
            else
                if (File.Exists(path))
                File.Delete(path);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabC.SelectedIndex == 1 && bgmList == null)
            {
                var path = Settings.Default.BgmPath;
                if (!File.Exists(path))
                    return;
                var xs = new XmlSerializer(typeof(Bangumi[]));
                using (var fs = new FileStream(path, FileMode.Open))
                    bgmList = new ObservableCollection<Bangumi>((Bangumi[])xs.Deserialize(fs));
                bgmList.CollectionChanged += (ss, ee) => bgmChanged = true;
                LV_BGM.ItemsSource = bgmList;
            }
            else if (TabC.SelectedIndex == 3 && cmdList == null)
            {
                var path = Settings.Default.CmdPath;
                if (File.Exists(path))
                {
                    var xs = new XmlSerializer(typeof(CMDArgs[]));
                    using (var fs = new FileStream(path, FileMode.Open))
                        cmdList = new ObservableCollection<CMDArgs>((CMDArgs[])xs.Deserialize(fs));
                }
                else
                    cmdList = new ObservableCollection<CMDArgs>();
                cmdList.CollectionChanged += (ss, ee) => cmdChanged = true;
                LB_Cmd.ItemsSource = cmdList;
            }
        }
        #endregion

        #region 番组列表
        private List<Bangumi> regex(String str)
        {
            try
            {
                var tsi = str.IndexOf("</thead>");
                var tei = str.IndexOf("</table>");
                str = str.Substring(tsi, tei - tsi + 8);

                var bgms = Regex.Matches(str, "<td([\\s\\S]*?)</td>");
                var flags = new List<bool>(new bool[45 * 7]);
                var list = new List<Bangumi>();
                foreach (Match bgm in bgms)
                {
                    var value = bgm.Value;
                    var index = flags.FindIndex(f => f == false);
                    var day = (index + 1) % 7;
                    var duration = 30;
                    flags[index] = true;

                    var match = Regex.Match(value, "rowspan=\"\\d{1,6}\"");
                    if (!string.IsNullOrEmpty(match.Value))
                    {
                        var span = int.Parse(match.Value.Substring(9, 1));
                        for (int i = 1; i < span; i++)
                        {
                            flags[index + 7 * i] = true;
                        }
                        duration *= span;
                    }

                    string type = "";
                    if (value.Contains("m.gif"))
                        type += "M";
                    if (!value.Contains("bg"))
                        type += "R";
                    else if (value.Contains("bg-l"))
                        type += "L";

                    int si = 0, ei = 0;
                    si = value.IndexOf("\"time\">") + 8;
                    ei = value.IndexOf("<", si);
                    var time = DateTime.Parse(value.Substring(si, ei - si).Trim());

                    si = value.IndexOf("\"title-p\">") + 11;
                    ei = value.IndexOf("</div>", si);
                    var title = value.Substring(si, ei - si);
                    title = Regex.Replace(title, "<.*?>", "").Trim();

                    si = value.IndexOf("\"rp\">") + 6;
                    ei = value.IndexOf("</div>", si);
                    var p = value.Substring(si, ei - si);
                    p = Regex.Replace(p, "(<!--.*?-->)|(<.*?>)", "").Trim();
                    Bangumi b = new Bangumi()
                    {
                        Title = title,
                        Day = day,
                        Personality = p,
                        Hour = time.Hour,
                        Minute = time.Minute,
                        Type = type,
                        Duration = duration
                    };
                    list.Add(b);
                }
                return list.Where(b => !b.Title.Contains("放送休止"))
                    //.OrderBy(b => b.Day).ThenBy(b => b.Hour)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private async void B_UpdateBGMList_Click(object sender, RoutedEventArgs e)
        {
            List<Bangumi> list = null;
            L_AGPgs.Content = "正在获取列表……";
            await Task.Run(() =>
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        var url = Settings.Default.番组表;
                        String page = client.DownloadString(url);
                        list = regex(page);
                    }
                    this.Dispatcher.Invoke(() =>
                    {

                        bgmList = new ObservableCollection<Bangumi>(list);
                        bgmChanged = true;
                        LV_BGM.ItemsSource = bgmList;
                    });
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
                }
                finally
                {
                    this.Dispatcher.Invoke(() => L_AGPgs.Content = "就绪");
                }
            });
        }

        private void TB_Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = TB_Filter.Text;
            bool r = (bool)CB_R.IsChecked;
            LV_BGM.Items.Filter = o =>
            {
                var b = o as Bangumi;
                int xq = -1;
                if (int.TryParse(text, out xq))
                    if (xq > -1 && xq < 7 && b.Day == xq)
                        if (r)
                            if (b.Type.Contains('R'))
                                return false;
                            else return true;
                        else return true;
                if (!b.Title.Contains(text) && !b.Personality.Contains(text))
                    return false;
                if (b.Type.Contains('R') && r)
                    return false;
                return true;
            };
        }

        private void B_Filter_Click(object sender, RoutedEventArgs e)
        {
            TB_Filter_TextChanged(null, null);
        }

        private void LV_BGM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LV_BGM.SelectedItem != null)
            {
                var b = LV_BGM.SelectedItem as Bangumi;
                TB_Title.Text = b.Title;
                TB_Personality.Text = b.Personality;
            }
        }

        private void GV_BGM_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader)
            {
                var clickedColumn = (e.OriginalSource as GridViewColumnHeader).Column;
                if (clickedColumn != null)
                {
                    var binding = clickedColumn.DisplayMemberBinding as System.Windows.Data.Binding;
                    string bindingProperty = binding.Path.Path;
                    var sdc = LV_BGM.Items.SortDescriptions;
                    var sortDirection = ListSortDirection.Ascending;
                    if (sdc.Count > 0)
                    {
                        SortDescription sd = sdc[0];
                        sortDirection = (ListSortDirection)((((int)sd.Direction) + 1) % 2);
                        sdc.Clear();
                    }
                    sdc.Add(new SortDescription(bindingProperty, sortDirection));
                }
            }
        }

        private void B_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (LV_BGM.SelectedItem != null)
            {
                var b = LV_BGM.SelectedItem as Bangumi;
                b.Title = TB_Title.Text;
                b.Personality = TB_Personality.Text;
                bgmChanged = true;
            }
        }

        private void B_AddToTask_Click(object sender, RoutedEventArgs e)
        {
            if (LV_BGM.SelectedItem != null)
            {
                var b = LV_BGM.SelectedItem as Bangumi;
                var t = new RecordTask();
                t.SetValue(b);
                t.NameFormat = Settings.Default.默认格式;
                t.Dir = Settings.Default.默认路径;
                if (!taskList.Contains(t))
                {
                    taskList.Add(t);
                    MessageBox.Show("已添加");
                }
            }
        }
        #endregion

        #region 任务
        private void LV_Task_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LV_Task.SelectedItem != null)
            {
                var t = LV_Task.SelectedItem as RecordTask;
                TB_P_T.Text = t.Personality;
                CB_DOW.SelectedIndex = t.Day;
                TB_ST.Text = t.Time;
                TB_NF.Text = t.NameFormat;
                TB_Dir.Text = t.Dir;
                TB_Du.Text = t.Duration.ToString();
                TB_标题.Text = t.Title;
            }
        }

        private void GV_Task_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader)
            {
                var clickedColumn = (e.OriginalSource as GridViewColumnHeader).Column;
                if (clickedColumn != null)
                {
                    var binding = clickedColumn.DisplayMemberBinding as System.Windows.Data.Binding;
                    string bindingProperty = binding.Path.Path;
                    var sdc = LV_Task.Items.SortDescriptions;
                    var sortDirection = ListSortDirection.Ascending;
                    if (sdc.Count > 0)
                    {
                        SortDescription sd = sdc[0];
                        sortDirection = (ListSortDirection)((((int)sd.Direction) + 1) % 2);
                        sdc.Clear();
                    }
                    sdc.Add(new SortDescription(bindingProperty, sortDirection));
                }
            }
        }

        private void B_删除_Click(object sender, RoutedEventArgs e)
        {
            if (LV_Task.SelectedIndex >= 0)
            {
                taskList.Remove(LV_Task.SelectedItem as RecordTask);
            }
        }

        private void B_编辑_Click(object sender, RoutedEventArgs e)
        {
            if (LV_Task.SelectedItem != null)
            {
                try
                {
                    var t = LV_Task.SelectedItem as RecordTask;
                    t.Personality = TB_P_T.Text;
                    t.Day = CB_DOW.SelectedIndex;
                    var temp = Convert.ToDateTime(TB_ST.Text);
                    t.Hour = temp.Hour;
                    t.Minute = temp.Minute;
                    t.NameFormat = TB_NF.Text;
                    t.Dir = TB_Dir.Text; ;
                    t.Duration = int.Parse(TB_Du.Text);
                    t.Title = TB_标题.Text;
                    taskChanged = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void B_添加_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var t = new RecordTask();
                t.Personality = TB_P_T.Text;
                t.Day = CB_DOW.SelectedIndex;
                var temp = Convert.ToDateTime(TB_ST.Text);
                t.Hour = temp.Hour;
                t.Minute = temp.Minute;
                t.NameFormat = TB_NF.Text;
                t.Dir = TB_Dir.Text; ;
                t.Duration = int.Parse(TB_Du.Text);
                t.Title = TB_标题.Text;
                if (!taskList.Contains(t))
                    taskList.Add(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void B_开始_Click(object sender, RoutedEventArgs e)
        {
            if (taskList.Count > 0)
            {
                B_开始.IsEnabled = false;
                B_停止.IsEnabled = true;
                notifyIcon.Text = "运行中……";
                var t = Settings.Default.前后时间.Split(',');
                pre = int.Parse(t[0]);
                fol = int.Parse(t[1]);
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 10);
                timer.Tick += timer_Tick;
                timer.Start();
                appendLog("任务开始……");
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow.AddHours(9);
            L_Now.Content = "现在时间：" + now.ToString("HH:mm");
            if (now.Hour < 6)
                now = now.AddDays(-1);
            foreach (var t in taskList.Where(t => t.Day == (int)now.DayOfWeek))
            {
                if (runningTask.Values.Contains(t))
                    continue;
                if ((int)now.TimeOfDay.TotalSeconds / 15 == (t.Hour * 3600 + t.Minute * 60 - pre) / 15)
                {
                    if (!Directory.Exists(t.Dir))
                        Directory.CreateDirectory(t.Dir);
                    startProcess(t, false);
                    changeLabel();
                    return;
                }
            }
        }

        private void changeLabel()
        {
            L_Count.Content = "运行中的任务：" + runningTask.Count.ToString();
        }

        private void startProcess(RecordTask t, Boolean def)
        {
            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo("rtmpdump.exe");
            psi.Arguments = String.Format("-r {0} --live --stop {1} -o \"{2}\"",
                Settings.Default.RTMP, t.Duration * 60 + pre + fol, t.GetFileName(def));
            psi.UseShellExecute = true;
            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            p.StartInfo = psi;
            p.EnableRaisingEvents = true;
            if (!def)
                p.Exited += p_Exited;
            else
                p.Exited += p_Exited_Def;
            p.Start();
            runningTask.Add(p.Id, t);
            notifyIcon.ShowBalloonTip(2000);
            appendLog("录制开始：" + t.GetFileName(def));
        }

        private void p_Exited_Def(object sender, EventArgs e)
        {
            var p = (Process)sender;
            var code = p.ExitCode;
            var rt = runningTask[p.Id];
            var name = rt.GetFileName(true);
            runningTask.Remove(p.Id);
            if (code == 0)
                appendLog("录制完成：" + name);
            else if (code == 1)
                appendLog("录制失败：" + name + " --放弃录制", true);
            else if (code == 2)
                appendLog("录制中断：" + name, true);
            this.Dispatcher.Invoke(changeLabel);
        }

        private void p_Exited(object sender, EventArgs e)
        {
            var p = (Process)sender;
            var code = p.ExitCode;
            var rt = runningTask[p.Id];
            var name = rt.GetFileName();
            runningTask.Remove(p.Id);
            if (code == 0)
                appendLog("录制完成：" + name);
            else if (code == 1)
            {
                appendLog("录制失败：" + name + " --准备重试", true);
                if (!rt.Title.Contains("需修改"))
                    rt.Title = "（需修改）" + rt.Title;
                startProcess(rt, true);
            }
            else if (code == 2)
                appendLog("录制中断：" + rt.GetFileName(), true);
            this.Dispatcher.Invoke(changeLabel);
        }

        private void B_停止_Click(object sender, RoutedEventArgs e)
        {
            B_停止.IsEnabled = false;
            B_开始.IsEnabled = true;
            notifyIcon.Text = "已停止";
            if (timer != null)
            {
                timer.Stop();
                appendLog("任务停止");
            }
        }

        private void B_立即_Click(object sender, RoutedEventArgs e)
        {
            var dir = Settings.Default.默认路径;
            var path = Path.Combine(dir, DateTime.UtcNow.AddHours(9).ToString("yyyyMMdd-HHmmss"));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo("rtmpdump.exe");
            psi.Arguments = String.Format(" -r {0} --live -o \"{1}.flv\"",
                Settings.Default.RTMP, path);
            psi.UseShellExecute = true;
            p.StartInfo = psi;
            p.Start();
        }
        #endregion

        #region 其他功能
        private void B_Exe_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if ((bool)ofd.ShowDialog())
            {
                TB_Exe.Text = ofd.FileName;
                TB_CmdType.Text = Path.GetFileNameWithoutExtension(ofd.FileName);
            }
        }

        private void B_IN_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = Settings.Default.默认路径;
            if ((bool)ofd.ShowDialog())
                TB_In.Text = ofd.FileName;
        }

        private void B_DO_Click(object sender, RoutedEventArgs e)
        {
            var exe = TB_Exe.Text;
            if (!File.Exists(exe))
            {
                MessageBox.Show("未能找到文件");
                return;
            }
            Process p = new Process();
            var psi = new ProcessStartInfo();
            psi.FileName = exe;
            psi.Arguments = String.Format(TB_Arg.Text, TB_In.Text, TB_Out.Text);
            psi.UseShellExecute = true;
            p.StartInfo = psi;
            p.Start();
        }

        private void B_CmdAdd_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TB_CmdName.Text))
            {
                MessageBox.Show("请输入名称");
                TB_CmdName.Focus();
                return;
            }
            CMDArgs cmd = new CMDArgs();
            cmd.Exe = TB_Exe.Text;
            cmd.Type = TB_CmdType.Text;
            cmd.Name = TB_CmdName.Text;
            cmd.Args = TB_Arg.Text;
            cmdList.Add(cmd);
        }

        private void LB_Cmd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_Cmd.SelectedItem != null)
            {
                var cmd = LB_Cmd.SelectedItem as CMDArgs;
                TB_Exe.Text = cmd.Exe;
                TB_Arg.Text = cmd.Args;
                TB_CmdName.Text = cmd.Name;
                TB_CmdType.Text = cmd.Type;
            }
        }

        private void MI_Cmd更新_Click(object sender, RoutedEventArgs e)
        {
            if (LB_Cmd.SelectedItem != null)
            {
                var cmd = LB_Cmd.SelectedItem as CMDArgs;
                cmd.Exe = TB_Exe.Text;
                cmd.Args = TB_Arg.Text;
                cmd.Name = TB_CmdName.Text;
                cmd.Type = TB_CmdType.Text;
                cmdChanged = true;
            }
        }

        private void MI_Cmd删除_Click(object sender, RoutedEventArgs e)
        {
            if (LB_Cmd.SelectedItem != null)
            {
                var cmd = LB_Cmd.SelectedItem as CMDArgs;
                cmdList.Remove(cmd);
            }
        }
        #endregion

        #region HiBiKi
        private HiBiKiBgm prase(String str, String type)
        {
            String s = str;
            var b = new HiBiKiBgm();
            b.Type = type;
            int sp = 0;
            int ep = 0;
            sp = s.IndexOf("Video('") + 7;
            ep = s.IndexOf('\'', sp);
            b.Tag = s.Substring(sp, ep - sp);

            s = s.Substring(ep);
            sp = s.IndexOf("src=\"") + 5;
            ep = s.IndexOf("\"", sp);
            b.ImgUri = s.Substring(sp, ep - sp);

            s = s.Substring(ep);
            var m = Regex.Match(s, "Button(New)?\">");
            sp = m.Index + m.Length;
            ep = s.IndexOf("</div>", sp);
            b.Title = s.Substring(sp, ep - sp).Replace("<br />", "").Replace("\r\n", "");

            s = Regex.Match(s, "Comment\">(.*?)</div>").Value;
            sp = s.IndexOf('>') + 1;
            ep = s.IndexOf('<');
            s = s.Substring(sp, ep - sp);

            b.Count = "";
            var temp = s.Split('　');
            foreach (var item in temp)
            {
                if (Regex.IsMatch(item, "第.*?回"))
                    b.Count = item.FwToHa();
                else
                    b.Date = item.FwToHa();
            }
            return b;
        }

        private async void B_GetHBK_Click(object sender, RoutedEventArgs e)
        {
            var wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            var list = new ObservableCollection<HiBiKiBgm>();
            var index = CB_HKBFilter.SelectedIndex;
            await Task.Run(() =>
            {
                try
                {
                    if (index == 0)
                        for (int i = 1; i < 12; i++)
                        {
                            this.Dispatcher.Invoke(() => L_Pgs.Content = "正在获取…… " + i.ToString() + "/11");
                            hkbPage(wc, list, i);
                        }
                    else
                    {
                        this.Dispatcher.Invoke(() => L_Pgs.Content = "正在获取…… ");
                        hkbPage(wc, list, index);
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
                }
                finally
                {
                    this.Dispatcher.Invoke(() => L_Pgs.Content = "就绪");
                }
            });
            LV_HBK.ItemsSource = list;
        }

        private void hkbPage(WebClient wc, ObservableCollection<HiBiKiBgm> list, int index)
        {
            var html = wc.DownloadString("http://hibiki-radio.jp/get_program/" + index.ToString());
            String t = HiBiKiTabs[index];
            int sp = 0;
            int ep = 0;
            var flag = "<div class=\"hbkProgram\">";
            while (ep != -1)
            {
                sp = html.IndexOf(flag);
                ep = html.IndexOf(flag, sp + flag.Length);
                var temp = "";
                if (ep == -1)
                    temp = html.Substring(sp);
                else
                {
                    temp = html.Substring(sp, ep - sp);
                    html = html.Substring(ep);
                }
                if (temp.Contains("生放送") && !temp.Contains("再配信"))
                    continue;
                list.Add(prase(temp, t));
            }
        }

        private void B_HBKDown_Click(object sender, RoutedEventArgs e)
        {
            if (LV_HBK.SelectedItem != null)
            {
                var b = LV_HBK.SelectedItem as HiBiKiBgm;
                var dir = TB_HBKDir.Text;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var name = String.Format(TB_HBKFormat.Text, b.Time, TB_HBKTitle.Text, b.Count);
                var path = Path.Combine(dir, name);
                Process p = new Process();
                ProcessStartInfo psi = new ProcessStartInfo("rtmpdump.exe");
                psi.Arguments = String.Format(" -r {0} -o \"{1}\" -y mp4:{2}_{3}_{2}_{3}.mp4",
                  Settings.Default.RTMPE, path, b.Time, b.Tag);
                psi.UseShellExecute = true;
                p.StartInfo = psi;
                p.Start();
            }
        }

        private void CB_HKBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_HKBFilter.SelectedIndex == 0)
            {
                LV_HBK.Items.Filter = null;
                return;
            }
            LV_HBK.Items.Filter = o =>
            {
                var b = o as HiBiKiBgm;
                return b.Type == HiBiKiTabs[CB_HKBFilter.SelectedIndex];
            };
        }

        private void LV_HBK_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LV_HBK.SelectedItem != null)
            {
                var b = LV_HBK.SelectedItem as HiBiKiBgm;
                TB_HBKTitle.Text = b.Title;
                if (!String.IsNullOrEmpty(b.ImgUri))
                    IB_Img.Source = new BitmapImage(new Uri(b.ImgUri));
            }
        }
        #endregion

        #region 设置
        private void loadSettings()
        {
            var setting = Settings.Default;
            TBS_AGNameFmt.Text = setting.默认格式;
            TBS_AGRTMP.Text = setting.RTMP;
            TBS_AG番组.Text = setting.番组表;
            TBS_BgmPath.Text = setting.BgmPath;
            TBS_CmdPath.Text = setting.CmdPath;
            TBS_DefaultDir.Text = setting.默认路径;
            TBS_HBKNameFmt.Text = setting.HBKFormat;
            TBS_HBKRTMP.Text = setting.RTMPE;
            TBS_PFTime.Text = setting.前后时间;
            TBS_TaskPath.Text = setting.TaskPath;
            CBS_托盘.IsChecked = setting.最小化到托盘;
        }

        private void B_SaveSetting_Click(object sender, RoutedEventArgs e)
        {
            var setting = Settings.Default;
            setting.默认格式 = TBS_AGNameFmt.Text;
            setting.RTMP = TBS_AGRTMP.Text;
            setting.番组表 = TBS_AG番组.Text;
            setting.BgmPath = TBS_BgmPath.Text;
            setting.CmdPath = TBS_CmdPath.Text;
            setting.默认路径 = TBS_DefaultDir.Text;
            setting.HBKFormat = TBS_HBKNameFmt.Text;
            setting.RTMPE = TBS_HBKRTMP.Text;
            setting.前后时间 = TBS_PFTime.Text;
            setting.TaskPath = TBS_TaskPath.Text;
            setting.最小化到托盘 = (bool)CBS_托盘.IsChecked;
            setting.Save();
        }

        private void B_Reload_Click(object sender, RoutedEventArgs e)
        {
            B_SaveSetting_Click(null, null);
            Settings.Default.Reload();
        }

        private void CB_托盘_Checked(object sender, RoutedEventArgs e)
        {
            var ck = (bool)CBS_托盘.IsChecked;
            this.StateChanged -= Window_StateChanged;
            if (ck)
                this.StateChanged += Window_StateChanged;
        }
        #endregion

        #region 日志
        private void loadLog()
        {
            log = new FlowDocument();
            pgh = new Paragraph();
            log.Blocks.Add(pgh);
        }

        private void B_ShowLog_Click(object sender, RoutedEventArgs e)
        {
            if (logWin == null)
                logWin = new Log(log);
            logWin.Show();
            logWin.Activate();
            logWin.RTB_Log.ScrollToEnd();
        }

        private void LV_BGM_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LV_BGM.SelectedItem != null)
            {
                var b = LV_BGM.SelectedItem as Bangumi;
                var t = new RecordTask();
                t.SetValue(b);
                t.NameFormat = Settings.Default.默认格式;
                t.Dir = Settings.Default.默认路径;
                if (!taskList.Contains(t, EqualityComparer<RecordTask>.Default))
                {
                    taskList.Add(t);
                }
                else
                {
                    MessageBox.Show("已存在");
                }
            }
        }

        private void appendLog(string text, bool error = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                var r = new Run(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + text + Environment.NewLine);
                r.Foreground = error ? Brushes.Red : Brushes.Blue;
                if (pgh.Inlines.Count >= 10000)
                    pgh.Inlines.Remove(pgh.Inlines.FirstInline);
                pgh.Inlines.Add(r);
            });
        }
        #endregion
    }
}
