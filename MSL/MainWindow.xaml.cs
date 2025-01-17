﻿using HandyControl.Controls;
using HandyControl.Themes;
using MSL.controls;
using MSL.pages;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Brush = System.Windows.Media.Brush;
using MessageBox = System.Windows.MessageBox;

namespace MSL
{
    public delegate void DeleControl();
    /// <summary>
    /// xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        readonly Home _homePage = new Home();
        readonly ServerList _listPage = new ServerList();
        readonly FrpcPage _frpcPage = new FrpcPage();
        readonly OnlinePage _onlinePage = new OnlinePage();
        readonly SettingsPage _setPage = new SettingsPage();
        readonly About _aboutPage = new About();
        public static event DeleControl AutoOpenServer;
        public static event DeleControl AutoOpenFrpc;
        public static string serverid;
        public static string serverLink;
        public static string serverLinkNew = "https://mslapi.xiaoyulu.cn/v2";
        public static float PhisicalMemory;
        public static bool getServerInfo = false;
        public static bool getPlayerInfo = false;

        public MainWindow()
        {
            InitializeComponent();
            Home.GotoFrpcEvent += GotoOnlinePage;
            SettingsPage.C_NotifyIcon += CtrlNotifyIcon;
            ServerRunner.GotoFrpcEvent += GotoFrpcPage;
            SettingsPage.ChangeSkinStyle += ChangeSkinStyle;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            LoadingCircle loadingCircle = new LoadingCircle();
            MainGrid.Children.Add(loadingCircle);
            MainGrid.RegisterName("loadingBar", loadingCircle);
            this.Topmost = false;
            //Logger.LogInfo("主窗体UI控件加载完毕！");
            Thread thread = new Thread(AsyncLoadEvent);
            thread.Start();
            //Logger.LogInfo("异步载入线程已启动！");
        }

        private void AsyncLoadEvent()
        {
            if (!Directory.Exists("MSL"))
            {
                Process.Start("https://www.mslmc.cn/eula.html");
                Dispatcher.Invoke(() =>
                {
                    bool dialog = DialogShow.ShowMsg(this, "请阅读并同意MSL开服器使用协议：https://www.mslmc.cn/eula.html", "提示", true, "不同意", "同意");
                    if (!dialog)
                    {
                        //Logger.LogWarning("用户未同意使用协议，退出软件……");
                        Close();
                        Environment.Exit(0);
                    }
                });
                Directory.CreateDirectory("MSL");
                //Logger.LogWarning("未检测到MSL文件夹，已进行创建");
            }

            //get serverlink
            try
            {
                serverLink = Functions.Get("", "https://waheal.oss-cn-hangzhou.aliyuncs.com/");
                try
                {
                    if (Functions.Get("")!="200")
                    {
                        serverLink = "waheal.top";
                        DialogShow.GrowlInfo("MSL主服务器连接超时（可能被DDos），已切换至备用服务器！");
                    }
                }
                catch
                {
                    serverLink = "waheal.top";
                    DialogShow.GrowlInfo("MSL主服务器连接超时（可能被DDos），已切换至备用服务器！");
                }
            }
            catch
            {
                serverLink = "waheal.top";
                //Logger.LogError("在匹配在线服务器时出现错误，已连接至备用服务器");
            }

            try
            {
                //firstLauchEvent
                if (!File.Exists(@"MSL\config.json"))
                {
                    //Logger.LogWarning("未检测到config.json文件，载入首次启动事件");
                    File.WriteAllText(@"MSL\config.json", string.Format("{{{0}}}", "\n"));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    //Logger.LogError("生成config.json文件失败，原因："+ex.Message);
                    DialogShow.ShowMsg(this, "MSL在初始化加载过程中出现问题，请尝试用管理员身份运行MSL……\n错误代码：" + ex.Message, "错误");
                    Close();
                });
            }

            //Logger.LogInfo("开始载入配置文件……");
            JObject jsonObject = null;
            try
            {
                jsonObject = JObject.Parse(File.ReadAllText(@"MSL\config.json", Encoding.UTF8));
                ////Logger.LogInfo("读取config.json成功！");
            }
            catch (Exception ex)
            {
                //Logger.LogError("读取config.json失败！尝试重新载入……");
                Dispatcher.Invoke(() =>
                {
                    DialogShow.ShowMsg(this, "MSL在加载配置文件时出现错误，将进行重试，若点击确定后软件突然闪退，请尝试使用管理员身份运行或将此问题报告给作者！\n错误代码：" + ex.Message, "错误");
                });
                File.WriteAllText(@"MSL\config.json", string.Format("{{{0}}}", "\n"));
                jsonObject = JObject.Parse(File.ReadAllText(@"MSL\config.json", Encoding.UTF8));
                //Logger.LogInfo("读取config.json成功！");
            }

