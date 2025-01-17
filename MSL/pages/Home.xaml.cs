﻿using MSL.controls;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace MSL.pages
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : System.Windows.Controls.Page
    {
        public static event DeleControl AutoOpenServer;
        public static event DeleControl GotoFrpcEvent;
        public Home()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(GetNotice);
            thread.Start();
            //welcomelabel.Content = "MSL开服器 版本：" + MainWindow.update;
            GetServerConfig();
        }
        void GetNotice()
        {
            //公告
            //version
            string noticeLabText = "";

            Dispatcher.Invoke(() =>
            {
                noticeLabText = noticeLab.Text;
            });
            string noticeversion1;
            try
            {
                string noticeversion = Functions.Get("query/notice/id", MainWindow.serverLinkNew);
                JObject jsonObject = JObject.Parse(File.ReadAllText(@"MSL\config.json", Encoding.UTF8));
                if (jsonObject["notice"] == null)
                {
                    string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject.Add("notice", "0");
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                    noticeversion1 = "0";
                }
                else
                {
                    noticeversion1 = jsonObject["notice"].ToString();
                }
                if (noticeversion1 != noticeversion)
                {
                    var notice = Functions.Get("query/notice/main", MainWindow.serverLinkNew);
                    var recommendations = Functions.Get("query/notice/tips", MainWindow.serverLinkNew);

                    if (!string.IsNullOrEmpty(notice))
                    {
                        noticeLabText = notice;
                        if (noticeLabText != "")
                        {
                            Dispatcher.Invoke(() =>
                            {
                                var mainwindow = (MainWindow)System.Windows.Window.GetWindow(this);
                                DialogShow.ShowMsg(mainwindow, noticeLabText, "公告", false, "确定");
                            });
                        }
                    }
                    else
                    {
                        noticeLabText = "获取公告失败！请检查网络连接是否正常或联系作者进行解决！";
                    }

                    if (!string.IsNullOrEmpty(recommendations))
                    {
                        LoadRecommendations(recommendations);
                    }

                    try
                    {
                        string jsonString = File.ReadAllText(@"MSL\config.json", Encoding.UTF8);
                        JObject jobject = JObject.Parse(jsonString);
                        jobject["notice"] = noticeversion.ToString();
                        string convertString = Convert.ToString(jobject);
                        File.WriteAllText(@"MSL\config.json", convertString, Encoding.UTF8);
                    }
                    catch (Exception a)
                    {
                        MessageBox.Show(a.Message);
                    }
                }
                else if (noticeLabText == "")
                {
                    Visibility noticevisible = Visibility.Visible;
                    Dispatcher.Invoke(() =>
                    {
                        noticevisible = noticeLab.Visibility;
                    });
                    if (noticevisible == Visibility.Visible)
                    {
                        var notice = Functions.Get("query/notice/main", MainWindow.serverLinkNew);
                        if (!string.IsNullOrEmpty(notice))
                        {
                            noticeLabText = notice;
                        }
                        else
                        {
                            noticeLabText = "获取公告失败！请检查网络连接是否正常或联系作者进行解决！";
                        }
                        var recommendations = Functions.Get("query/notice/tips", MainWindow.serverLinkNew);
                        if (!string.IsNullOrEmpty(recommendations))
                        {
                            LoadRecommendations(recommendations);
                        }
                    }
                }
            }
            catch
            {
                noticeLabText = "获取公告失败！请检查网络连接是否正常或联系作者进行解决！";
            }

            Dispatcher.Invoke(() =>
            {
                if (noticeLabText == "")
                {
                    noticeLab.Visibility = Visibility.Hidden;
                    noticeLab.Text = "";
                    noticeImage.Source = new BitmapImage(new Uri("https://msl." + MainWindow.serverLinkNew + "/notice.png"));
                }
                else
                {
                    noticeLab.Visibility = Visibility.Visible;
                    noticeImage.Source = null;
                    noticeLab.Text = noticeLabText;
                }

            });
        }


        void LoadRecommendations(string recommendations)
        {
            Dispatcher.Invoke(() =>
            {
                recommendBorder.Visibility = Visibility.Visible;
                for (int x = 1; x < 100; x++)
                {
                    TextBlock textBlock = RecommendGrid.FindName("RecText" + x.ToString()) as TextBlock;
                    if (textBlock != null)
                    {
                        RecommendGrid.Children.Remove(textBlock);
                        RecommendGrid.UnregisterName("RecText" + x.ToString());
                        Image img = RecommendGrid.FindName("RecImg" + x.ToString()) as Image;
                        if (img != null)
                        {
                            RecommendGrid.Children.Remove(img);
                            RecommendGrid.UnregisterName("RecImg" + x.ToString());
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            });

            // Parse the JSON array from the string
            JArray recommendationList = JArray.Parse(recommendations);
            int i = 0;
            foreach (var recommendation in recommendationList)
            {
                i++;
                Dispatcher.Invoke(() =>
                {
                    Image image = new Image();
                    if (i == 1)
                    {
                        image.Margin = new Thickness(5, 35 * i, 5, 5);
                    }
                    else
                    {
                        image.Margin = new Thickness(5, 35 + (53 * (i - 1)), 5, 5);
                    }
                    image.HorizontalAlignment = HorizontalAlignment.Left;
                    image.VerticalAlignment = VerticalAlignment.Top;
                    image.Width = 48;
                    image.Height = 48;
                    if (recommendation.ToString().StartsWith("*"))
                    {
                        image.Source = new BitmapImage(new Uri("pack://application:,,,/icon.ico"));
                    }
                    else
                    {
                        image.Source = new BitmapImage(new Uri("pack://application:,,,/icon.ico"));
                        //image.Source = new BitmapImage(new Uri("https://msl." + MainWindow.serverLinkNew + "/recommendImg/" + i.ToString() + ".png"));
                    }
                    RecommendGrid.Children.Add(image);
                    RecommendGrid.RegisterName("RecImg" + i.ToString(), image);

                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = recommendation.ToString();
                    textBlock.SetResourceReference(ForegroundProperty, "PrimaryTextBrush");
                    if (i == 1)
                    {
                        textBlock.Margin = new Thickness(58, 35 * i, 5, 5);
                    }
                    else
                    {
                        textBlock.Margin = new Thickness(58, 35 + (53 * (i - 1)), 5, 5);
                    }
                    RecommendGrid.Children.Add(textBlock);
                    RecommendGrid.RegisterName("RecText" + i.ToString(), textBlock);
                });
            }
        }


        void GetServerConfig()
        {
            try
            {
                int i = 0;
                JObject _jsonObject = JObject.Parse(File.ReadAllText(@"MSL\config.json", Encoding.UTF8));
                if (_jsonObject["selectedServer"] != null)
                {
                    //MessageBox.Show(_jsonObject["selectedServer"].ToString());
                    int _i = int.Parse(_jsonObject["selectedServer"].ToString());
                    if (_i != -1)
                    {
                        i = _i;
                    }
                }
                ServerList.serverid.Clear();
                startServerDropdown.Items.Clear();
                JObject jsonObject = JObject.Parse(File.ReadAllText(@"MSL\ServerList.json", Encoding.UTF8));
                foreach (var item in jsonObject)
                {
                    ServerList.serverid.Add(item.Key);
                    startServerDropdown.Items.Add(item.Value["name"]);
                }
                startServerDropdown.SelectedIndex = i;
            }
            catch
            {
                startServerDropdown.SelectedIndex = -1;
            }
            finally
            {
                if (startServerDropdown.SelectedIndex == -1)
                {
                    selectedItemTextBlock.Text = "创建一个新的服务器";
                }
            }
        }
        private void startServer_Click(object sender, RoutedEventArgs e)
        {
            if (startServer.IsDropDownOpen)
            {
                return;
            }
            if (startServerDropdown.SelectedIndex == -1)
            {
                var mainwindow = (MainWindow)System.Windows.Window.GetWindow(this);
                System.Windows.Window wn = new forms.CreateServer();
                wn.Owner = mainwindow;
                wn.ShowDialog();
                GetServerConfig();
            }
            else
            {
                MainWindow.serverid = ServerList.serverid[startServerDropdown.SelectedIndex];
                AutoOpenServer();
            }
        }

        private void gotoFrpBtn_Click(object sender, RoutedEventArgs e)
        {
            GotoFrpcEvent();
        }

        private void noticeBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            recommendBorder.Margin = new Thickness(10, noticeBorder.ActualHeight + 20, 10, 80);
        }

        private void startServerDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedItemTextBlock.Text = startServerDropdown.SelectedItem?.ToString();
            JObject _jsonObject = JObject.Parse(File.ReadAllText(@"MSL\config.json", Encoding.UTF8));
            if (_jsonObject["selectedServer"] == null)
            {
                _jsonObject.Add("selectedServer", startServerDropdown.SelectedIndex.ToString());
            }
            else
            {
                _jsonObject["selectedServer"].Replace(startServerDropdown.SelectedIndex.ToString());
            }
            File.WriteAllText(@"MSL\config.json", Convert.ToString(_jsonObject), Encoding.UTF8);
            startServer.IsDropDownOpen = false;
        }
    }
}