﻿using HandyControl.Controls;
using ICSharpCode.SharpZipLib.Zip;
using MSL.controls;
using MSL.pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;

namespace MSL.forms
{
    /// <summary>
    /// CreateServer.xaml 的交互逻辑
    /// </summary>
    public partial class CreateServer : HandyControl.Controls.Window
    {
        string DownjavaName;

        string servername;
        string serverjava;
        string serverbase;
        string servercore;
        string servermemory;
        string serverargs;

        public CreateServer()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int a = 1; a != 0; a++)
            {
                if (Directory.Exists(@"MSL\Server") && !(Directory.GetDirectories(@"MSL\Server").Length > 0 || Directory.GetFiles(@"MSL\Server").Length > 0))
                {
                    txb6.Text = AppDomain.CurrentDomain.BaseDirectory + "MSL\\Server";
                    return;
                }
                else if (!Directory.Exists(@"MSL\Server"))
                {
                    //MessageBox.Show(@"MSL\Server");
                    txb6.Text = AppDomain.CurrentDomain.BaseDirectory + "MSL\\Server";
                    return;
                }
                else if (Directory.Exists(@"MSL\Server" + a.ToString()) && !(Directory.GetDirectories(@"MSL\Server" + a.ToString()).Length > 0 || Directory.GetFiles(@"MSL\Server" + a.ToString()).Length > 0))
                {
                    txb6.Text = AppDomain.CurrentDomain.BaseDirectory + "MSL\\Server" + a.ToString();
                    return;
                }
                else if (!Directory.Exists(@"MSL\Server" + a.ToString()))
                {
                    //MessageBox.Show(@"MSL\Server" + a.ToString());
                    txb6.Text = AppDomain.CurrentDomain.BaseDirectory + "MSL\\Server" + a.ToString();
                    return;
                }
            }
        }

        private async void next3_Click(object sender, RoutedEventArgs e)
        {
            if (useJVself.IsChecked == true)
            {
                outlog.Content = "正在检查所选Java可用性，请稍等……";
                txjava.IsEnabled = false;
                a0002_Copy.IsEnabled = false;
                (bool javaAvailability, string javainfo) = await Functions.CheckJavaAvailabilityAsync(txjava.Text);
                txjava.IsEnabled = true;
                a0002_Copy.IsEnabled = true;
                if (javaAvailability)
                {
                    outlog.Content = "所选Java版本：" + javainfo;
                }
                else
                {
                    outlog.Content = "检测Java可用性失败";
                    DialogShow.ShowMsg(this, "检测Java可用性失败，您的Java似乎不可用！请检查是否选择正确！", "错误");
                    usedownloadjv.IsChecked = true;
                    return;
                }
                serverjava = txjava.Text;
                sserver.IsSelected = true;
                sserver.IsEnabled = true;
                sjava.IsEnabled = false;
                await Dispatcher.InvokeAsync(() =>
                {
                    CheckServerPackCore();
                });
            }
            else if (usejvPath.IsChecked == true)
            {
                serverjava = "Java";
                sserver.IsSelected = true;
                sserver.IsEnabled = true;
                sjava.IsEnabled = false;
                await Dispatcher.InvokeAsync(() =>
                {
                    CheckServerPackCore();
                });
            }
            else if (usecheckedjv.IsChecked == true)
            {
                string a = selectCheckedJavaComb.Items[selectCheckedJavaComb.SelectedIndex].ToString();
                serverjava = a.Substring(a.IndexOf(":") + 1);
                sserver.IsSelected = true;
                sserver.IsEnabled = true;
                sjava.IsEnabled = false;
                await Dispatcher.InvokeAsync(() =>
                {
                    CheckServerPackCore();
                });
            }
            else if (usedownloadjv.IsChecked == true)
            {
                try
                {
                    outlog.Content = "当前进度:获取Java下载地址……";
                    string _javaList = await AsyncGetJavaDwnLink();

                    JObject javaList0 = JObject.Parse(_javaList);
                    JObject javaList = (JObject)javaList0["java"];
                    outlog.Content = "当前进度:下载Java……";

                    next3.IsEnabled = false;
                    return5.IsEnabled = false;
                    if (usedownloadjv.IsChecked == true)
                    {
                        try
                        {
                            int dwnJava = 0;
                            await Dispatcher.InvokeAsync(() =>
                            {
                                switch (selectJavaComb.SelectedIndex)
                                {
                                    case 0:
                                        dwnJava = DownloadJava("Java8", javaList["Java8"].ToString());
                                        break;
                                    case 1:
                                        dwnJava = DownloadJava("Java11", javaList["Java11"].ToString());
                                        break;
                                    case 2:
                                        dwnJava = DownloadJava("Java16", javaList["Java16"].ToString());
                                        break;
                                    case 3:
                                        dwnJava = DownloadJava("Java17", javaList["Java17"].ToString());
                                        break;
                                    case 4:
                                        dwnJava = DownloadJava("Java18", javaList["Java18"].ToString());
                                        break;
                                    case 5:
                                        dwnJava = DownloadJava("Java19", javaList["Java19"].ToString());
                                        break;
                                    case 6:
                                        dwnJava = DownloadJava("Java20", javaList["Java20"].ToString());
                                        break;
                                    case 7:
                                        dwnJava = DownloadJava("Java21", javaList["Java21"].ToString());
                                        break;
                                    default:
                                        DialogShow.GrowlErr("请选择一个版本以下载！");
                                        break;
                                }
                            });
                            if (dwnJava == 1)
                            {
                                outlog.Content = "当前进度:解压Java……";
                                bool unzipJava = await UnzipJava();
                                if (unzipJava)
                                {
                                    outlog.Content = "完成";
                                    next3.IsEnabled = true;
                                    return5.IsEnabled = true;
                                    sserver.IsSelected = true;
                                    sserver.IsEnabled = true;
                                    sjava.IsEnabled = false;
                                    await Dispatcher.InvokeAsync(() =>
                                    {
                                        CheckServerPackCore();
                                    });
                                }
                                else
                                {
                                    next3.IsEnabled = true;
                                    return5.IsEnabled = true;
                                    return;
                                }
                            }
                            else if (dwnJava == 2)
                            {
                                outlog.Content = "完成";
                                next3.IsEnabled = true;
                                return5.IsEnabled = true;
                                sserver.IsSelected = true;
                                sserver.IsEnabled = true;
                                sjava.IsEnabled = false;
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    CheckServerPackCore();
                                });
                            }
                            else
                            {
                                DialogShow.ShowMsg(this, "下载取消！", "提示");
                                next3.IsEnabled = true;
                                return5.IsEnabled = true;
                                return;
                            }
                        }
                        catch
                        {
                            DialogShow.GrowlErr("出现错误，请检查网络连接！");
                        }
                    }
                }
                catch
                {
                    next3.IsEnabled = true;
                    return5.IsEnabled = true;
                    await Dispatcher.InvokeAsync(() =>
                    {
                        DialogShow.ShowMsg(this, "出现错误！请检查您的网络连接！", "信息", false, "确定");
                    });
                }
            }
        }
        void CheckServerPackCore()
        {
            if (isImportPack)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(serverbase);
                FileInfo[] fileInfo = directoryInfo.GetFiles("*.jar");
                List<string> files = new List<string>();
                foreach (var file in fileInfo)
                {
                    files.Add(file.Name);
                }
                if (files.Count > 1)
                {
                    string filestr = "";
                    int i = 0;
                    foreach (var file in files)
                    {
                        filestr += "\n" + i.ToString() + "." + file;
                        i++;
                    }
                    bool ret = DialogShow.ShowInput(this, "开服器在整合包中检测到了以下jar文件，你可输选择一个作为开服核心（输入文件前对应的数字，取消为不选择以下文件）\n" + filestr, out string selectFile);
                    if (ret)
                    {
                        txb3.Text = files[int.Parse(selectFile)];
                        if (txb3.Text.Contains("forge") && txb3.Text.Contains("installer"))
                        {
                            bool dialog = DialogShow.ShowMsg(this, "您选择的服务端是forge安装器，是否将其展开安装？\n如果不展开安装，服务器可能无法开启！", "提示", true, "取消");
                            if (dialog)
                            {
                                InstallForgeCustomMode();
                            }
                        }
                        servercore = txb3.Text;
                        sJVM.IsSelected = true;
                        sJVM.IsEnabled = true;
                        sserver.IsEnabled = false;
                    }
                }
                else if (files.Count == 1)
                {
                    bool ret = DialogShow.ShowMsg(this, "开服器在整合包中检测到了jar文件" + files[0] + "，是否选择此文件为开服核心？", "提示", true, "取消");
                    if (ret)
                    {
                        txb3.Text = files[0];
                        if (txb3.Text.Contains("forge") && txb3.Text.Contains("installer"))
                        {
                            bool dialog = DialogShow.ShowMsg(this, "您选择的服务端是forge安装器，是否将其展开安装？\n如果不展开安装，服务器可能无法开启！", "提示", true, "取消");
                            if (dialog)
                            {
                                InstallForgeCustomMode();
                            }
                        }
                        servercore = txb3.Text;
                        sJVM.IsSelected = true;
                        sJVM.IsEnabled = true;
                        sserver.IsEnabled = false;
                    }
                }
                else if (files.Count == 0)
                {
                    DialogShow.GrowlInfo("开服器未在整合包中找到核心文件，请您进行下载或手动选择已有核心，核心的版本要和整合包对应的游戏版本一致");
                }
            }
        }
        private int DownloadJava(string fileName, string downUrl)
        {
            if (!File.Exists(@"MSL\" + fileName + @"\bin\java.exe"))
            {
                DialogShow.ShowMsg(this, "下载Java即代表您接受Java的服务条款：\nhttps://www.oracle.com/downloads/licenses/javase-license1.html", "信息", false, "确定");
                DownjavaName = fileName;
                bool downDialog = DialogShow.ShowDownload(this, downUrl, "MSL", "Java.zip", "下载" + fileName + "中……");
                if (downDialog)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                serverjava = AppDomain.CurrentDomain.BaseDirectory + "MSL\\" + fileName + "\\bin\\java.exe";
                return 2;
            }
        }
        private async Task<bool> UnzipJava()
        {
            try
            {
                string javaDirName = "";
                using (ZipFile zip = new ZipFile(@"MSL\Java.zip"))
                {
                    foreach (ZipEntry entry in zip)
                    {
                        if (entry.IsDirectory == true)
                        {
                            int c0 = entry.Name.Length - entry.Name.Replace("/", "").Length;
                            if (c0 == 1)
                            {
                                javaDirName = entry.Name.Replace("/", "");
                                break;
                            }
                        }
                    }
                }
                FastZip fastZip = new FastZip();
                await Task.Run(() => fastZip.ExtractZip(@"MSL\Java.zip", "MSL", ""));
                File.Delete(@"MSL\Java.zip");
                if (@"MSL\" + javaDirName != @"MSL\" + DownjavaName)
                {
                    Functions.MoveFolder(@"MSL\" + javaDirName, @"MSL\" + DownjavaName);
                }
                while (!File.Exists(@"MSL\" + DownjavaName + @"\bin\java.exe"))
                {
                    await Task.Delay(1000);
                }
                serverjava = AppDomain.CurrentDomain.BaseDirectory + "MSL\\" + DownjavaName + "\\bin\\java.exe";
                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogShow.ShowMsg(this, "解压失败，Java压缩包可能已损坏，请重试！错误代码：" + ex.Message + "\n（注：若多次重试均无法解压的话，请自行去网络上下载安装并使用自定义模式来创建服务器）", "错误");
                return false;
            }
        }
        private void return2_Click(object sender, RoutedEventArgs e)
        {
            sjava.IsSelected = true;
            sjava.IsEnabled = true;
            sserver.IsEnabled = false;
        }
        private void usedefault_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                txb4.IsEnabled = false;
                txb5.IsEnabled = false;
            }
        }

        private void useJVM_Checked(object sender, RoutedEventArgs e)
        {
            txb4.IsEnabled = true;
            txb5.IsEnabled = true;
        }

        private void done_Click(object sender, RoutedEventArgs e)
        {
            if (usedefault.IsChecked == true)
            {
                servermemory = "";
            }
            else
            {
                servermemory = "-Xms" + txb4.Text + "M -Xmx" + txb5.Text + "M";
            }
            serverargs += txb7.Text;
            try
            {
                if (!Directory.Exists(serverbase))
                {
                    Directory.CreateDirectory(serverbase);
                }

                if (!File.Exists(@"MSL\ServerList.json"))
                {
                    File.WriteAllText(@"MSL\ServerList.json", string.Format("{{{0}}}", "\n"));
                }
                JObject _json = new JObject
                {
                    { "name", servername },
                    { "java", serverjava },
                    { "base", serverbase },
                    { "core", servercore },
                    { "memory", servermemory },
                    { "args", serverargs }
                };
                JObject jsonObject = JObject.Parse(File.ReadAllText(@"MSL\ServerList.json", Encoding.UTF8));
                List<string> keys = jsonObject.Properties().Select(p => p.Name).ToList();
                var _keys = keys.Select(x => Convert.ToInt32(x));
                int[] ikeys = _keys.ToArray();
                Array.Sort(ikeys);
                int i = 0;

                foreach (int key in ikeys)
                {
                    if (i == key)
                    {
                        //jsonObject.Add(i.ToString(), _json);
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                jsonObject.Add(i.ToString(), _json);
                File.WriteAllText(@"MSL\ServerList.json", Convert.ToString(jsonObject), Encoding.UTF8);
                DialogShow.ShowMsg(this, "创建完毕，请点击“开启服务器”按钮以开服", "信息");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现错误，请重试：" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void return3_Click(object sender, RoutedEventArgs e)
        {
            sserver.IsSelected = true;
            sserver.IsEnabled = true;
            sJVM.IsEnabled = false;
        }

        private void a0002_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            openfile.Title = "请选择文件";
            openfile.Filter = "JAR文件|*.jar|所有文件类型|*.*";
            var res = openfile.ShowDialog();
            if (res == true)
            {
                txb3.Text = openfile.FileName;
            }
        }

        private void a0003_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txb6.Text = dialog.SelectedPath;
            }
        }

        private void return5_Click(object sender, RoutedEventArgs e)
        {
            if (isImportPack)
            {
                MainGrid.Visibility = Visibility.Visible;
                tabCtrl.Visibility = Visibility.Hidden;
                welcome.IsSelected = true;
                welcome.IsEnabled = true;
                sjava.IsEnabled = false;
                return;
            }
            welcome.IsSelected = true;
            welcome.IsEnabled = true;
            sjava.IsEnabled = false;
        }

        private void a0002_Copy_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            openfile.Title = "请选择文件，通常为java.exe";
            openfile.Filter = "EXE文件|*.exe|所有文件类型|*.*";
            var res = openfile.ShowDialog();
            if (res == true)
            {
                txjava.Text = openfile.FileName;
            }
        }
        private void usedownloadjv_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                txjava.IsEnabled = false;
                a0002_Copy.IsEnabled = false;
            }
        }
        private void usecheckedjv_Checked(object sender, RoutedEventArgs e)
        {
            txjava.IsEnabled = false;
            a0002_Copy.IsEnabled = false;
            selectCheckedJavaComb.Items.Clear();
            List<string> strings =Functions.CheckJava();
            if (strings != null)
            {
                foreach (string str in strings)
                {
                    selectCheckedJavaComb.Items.Add(str);
                }
            }
            
            if (selectCheckedJavaComb.Items.Count > 0)
            {
                outlog.Content = "检测完毕！";
                selectCheckedJavaComb.SelectedIndex = 0;
            }
            else
            {
                outlog.Content = "检测完毕，暂未找到Java";
                usedownloadjv.IsChecked = true;
            }
        }

        private async void usejvPath_Checked(object sender, RoutedEventArgs e)
        {
            outlog.Content = "正在检查环境变量可用性，请稍等……";
            txjava.IsEnabled = false;
            a0002_Copy.IsEnabled = false;
            (bool javaAvailability, string javainfo) = await Functions.CheckJavaAvailabilityAsync("java");
            if (javaAvailability)
            {
                outlog.Content = "环境变量可用性检查完毕，您的环境变量正常！";
                usejvPath.Content = "使用环境变量：" + javainfo;
            }
            else
            {
                outlog.Content = "检测环境变量失败";
                DialogShow.ShowMsg(this, "检测环境变量失败，您的环境变量似乎不存在！", "错误");
                usedownloadjv.IsChecked = true;
            }
        }

        private void useJVself_Checked(object sender, RoutedEventArgs e)
        {
            txjava.IsEnabled = true;
            a0002_Copy.IsEnabled = true;
        }
        private void usedownloadserver_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                txb3.IsEnabled = false;
                a0002.IsEnabled = false;
            }
        }
        private void useServerself_Checked(object sender, RoutedEventArgs e)
        {
            txb3.IsEnabled = true;
            a0002.IsEnabled = true;
        }
        

        

        

        private void next_Click(object sender, RoutedEventArgs e)
        {
            servername = serverNameBox.Text;
            if (new Regex("[\u4E00-\u9FA5]").IsMatch(txb6.Text))
            {
                bool result = DialogShow.ShowMsg(this, "使用带有中文的路径可能造成编码错误，导致无法开服，您确定要继续吗？", "警告", true, "取消");
                if (result == false)
                {
                    return;
                }
            }
            else if (txb6.Text.IndexOf(" ") + 1 != 0)
            {
                bool result = DialogShow.ShowMsg(this, "使用带有空格的路径可能造成编码错误，导致无法开服，您确定要继续吗？", "警告", true, "取消");
                if (result == false)
                {
                    return;
                }
            }
            if (!Path.IsPathRooted(txb6.Text))
            {
                serverbase = AppDomain.CurrentDomain.BaseDirectory + txb6.Text;
            }
            else
            {
                txb6.Text = txb6.Text;
                serverbase = txb6.Text;
            }
            sjava.IsSelected = true;
            sjava.IsEnabled = true;
            welcome.IsEnabled = false;
        }

        bool isImportPack = false;
        private async void importPack_Click(object sender, RoutedEventArgs e)
        {
            bool _dialog = DialogShow.ShowMsg(this, "请选择你要导入在线整合包还是本地整合包！", "提示", true, "导入本地整合包", "导入在线整合包");
            if (_dialog)
            {
                DownloadMods downloadMods = new DownloadMods(1);
                downloadMods.Owner = this;
                downloadMods.ShowDialog();
                if (!File.Exists("MSL\\ServerPack.zip"))
                {
                    DialogShow.ShowMsg(this, "下载失败！", "错误");
                    return;
                }
                string input;
                bool result = DialogShow.ShowInput(this, "服务器名称：", out input, "ImportedServer");
                if (result)
                {
                    servername = input;
                    string serverPath = "";
                    for (int a = 1; a != 0; a++)
                    {
                        if (!Directory.Exists("MSL\\Server"))
                        {
                            serverPath = AppDomain.CurrentDomain.BaseDirectory + "MSL\\Server";
                            break;
                        }
                        if (!Directory.Exists("MSL\\Server" + a.ToString()))
                        {
                            serverPath = AppDomain.CurrentDomain.BaseDirectory + "MSL\\Server" + a.ToString();
                            break;
                        }
                    }
                    Dialog waitDialog = null;
                    try
                    {
                        waitDialog = Dialog.Show(new TextDialog("解压整合包中，请稍等……"));
                        await Task.Run(() => new FastZip().ExtractZip("MSL\\ServerPack.zip", serverPath, ""));
                        DirectoryInfo[] dirs = new DirectoryInfo(serverPath).GetDirectories();
                        if (dirs.Length == 1)
                        {
                            Functions.MoveFolder(dirs[0].FullName, serverPath);
                        }
                        File.Delete("MSL\\ServerPack.zip");
                    }
                    catch (Exception ex)
                    {
                        this.Focus();
                        waitDialog.Close();
                        DialogShow.ShowMsg(this, "整合包解压失败！请确认您的整合包是.zip格式！\n错误代码：" + ex.Message, "错误");
                        return;
                    }
                    this.Focus();
                    waitDialog.Close();
                    MainGrid.Visibility = Visibility.Hidden;
                    tabCtrl.Visibility = Visibility.Visible;
                    isImportPack = true;
                    serverbase = serverPath;
                    DialogShow.GrowlInfo("整合包解压完成！请在此界面选择Java环境，Java的版本要和导入整合包的版本相对应，详情查看界面下方的表格");
                    sjava.IsSelected = true;
                    sjava.IsEnabled = true;
                    welcome.IsEnabled = false;
                }
            }
            else
            {
                bool dialog = DialogShow.ShowMsg(this, "目前仅支持导入.zip格式的整合包文件，如果您要导入的是模组整合包，请确保您下载的整合包是服务器专用包（如RlCraft下载界面就有一个ServerPack的压缩包），否则可能会出现无法开服或者崩溃的问题！", "提示", true, "取消");
                if (dialog == true)
                {
                    string input;
                    bool result = DialogShow.ShowInput(this, "服务器名称：", out input, "ImportedServer");
                    if (result)
                    {
                        servername = input;
                        string serverPath = "";
                        for (int a = 1; a != 0; a++)
                        {
                            if (!Directory.Exists("MSL\\Server"))
                            {
                                serverPath = AppDomain.CurrentDomain.BaseDirectory + "MSL\\Server";
                                break;
                            }
                            if (!Directory.Exists("MSL\\Server" + a.ToString()))
                            {
                                serverPath = AppDomain.CurrentDomain.BaseDirectory + "MSL\\Server" + a.ToString();
                                break;
                            }
                        }
                        OpenFileDialog openfile = new OpenFileDialog();
                        openfile.InitialDirectory = "MSL";
                        openfile.Title = "请选择整合包压缩文件";
                        openfile.Filter = "ZIP文件|*.zip|所有文件类型|*.*";
                        var res = openfile.ShowDialog();
                        if (res == true)
                        {
                            Dialog waitDialog = null;
                            try
                            {
                                waitDialog = Dialog.Show(new TextDialog("解压整合包中，请稍等……"));
                                await Task.Run(() => new FastZip().ExtractZip(openfile.FileName, serverPath, ""));
                                DirectoryInfo[] dirs = new DirectoryInfo(serverPath).GetDirectories();
                                if (dirs.Length == 1)
                                {
                                    Functions.MoveFolder(dirs[0].FullName, serverPath);
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Focus();
                                waitDialog.Close();
                                DialogShow.ShowMsg(this, "整合包解压失败！请确认您的整合包是.zip格式！\n错误代码：" + ex.Message, "错误");
                                return;
                            }
                            this.Focus();
                            waitDialog.Close();
                            MainGrid.Visibility = Visibility.Hidden;
                            tabCtrl.Visibility = Visibility.Visible;
                            isImportPack = true;
                            serverbase = serverPath;
                            DialogShow.GrowlInfo("整合包解压完成！请在此界面选择Java环境，Java的版本要和导入整合包的版本相对应，详情查看界面下方的表格");
                            sjava.IsSelected = true;
                            sjava.IsEnabled = true;
                            welcome.IsEnabled = false;
                        }
                    }
                }
            }
        }


        private void next2_Click(object sender, RoutedEventArgs e)
        {
            if (usedownloadserver.IsChecked == true)
            {
                DownloadServer.downloadServerJava = serverjava;
                DownloadServer.downloadServerBase = serverbase;
                DownloadServer downloadServer = new DownloadServer();
                downloadServer.Owner = this;
                downloadServer.ShowDialog();
                if (File.Exists(serverbase + @"\" + DownloadServer.downloadServerName))
                {
                    servercore = DownloadServer.downloadServerName;
                    sJVM.IsSelected = true;
                    sJVM.IsEnabled = true;
                    sserver.IsEnabled = false;
                }
                else if (DownloadServer.downloadServerName.StartsWith("@libraries/"))
                {
                    servercore = DownloadServer.downloadServerName;
                    sJVM.IsSelected = true;
                    sJVM.IsEnabled = true;
                    sserver.IsEnabled = false;
                }
                else
                {
                    DialogShow.ShowMsg(this, "出现错误，下载失败！", "错误");
                }
            }
            else
            {
                try
                {
                    if (!Directory.Exists(serverbase))
                    {
                        Directory.CreateDirectory(serverbase);
                    }
                    string _filename = Path.GetFileName(txb3.Text);
                    if (!Path.IsPathRooted(txb3.Text))
                    {
                        txb3.Text = AppDomain.CurrentDomain.BaseDirectory + txb3.Text;
                    }
                    if (Path.GetDirectoryName(txb3.Text) != serverbase)
                    {
                        File.Copy(txb3.Text, serverbase + @"\" + _filename, true);
                        DialogShow.ShowMsg(this, "已将服务端文件移至服务器文件夹中！您可将源文件删除！", "提示");
                        txb3.Text = _filename;
                    }

                    if (txb3.Text.Contains("forge") && txb3.Text.Contains("installer"))
                    {
                        bool dialog = DialogShow.ShowMsg(this, "您选择的服务端是forge安装器，是否将其展开安装？\n如果不展开安装，服务器可能无法开启！", "提示", true, "取消");
                        if (dialog)
                        {
                            InstallForgeCustomMode();
                        }
                    }
                    servercore = txb3.Text;
                    sJVM.IsSelected = true;
                    sJVM.IsEnabled = true;
                    sserver.IsEnabled = false;
                    next3.IsEnabled = true;
                    return5.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    DialogShow.ShowMsg(this, ex.Message, "错误");
                }
            }
        }
        void InstallForgeCustomMode()
        {
            bool keepTrying = true;
            while (keepTrying)
            {
                string forgeVersion;
                Match match = Regex.Match(txb3.Text, @"forge-([\w.-]+)-installer");
                forgeVersion = match.Groups[1].Value;
                Directory.SetCurrentDirectory(serverbase);
                Process process = new Process();
                process.StartInfo.FileName = serverjava;
                process.StartInfo.Arguments = "-jar " + txb3.Text + " -installServer";
                process.Start();
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                try
                {

                    while (!process.HasExited)
                    {
                        Thread.Sleep(1000);
                    }
                    if (File.Exists(serverbase + "\\libraries\\net\\minecraftforge\\forge\\" + forgeVersion + "\\win_args.txt"))
                    {
                        txb3.Text = "@libraries/net/minecraftforge/forge/" + forgeVersion + "/win_args.txt %*";
                        keepTrying = false;
                    }
                    else
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(serverbase);
                        FileInfo[] fileInfo = directoryInfo.GetFiles();
                        foreach (FileInfo file in fileInfo)
                        {
                            if (file.Name.IndexOf("forge-" + forgeVersion) + 1 != 0 && !file.Name.Contains("installer"))
                            {
                                txb3.Text = file.FullName.Replace(serverbase + @"\", "");
                                keepTrying = false;
                                break;
                            }
                        }
                        if (keepTrying)
                        {
                            bool dialog = DialogShow.ShowMsg(this, "安装失败,请多次尝试或使用代理再试！\n点击确定重试！", "错误", true, "取消");
                            keepTrying = dialog;
                        }
                    }
                }
                catch
                {
                    DialogShow.ShowMsg(this, "安装失败！", "错误");
                }
            }
        }

        private void skip_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Visibility = Visibility.Visible;
            tabCtrl.Visibility = Visibility.Hidden;
            FastModeGrid.Visibility = Visibility.Hidden;
        }
        private void usebasicfastJvm_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("使用优化参数需要手动设置大小相同的内存，请对上面的内存进行更改！Java11及以上请勿选择此优化参数！", "警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            useJVM.IsChecked = true;
            usefastJvm.IsChecked = false;
            txb7.Text = "-XX:+AggressiveOpts";
            txb4.Text = "2048";
            txb5.Text = "2048";
        }
        private void usefastJvm_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("使用优化参数需要手动设置大小相同的内存，请对上面的内存进行更改！", "警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            useJVM.IsChecked = true;
            usebasicfastJvm.IsChecked = false;
            txb7.Text = "-XX:+UseG1GC -XX:+UnlockExperimentalVMOptions -XX:+ParallelRefProcEnabled -XX:MaxGCPauseMillis=200 -XX:+UnlockExperimentalVMOptions -XX:+DisableExplicitGC -XX:+AlwaysPreTouch -XX:G1NewSizePercent=30 -XX:G1MaxNewSizePercent=40 -XX:G1HeapRegionSize=8M -XX:G1ReservePercent=20 -XX:G1HeapWastePercent=5 -XX:G1MixedGCCountTarget=4 -XX:InitiatingHeapOccupancyPercent=15 -XX:G1MixedGCLiveThresholdPercent=90 -XX:G1RSetUpdatingPauseTimePercent=5 -XX:SurvivorRatio=32 -XX:+PerfDisableSharedMem -XX:MaxTenuringThreshold=1 -Dusing.aikars.flags=https://mcflags.emc.gs -Daikars.new.flags=true";
            txb4.Text = "4096";
            txb5.Text = "4096";
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Growl.SetGrowlParent(GrowlPanel, true);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Growl.SetGrowlParent(GrowlPanel, false);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FastModeBtn_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Visibility = Visibility.Hidden;
            FastModeGrid.Visibility = Visibility.Visible;
            Thread thread = new Thread(FastModeGetCore);
            thread.Start();
        }

        Dictionary<string, List<string>> serverCoreTypes = new Dictionary<string, List<string>>
            {
                {"pluginsCore",new List<string>()
                    {"paper","purpur","spigot","bukkit","folia","leaves"}
                },
                {"pluginsAndModsCore",new List<string>()
                    {"arclight","mohist","catserver"}
                },
                {"modsCore_Forge",new List<string>()
                    {"forge","neoforge"}
                },
                {"modsCore_Fabric",new List<string>()
                    {"fabric","quilt"}
                },
                {"vanillaCore",new List<string>()
                    {"vanilla"}
                },
                {"bedrockCore",new List<string>()
                    {"nukkitx"}
                },
                {"proxyCore",new List<string>()
                    {"velocity","bungeecord","lightfall"}
                }
            };
        string[] serverTypes;
        void FastModeGetCore()
        {
            try
            {
                try
                {
                    if (Functions.Get("") != "200")
                    {
                        MainWindow.serverLink = "waheal.top";
                    }
                }
                catch
                {
                    MainWindow.serverLink = "waheal.top";
                }
                string jsonData = Functions.Get("query/available_server_types", MainWindow.serverLinkNew);
                serverTypes = JsonConvert.DeserializeObject<string[]>(jsonData);
                Dispatcher.Invoke(() =>
                {
                    ServerCoreCombo.SelectedIndex = 0;
                });
            }
            catch (Exception a)
            {
                DialogShow.GrowlInfo("获取服务端失败！请重试" + a.Message);
            }
        }
        List<string> typeVersions = new List<string>();
        private void ServerCoreCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FastModeNextBtn.IsEnabled = false;
            ServerVersionCombo.ItemsSource = null;
            typeVersions.Clear();
            tempServerCore.Clear();
            if (serverTypes == null)
            {
                DialogShow.ShowMsg(this, "服务端正在加载中，请稍后再选择！", "提示");
                return;
            }
            Thread thread = new Thread(GetServerVersion);
            thread.Start();
        }

        private void GetServerVersion()
        {
            int selectType = 0;
            Dispatcher.Invoke(() =>
            {
                ServerCoreDescrip.Text = "加载中，请稍等……";
                selectType = ServerCoreCombo.SelectedIndex;
            });
            try
            {
                int i = 0;
                foreach (var serverType in serverTypes)
                {
                    //MessageBox.Show(serverType);
                    int x = 0;
                    foreach (var coreTypes in serverCoreTypes)
                    {
                        if (x == selectType)
                        {
                            string _serverType = serverType;
                            if (serverType.Contains("（"))
                            {
                                _serverType = serverType.Substring(0, serverType.IndexOf("（"));
                            }
                            foreach (var coreType in coreTypes.Value)
                            {
                                if (coreType == _serverType)
                                {
                                    //MessageBox.Show(_serverType);
                                    try
                                    {
                                        var resultData = Functions.Get("query/available_versions/" + _serverType, MainWindow.serverLinkNew);
                                       // MessageBox.Show(resultData);
                                        tempServerCore.Add(coreType, resultData);
                                        List<string> serverVersions = JsonConvert.DeserializeObject<List<string>>(resultData);
                                        foreach (var item in serverVersions)
                                        {
                                            //MessageBox.Show(item);
                                            if (!typeVersions.Contains(item) && !item.StartsWith("*"))
                                            {
                                                typeVersions.Add(item);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            // JObject patientinfo = new JObject();
                                            // patientinfo["server_name"] = i;
                                            // string sendData = JsonConvert.SerializeObject(patientinfo);
                                            //var resultData = Functions.Post("serverlist", 0, sendData, "https://api.waheal.top");
                                            var resultData = Functions.Get("query/available_versions/" + coreType, MainWindow.serverLinkNew);
                                            //MessageBox.Show(resultData);
                                            tempServerCore.Add(coreType, resultData);
                                            Dictionary<string, string> serverDetails = JsonConvert.DeserializeObject<Dictionary<string, string>>(resultData);
                                            foreach (var item in serverDetails.Keys)
                                            {
                                                //MessageBox.Show(item);
                                                if (!typeVersions.Contains(item) && !item.StartsWith("*"))
                                                {
                                                    typeVersions.Add(item);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Dispatcher.Invoke(() =>
                                            {
                                                DialogShow.ShowMsg(this, "获取服务端失败！请重试！\n错误代码：" + ex.Message, "错误");
                                            });
                                            return;
                                        }
                                    }
                                    //typeVersions = serverDetails.Keys.ToList();
                                }
                            }
                            x++;
                            //continue;
                        }
                        else
                        {
                            x++;
                        }
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    DialogShow.ShowMsg(this, "出现错误：" + ex.Message, "err");
                    FastModeNextBtn.IsEnabled = true;
                    return;
                });
            }
            var sortedList = typeVersions.OrderByDescending(p => Functions.VersionCompare(p)).ToList();
            Dispatcher.Invoke(() =>
            {
                FastModeNextBtn.IsEnabled = true;
                ServerVersionCombo.ItemsSource = sortedList;
                ServerVersionCombo.SelectedIndex = 0;
                switch (ServerCoreCombo.SelectedIndex)
                {
                    case 0:
                        ServerCoreDescrip.Text = "插件服务器：指在服务端添加插件（客户端无需添加），通过更改服务端底层来增加功能，这种方式极易做到对服务器、服务器用户玩家进行管理，如权限组、封禁系统等，但这种方式不能修改客户端内容，所以也导致很多功能很难实现，如添加新的物品，只能通过更改材质包的方式让客户端显示新的物品";
                        break;
                    case 1:
                        ServerCoreDescrip.Text = "插件模组混合服务器（Forge加载器）：这种服务器将插件服务端和Forge服务端合二为一，既吸取了二者的优点（服务器管理功能可通过添加插件做到，添加新物品更改游戏玩法可通过添加模组做到），同时又有许多缺点（如服务器不稳定，同时添加插件和模组，极易造成冲突问题，且也存在模组服务器服务端和客户端需要同步模组的问题）";
                        break;
                    case 2:
                        ServerCoreDescrip.Text = "注意：此服务端的相关库文件源在海外，若多次出现下载失败的情况，请换用混合服务端！\n模组服务器（Forge加载器）：指通过Forge加载器，添加模组来增加功能（服务端和客户端均需添加），这种方式既可以更改服务端的内容，也可以更改客户端的内容，所以插件服务器无法实现的功能在这里即可轻易做到，但是这种方式很难做到插件服的管理功能，且需要客户端的模组和服务端进行同步，会给玩家造成一定的麻烦";
                        break;
                    case 3:
                        ServerCoreDescrip.Text = "模组服务器（Fabric加载器）：指通过Fabric加载器，添加模组来增加功能（服务端和客户端均需添加），这种方式既可以更改服务端的内容，也可以更改客户端的内容，所以插件服务器无法实现的功能在这里即可轻易做到，但是这种方式很难做到插件服的管理功能，且需要客户端的模组和服务端进行同步，会给玩家造成一定的麻烦";
                        break;
                    case 4:
                        ServerCoreDescrip.Text = "原版服务器：Mojang纯原生服务器，不能添加任何插件或模组，给您原汁原味的体验";
                        break;
                    case 5:
                        ServerCoreDescrip.Text = "基岩版服务器：专为基岩版提供的服务器，这种服务器在配置等方面和Java版服务器不太一样，同时开服器也不太适配，更改配置文件等相关操作只能您手动操作";
                        break;
                    case 6:
                        ServerCoreDescrip.Text = "代理服务器：指Java版群组服务器的转发服务器，这种服务器相当于一个桥梁，将玩家在不同的服务器之间进行传送转发，使用这种服务器您首先需要开启一个普通服务器，因为这种服务器没有游戏内容，如果没有普通服务器进行连接，玩家根本无法进入，且目前开服器并不兼容这种服务器，创建完毕后您需在列表右键该服务器并使用“命令行开服”功能来启动";
                        break;
                }
            });
        }

        List<string> downloadCoreUrl = new List<string>();
        Dictionary<string, string> tempServerCore = new Dictionary<string, string>();
        private void FastModeNextBtn_Click(object sender, RoutedEventArgs e)
        {
            servername = ServerNameBox.Text;
            if (new Regex("[\u4E00-\u9FA5]").IsMatch(txb6.Text))
            {
                bool result = DialogShow.ShowMsg(this, "开服器被放置于带有中文的目录里，中文目录可能会造成编码错误导致无法开服，您确定要继续吗？", "警告", true, "取消");
                if (result == false)
                {
                    return;
                }
            }
            else if (txb6.Text.IndexOf(" ") + 1 != 0)
            {
                bool result = DialogShow.ShowMsg(this, "开服器被放置于带有空格的目录里，这种目录可能会造成编码错误导致无法开服，您确定要继续吗？", "警告", true, "取消");
                if (result == false)
                {
                    return;
                }
            }
            serverbase = txb6.Text;
            downloadCoreUrl.Clear();
            FinallyCoreCombo.Items.Clear();
            FastModeNextBtn.IsEnabled = false;

            //Thread thread = new Thread(GetFinallyServerCore);
            //thread.Start();

            foreach (var _item in tempServerCore)
            {
                List<string> serverVersions = JsonConvert.DeserializeObject<List<string>>(_item.Value);
                foreach (var version in serverVersions)
                {
                    if (version == ServerVersionCombo.SelectedItem.ToString() && !FinallyCoreCombo.Items.Contains(_item.Key + "-" + version))
                    {
                        FinallyCoreCombo.Items.Add(_item.Key + "-" + version);
                        // downloadCoreUrl.Add(item.Value); 
                    }
                }
            }

            string versionString = ServerVersionCombo.Items[ServerVersionCombo.SelectedIndex].ToString();
            if (versionString != "Latest")
            {
                if (versionString.Contains("-"))
                {
                    versionString = versionString.Substring(0, versionString.IndexOf("-"));
                }
                string[] components = versionString.Split('.');
                if (components.Length >= 3 && int.TryParse(components[2], out int _))
                {
                    versionString = $"{components[0]}.{components[1]}"; // remove the last component
                }

                Version _version = new Version(versionString);
                Version targetVersion1 = new Version("1.7");
                Version targetVersion2 = new Version("1.12");
                Version targetVersion3 = new Version("1.17");


                if (_version <= targetVersion1)
                {
                    //_version <=1.7
                    FinallyJavaDescrip.Text = "根据您的选择，最适合您服务器的Java版本为：Java7-Java8";
                    FinallyJavaCombo.SelectedIndex = 0;
                }
                else if (_version <= targetVersion2)
                {
                    //1.7< _version <=1.12
                    FinallyJavaDescrip.Text = "根据您的选择，最适合您服务器的Java版本为：Java8-Java11";
                    FinallyJavaCombo.SelectedIndex = 0;
                }
                else if (_version <= targetVersion3)
                {
                    //1.12< _version <=1.17
                    FinallyJavaDescrip.Text = "根据您的选择，最适合您服务器的Java版本为：Java11-Java17（或更高）";
                    FinallyJavaCombo.SelectedIndex = 3;
                }
                else
                {
                    //_version >1.17
                    FinallyJavaDescrip.Text = "根据您的选择，最适合您服务器的Java版本为：Java18-Java19（或更高）";
                    FinallyJavaCombo.SelectedIndex = 5;
                }
            }
            else
            {
                FinallyJavaDescrip.Text = "根据您的选择，最适合您服务器的Java版本为：Java8-Java19（或更高）";
                FinallyJavaCombo.SelectedIndex = 5;
            }
            FinallyCoreCombo.SelectedIndex = 0;
            FastModeNextBtn.IsEnabled = true;
            FastModeGrid.Visibility = Visibility.Hidden;
            InstallGrid.Visibility = Visibility.Visible;
        }

        private async Task<string> AsyncGetJavaDwnLink()
        {
            string url = "https://api.waheal.top";
            if (MainWindow.serverLink != "waheal.top")
            {
                url = "https://api." + MainWindow.serverLink;
            }
            WebClient MyWebClient = new WebClient();
            byte[] pageData = await MyWebClient.DownloadDataTaskAsync(url + "/otherdownloads");
            string _javaList = Encoding.UTF8.GetString(pageData);
            return _javaList;
        }

        private async void FastModeInstallBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FastModeInstallBtn.IsEnabled = false;
                FastInstallProcess.Text = "当前进度:获取Java下载地址……";
                //Thread.Sleep(1000);
                string _javaList = await AsyncGetJavaDwnLink();

                JObject javaList0 = JObject.Parse(_javaList);
                JObject javaList = (JObject)javaList0["java"];
                FastInstallProcess.Text = "当前进度:下载Java……";
                int dwnJava = 0;
                await Dispatcher.InvokeAsync(() =>
                {
                    switch (FinallyJavaCombo.SelectedIndex)
                    {
                        case 0:
                            dwnJava = DownloadJava("Java8", javaList["Java8"].ToString());
                            break;
                        case 1:
                            dwnJava = DownloadJava("Java11", javaList["Java11"].ToString());
                            break;
                        case 2:
                            dwnJava = DownloadJava("Java16", javaList["Java16"].ToString());
                            break;
                        case 3:
                            dwnJava = DownloadJava("Java17", javaList["Java17"].ToString());
                            break;
                        case 4:
                            dwnJava = DownloadJava("Java18", javaList["Java18"].ToString());
                            break;
                        case 5:
                            dwnJava = DownloadJava("Java19", javaList["Java19"].ToString());
                            break;
                        case 6:
                            dwnJava = DownloadJava("Java20", javaList["Java20"].ToString());
                            break;
                        case 7:
                            dwnJava = DownloadJava("Java21", javaList["Java21"].ToString());
                            break;
                        default:
                            DialogShow.GrowlErr("请选择一个版本以下载！");
                            break;
                    }
                });
                if (dwnJava == 1)
                {
                    FastInstallProcess.Text = "当前进度:解压Java……";
                    bool unzipJava = await UnzipJava();
                    if (unzipJava)
                    {
                        FastInstallProcess.Text = "当前进度:下载服务端……";
                        await Dispatcher.InvokeAsync(() =>
                        {
                            FastModeInstallCore();
                        });
                    }
                    else
                    {
                        FastModeInstallBtn.IsEnabled = true;
                        return;
                    }
                }
                else if (dwnJava == 2)
                {
                    FastInstallProcess.Text = "当前进度:下载服务端……";
                    await Dispatcher.InvokeAsync(() =>
                    {
                        FastModeInstallCore();
                    });
                }
                else
                {
                    DialogShow.ShowMsg(this, "下载取消！", "提示");
                    FastInstallProcess.Text = "取消安装！";
                    FastModeInstallBtn.IsEnabled = true;
                    return;
                }
            }
            catch
            {
                DialogShow.GrowlErr("出现错误，请检查网络连接！");
                FastModeInstallBtn.IsEnabled = true;
            }
        }
        void FastModeInstallCore()
        {
            string filename = FinallyCoreCombo.Items[FinallyCoreCombo.SelectedIndex].ToString() + ".jar";
            string dlUrl = Functions.Get("download/server/" + FinallyCoreCombo.SelectedItem.ToString().Replace("-","/"),MainWindow.serverLinkNew);
            bool dwnDialog = DialogShow.ShowDownload(this, dlUrl, serverbase, filename, "下载服务端中……");
            if (!dwnDialog)
            {
                DialogShow.ShowMsg(this, "下载取消！", "提示");
                FastInstallProcess.Text = "取消安装！";
                FastModeInstallBtn.IsEnabled = true;
                return;
            }
            if (File.Exists(serverbase + @"\" + filename))
            {
                servercore = filename;
                bool installReturn = true;
                if (filename.IndexOf("Forge") + 1 != 0)
                {
                    DialogShow.ShowMsg(this, "检测到您下载的是Forge端，开服器将自动进行安装操作，稍后请您不要随意移动鼠标且不要随意触碰键盘，耐心等待安装完毕！", "提示");
                    installReturn = InstallForge();
                }
                if (installReturn)
                {
                    FastInstallProcess.Text = "当前进度:完成！";
                    try
                    {
                        if (!File.Exists(@"MSL\ServerList.json"))
                        {
                            File.WriteAllText(@"MSL\ServerList.json", string.Format("{{{0}}}", "\n"));
                        }
                        JObject _json = new JObject
                        {
                        { "name", servername },
                        { "java", serverjava },
                        { "base", serverbase },
                        { "core", servercore },
                        { "memory", servermemory },
                        { "args", serverargs }
                        };
                        JObject jsonObject = JObject.Parse(File.ReadAllText(@"MSL\ServerList.json", Encoding.UTF8));
                        List<string> keys = jsonObject.Properties().Select(p => p.Name).ToList();
                        var _keys = keys.Select(x => Convert.ToInt32(x));
                        int[] ikeys = _keys.ToArray();
                        Array.Sort(ikeys);
                        int i = 0;

                        foreach (int key in ikeys)
                        {
                            if (i == key)
                            {
                                i++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        jsonObject.Add(i.ToString(), _json);
                        File.WriteAllText(@"MSL\ServerList.json", Convert.ToString(jsonObject), Encoding.UTF8);
                        DialogShow.ShowMsg(this, "创建完毕，请点击“开启服务器”按钮以开服", "信息");
                        Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("出现错误，请重试：" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        FastModeInstallBtn.IsEnabled = true;
                    }
                }
                else
                {
                    DialogShow.ShowMsg(this, "下载失败,请多次尝试或使用代理再试！", "错误");
                    FastModeInstallBtn.IsEnabled = true;
                }
            }
            else
            {
                DialogShow.ShowMsg(this, "下载失败！", "错误");
                FastModeInstallBtn.IsEnabled = true;
            }
        }

        #region InstallForge
        bool InstallForge()
        {
            string filename = FinallyCoreCombo.Items[FinallyCoreCombo.SelectedIndex].ToString() + ".jar";
            string forgeVersion;
            string serverDownUrl = downloadCoreUrl[FinallyCoreCombo.SelectedIndex].ToString();

            if (serverDownUrl.Contains("bmcl"))
            {
                Match match = Regex.Match(serverDownUrl, @"&version=([\w.-]+)&category");
                string version = FinallyCoreCombo.SelectedItem.ToString().Split('-')[1];
                if (version.Contains("-"))
                {
                    string _version = version.Split('-')[0];
                    forgeVersion = _version + "-" + match.Groups[1].Value;
                }
                else
                {
                    forgeVersion = version + "-" + match.Groups[1].Value;
                }
            }
            else
            {
                Match match = Regex.Match(serverDownUrl, @"forge-([\w.-]+)-installer");
                forgeVersion = match.Groups[1].Value;
            }
            Directory.SetCurrentDirectory(serverbase);
            Process process = new Process();
            process.StartInfo.FileName = serverjava;
            process.StartInfo.Arguments = "-jar " + filename + " -installServer";
            process.Start();
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            try
            {

                while (!process.HasExited)
                {
                    Thread.Sleep(1000);
                }
                if (File.Exists(serverbase + "\\libraries\\net\\minecraftforge\\forge\\" + forgeVersion + "\\win_args.txt"))
                {
                    servercore = "@libraries/net/minecraftforge/forge/" + forgeVersion + "/win_args.txt %*";
                    return true;
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(serverbase);
                    FileInfo[] fileInfo = directoryInfo.GetFiles();
                    foreach (FileInfo file in fileInfo)
                    {
                        if (file.Name.IndexOf("forge-" + forgeVersion) + 1 != 0)
                        {
                            servercore = file.FullName.Replace(serverbase + @"\", "");
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        private void CustomModeBtn_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Visibility = Visibility.Hidden;
            tabCtrl.Visibility = Visibility.Visible;
        }

        private void FastModeReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            //FinallyCoreCombo.Items.Clear();
            //downloadCoreUrl.Clear();
            InstallGrid.Visibility = Visibility.Hidden;
            FastModeGrid.Visibility = Visibility.Visible;
            //FastModeNextBtn.IsEnabled=false;
        }
    }
}