            //下面是加载配置部分
            try
            {
                if (jsonObject["notifyIcon"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("notifyIcon", "False");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                }
                else if (jsonObject["notifyIcon"].ToString() == "True")
                {
                    Dispatcher.Invoke(() =>
                    {
                        CtrlNotifyIcon();
                    });
                }
                //Logger.LogInfo("读取托盘图标配置成功！");
                if (jsonObject["sidemenu"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("sidemenu", "0");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                    Dispatcher.Invoke(() =>
                    {
                        sideMenuContextOpen.Width = 100;
                        SideMenu.Width = 100;
                        frame.Margin = new Thickness(100, 0, 0, 0);
                    });
                }
                else if (jsonObject["sidemenu"].ToString() == "0")
                {
                    Dispatcher.Invoke(() =>
                    {
                        sideMenuContextOpen.Width = 100;
                        SideMenu.Width = 100;
                        frame.Margin = new Thickness(100, 0, 0, 0);
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        sideMenuContextOpen.Width = 50;
                        SideMenu.Width = 50;
                        frame.Margin = new Thickness(50, 0, 0, 0);
                    });
                }
                //Logger.LogInfo("读取侧栏配置成功！");
                if (jsonObject["skin"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("skin", "1");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                    Dispatcher.Invoke(() =>
                    {
                        BrushConverter brushConverter = new BrushConverter();
                        ThemeManager.Current.AccentColor = (Brush)brushConverter.ConvertFromString("#0078D4");
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        switch (jsonObject["skin"].ToString())
                        {
                            case "0":
                                ThemeManager.Current.UsingSystemTheme = true;
                                break;
                            case "1":
                                BrushConverter brushConverter = new BrushConverter();
                                ThemeManager.Current.AccentColor = (Brush)brushConverter.ConvertFromString("#0078D4");
                                break;
                            case "2":
                                ThemeManager.Current.AccentColor = Brushes.Red;
                                break;
                            case "3":
                                ThemeManager.Current.AccentColor = Brushes.Green;
                                break;
                            case "4":
                                ThemeManager.Current.AccentColor = Brushes.Orange;
                                break;
                            case "5":
                                ThemeManager.Current.AccentColor = Brushes.Purple;
                                break;
                            case "6":
                                ThemeManager.Current.AccentColor = Brushes.DeepPink;
                                break;
                            default:
                                BrushConverter _brushConverter = new BrushConverter();
                                ThemeManager.Current.AccentColor = (Brush)_brushConverter.ConvertFromString("#0078D4");
                                break;
                        }
                    });
                }
                //Logger.LogInfo("读取皮肤配置成功！");
                if (jsonObject["darkTheme"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("darkTheme", "False");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                }
                else if (jsonObject["darkTheme"].ToString() == "True")
                {
                    Dispatcher.Invoke(() =>
                    {
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    });
                }
                //Logger.LogInfo("读取暗色模式配置成功！");
                if (File.Exists("MSL\\Background.png"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        Background = new ImageBrush(SettingsPage.GetImage("MSL\\Background.png"));
                        SideMenuBorder.BorderThickness = new Thickness(0);
                    });
                }
                //Logger.LogInfo("加载背景图片成功！");
                if (jsonObject["semitransparentTitle"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("semitransparentTitle", "False");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                }
                else if (jsonObject["semitransparentTitle"].ToString() == "True")
                {
                    Dispatcher.Invoke(() =>
                    {
                        ChangeTitleStyle(true);
                    });
                }
                //Logger.LogInfo("读取标题栏样式成功！");
                if (jsonObject["autoGetServerInfo"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("autoGetServerInfo", "True");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                    getServerInfo = true;
                }
                else if (jsonObject["autoGetServerInfo"].ToString() == "True")
                {
                    getServerInfo = true;
                }
                if (jsonObject["autoGetPlayerInfo"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("autoGetPlayerInfo", "True");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                    getPlayerInfo = true;
                }
                else if (jsonObject["autoGetPlayerInfo"].ToString() == "True")
                {
                    getPlayerInfo = true;
                }
                //Logger.LogInfo("读取自动化功能配置成功（自动打开显示占用、记录玩家功能）！");
            }
            catch (Exception ex)
            {
                //Logger.LogError("读取配置时出现错误！错误代码："+ex.Message);
                DialogShow.GrowlErr("MSL在加载配置文件时出现错误，此报错可能不影响软件运行，但还是建议您将其反馈给作者！\n错误代码：" + ex.Message);
            }

            //更新
            try
            {
                string pageHtml = "";
                //string pageHtml = Functions.Get("update");
                //string strtempa = "#";
                //int IndexofA = pageHtml.IndexOf(strtempa);
                //string Ru = pageHtml.Substring(IndexofA + 1);
                //string aaa = Ru.Substring(0, Ru.IndexOf("#"));
                string aaa = Functions.Get("query/update",MainWindow.serverLinkNew);
                if (aaa.Contains("v"))
                {
                    aaa = aaa.Replace("v", "");
                }
                Version newVersion = new Version(aaa);
                Version version = new Version(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

                if (newVersion > version)
                {
                    //Logger.LogInfo("检测到新版本！");
                    var updatelog = Functions.Get("query/update/log", MainWindow.serverLinkNew);
                    Dispatcher.Invoke(() =>
                    {
                        if (jsonObject["autoUpdateApp"] == null)
                        {
                            string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                            JObject jobject = JObject.Parse(jsonString);
                            jobject.Add("autoUpdateApp", "False");
                            string convertString = Convert.ToString(jobject);
                            File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                        }
                        else if (jsonObject["autoUpdateApp"].ToString() == "True")
                        {
                            //Logger.LogInfo("自动更新功能已打开，更新新版本……");
                            UpdateApp(pageHtml, aaa);
                        }
                        bool dialog = DialogShow.ShowMsg(this, "发现新版本，版本号为：" + aaa + "，是否进行更新？\n更新日志：\n" + updatelog, "更新", true, "取消");
                        if (dialog == true)
                        {
                            //Logger.LogInfo("更新新版本……");
                            UpdateApp(pageHtml, aaa);
                        }
                        else
                        {
                            //Logger.LogInfo("用户拒绝更新！");
                            DialogShow.GrowlErr("您拒绝了更新新版本，若在此版本中遇到bug，请勿报告给作者！");
                        }
                    });
                }
                else if (newVersion < version)
                {
                    DialogShow.GrowlInfo("当前版本高于正式版本，若使用中遇到BUG，请及时反馈！");
                }
                else
                {
                    DialogShow.GrowlSuccess("您使用的开服器已是最新版本！");
                }
            }
            catch
            {
                //Logger.LogError("检测更新失败！");
                DialogShow.GrowlErr("检查更新失败！");
            }
            //获取电脑内存
            try
            {
                //Logger.LogInfo("读取系统内存……");
                PhisicalMemory = GetPhisicalMemory();
            }
            catch (Exception ex)
            {
                //Logger.LogError("读取系统内存失败！");
                MessageBox.Show("获取系统内存失败！" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                PhisicalMemory = 0;
            }
            //自动开启服务器
            try
            {
                if (jsonObject["autoOpenServer"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("autoOpenServer", "False");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                }
                else if (jsonObject["autoOpenServer"].ToString() != "False")
                {
                    string servers = jsonObject["autoOpenServer"].ToString();
                    DialogShow.GrowlInfo("正在为你自动打开相应服务器……");
                    while (servers != "")
                    {
                        int aserver = servers.IndexOf(",");
                        serverid = servers.Substring(0, aserver);
                        AutoOpenServer();
                        servers = servers.Replace(serverid + ",", "");
                    }
                }
                //Logger.LogInfo("读取自动开启（服务器）配置成功！");
            }
            catch (Exception ex)
            {
                //Logger.LogError("读取自动开启（服务器）配置失败！");
                MessageBox.Show("自动启动服务器失败！" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //自动开启Frpc
            try
            {
                if (jsonObject["autoOpenFrpc"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("autoOpenFrpc", "False");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                }
                else if (jsonObject["autoOpenFrpc"].ToString() == "True")
                {
                    DialogShow.GrowlInfo("正在为你自动打开内网映射……");
                    AutoOpenFrpc();
                }
                //Logger.LogInfo("读取自动开启（内网映射）配置成功！");
            }
            catch (Exception ex)
            {
                //Logger.LogError("读取自动开启（内网映射）配置失败！");
                MessageBox.Show("自动启动内网映射失败！" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Logger.LogInfo("所有配置载入完毕！调整UI界面……");
            Dispatcher.Invoke(() =>
            {
                SideMenu.IsEnabled = true;
                SideMenu.SelectedIndex = 0;
                LoadingCircle loadingCircle = MainGrid.FindName("loadingBar") as LoadingCircle;
                MainGrid.Children.Remove(loadingCircle);
                MainGrid.UnregisterName("loadingBar");
            });
            //Logger.LogInfo("软件加载完毕！");
        }

        private void UpdateApp(string pageHtml, string aaa)
        {
            //string strtempa1 = "* ";
            //int IndexofA1 = pageHtml.IndexOf(strtempa1);
            //string Ru1 = pageHtml.Substring(IndexofA1 + 2);
            string aaa1 = Functions.Get("download/update",MainWindow.serverLinkNew);
            DialogShow.ShowDownload(this, aaa1, AppDomain.CurrentDomain.BaseDirectory, "MSL" + aaa + ".exe", "下载新版本中……");
            if (File.Exists("MSL" + aaa + ".exe"))
            {
                string oldExePath = Process.GetCurrentProcess().MainModule.ModuleName;
                string newExeDir = AppDomain.CurrentDomain.BaseDirectory;

                string cmdCommand = "/C choice /C Y /N /D Y /T 1 & Del \"" + oldExePath + "\" & Ren \"" + "MSL" + aaa + ".exe" + "\" \"MSL.exe\" & start \"\" \"MSL.exe\"";

                // 关闭当前运行中的应用程序
                Application.Current.Shutdown();

                // 删除旧版本并启动新版本
                Process delProcess = new Process();
                delProcess.StartInfo.FileName = "cmd.exe";
                delProcess.StartInfo.Arguments = cmdCommand;
                Directory.SetCurrentDirectory(newExeDir);
                delProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                delProcess.Start();

                // 退出当前进程
                Process.GetCurrentProcess().Kill();
                //Environment.Exit(0);
            }
            else
            {
                MessageBox.Show("更新失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static long GetPhisicalMemory()
        {
            long amemory = 0;
            //获得物理内存 
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    amemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
            return amemory;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Logger.LogInfo("MSL，关闭！");
            if (MainNoticyIcon.Visibility == Visibility.Visible)
            {
                //Logger.LogWarning("托盘图标已打开，取消关闭事件！");
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
                //Logger.LogInfo("窗口已隐藏！");
                return;
            }
            else if (CloseApp())
            {
                //Logger.LogInfo("正在结束MSL进程……！");
                Application.Current.Shutdown();
                //Process.GetCurrentProcess().Kill();
                //Environment.Exit(0);

            }
            else
            {
                //Logger.LogWarning("取消关闭事件！");
                e.Cancel = true;
            }
        }
        private bool CloseApp()
        {
            try
            {
                if (ServerList.RunningServerIDs.Contains("_1") || FrpcPage.FRPCMD.HasExited == false || OnlinePage.FRPCMD.HasExited == false)
                {
                    //Logger.LogWarning("服务器、内网映射或联机功能正在运行中！弹出对话框询问是否关闭……");
                    bool dialog = DialogShow.ShowMsg(this, "您的服务器、内网映射或联机功能正在运行中，关闭软件可能会让服务器进程在后台一直运行并占用资源！确定要继续关闭吗？\n注：如果想隐藏主窗口的话，请前往设置打开托盘图标", "警告", true, "取消");
                    if (dialog == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return true;
                }
            }
            catch
            {
                try
                {
                    if (FrpcPage.FRPCMD.HasExited == false || OnlinePage.FRPCMD.HasExited == false)
                    {
                        //Logger.LogWarning("内网映射或联机功能正在运行中！弹出对话框询问是否关闭……");
                        bool dialog = DialogShow.ShowMsg(this, "内网映射或联机功能正在运行中，关闭软件可能会让内网映射进程在后台一直运行并占用资源！确定要继续关闭吗？\n如果想隐藏主窗口的话，请前往设置打开托盘图标", "警告", true, "取消");
                        if (dialog == true)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                catch
                {
                    try
                    {
                        if (OnlinePage.FRPCMD.HasExited == false)
                        {
                            //Logger.LogWarning("联机功能正在运行中！弹出对话框询问是否关闭……");
                            bool dialog = DialogShow.ShowMsg(this, "联机功能正在运行中，关闭软件可能会让内网映射进程在后台一直运行并占用资源！确定要继续关闭吗？\n如果想隐藏主窗口的话，请前往设置打开托盘图标", "警告", true, "取消");
                            if (dialog == true)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        return true;
                    }
                }
            }
        }

        void CtrlNotifyIcon()//C_NotifyIcon
        {
            if (MainNoticyIcon.Visibility == Visibility.Hidden)
            {
                MainNoticyIcon.Visibility = Visibility.Visible;
            }
            else
            {
                MainNoticyIcon.Visibility = Visibility.Hidden;
            }
        }

        void GotoFrpcPage()
        {
            this.Focus();
            frame.Content = _frpcPage;
            SideMenu.SelectedIndex = 2;
        }
        void GotoOnlinePage()
        {
            frame.Content = _onlinePage;
            SideMenu.SelectedIndex = 3;
        }
        void ChangeSkinStyle()
        {
            try
            {
                JObject jsonObject = JObject.Parse(File.ReadAllText(@"MSL\config.json", Encoding.UTF8));
                if (jsonObject["semitransparentTitle"].ToString() == "True")
                {
                    ChangeTitleStyle(true);
                }
                else
                {
                    ChangeTitleStyle(false);
                }
                if (File.Exists("MSL\\Background.png"))//check background and set it
                {
                    SideMenuBorder.BorderThickness = new Thickness(0);
                    Background = new ImageBrush(SettingsPage.GetImage("MSL\\Background.png"));
                }
                else
                {
                    SetResourceReference(BackgroundProperty, "BackgroundBrush");
                    SideMenuBorder.BorderThickness = new Thickness(0, 0, 1, 0);
                }
                //RunFormChangeTitle();
            }
            catch
            { }
        }
        void ChangeTitleStyle(bool isOpen)
        {
            if (isOpen)
            {
                this.SetResourceReference(NonClientAreaBackgroundProperty, "SideMenuBrush");
                TitleBox.SetResourceReference(ForegroundProperty, "PrimaryTextBrush");
                this.SetResourceReference(CloseButtonForegroundProperty, "PrimaryTextBrush");
                this.SetResourceReference(OtherButtonForegroundProperty, "PrimaryTextBrush");
            }
            else
            {
                this.SetResourceReference(NonClientAreaBackgroundProperty, "PrimaryBrush");
                TitleBox.Foreground = Brushes.White;
                CloseButtonForeground = Brushes.White;
                OtherButtonForeground = Brushes.White;
            }
        }

        private void sideMenuContextOpen_Click(object sender, RoutedEventArgs e)
        {
            if (sideMenuContextOpen.Width == 50)
            {
                sideMenuContextOpen.Width = 100;
                SideMenu.Width = 100;
                frame.Margin = new Thickness(100, 0, 0, 0);
                string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["sidemenu"] = "0";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
            }
            else
            {
                sideMenuContextOpen.Width = 50;
                SideMenu.Width = 50;
                frame.Margin = new Thickness(50, 0, 0, 0);
                string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["sidemenu"] = "1";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Growl.SetGrowlParent(GrowlPanel, true);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Growl.SetGrowlParent(GrowlPanel, false);
        }


        private void SideMenu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (SideMenu.SelectedIndex)
            {
                case 0:
                    frame.Content = _homePage;
                    break;
                case 1:
                    frame.Content = _listPage;
                    break;
                case 2:
                    frame.Content = _frpcPage;
                    break;
                case 3:
                    frame.Content = _onlinePage;
                    break;
                case 4:
                    frame.Content = _setPage;
                    break;
                case 5:
                    frame.Content = _aboutPage;
                    break;
            }
        }

        private void MainNoticyIcon_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void NotifyClose_Click(object sender, RoutedEventArgs e)
        {
            if (CloseApp())
            {
                Application.Current.Shutdown();
                //Process.GetCurrentProcess().Kill();
                //Environment.Exit(0);
            }
            else
            {
                return;
            }
        }
    }
}
