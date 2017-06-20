using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using MFUpdater;

namespace AutoUpdateWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (AutoUpdater.Instance.versionInfo != null)
            {
                //this.lblInfoShower.Text = AutoUpdater.Instance.versionInfo.VersionName + "版本更新\r\n\r\n";
                //AutoUpdater.Instance.versionInfo.Description = "1.修改了部分bug。$$2.修复了闪退功能。$$3.增加了界面设置$$2.修复了闪退功能。$$3.增加了界面设置$$2.修复了闪退功能。$$3.增加了界面设置$$2.修复了闪退功能。$$3.增加了界面设置";
                string[] stringList = AutoUpdater.Instance.versionInfo.Description.Split(new string[] { "$$"}, StringSplitOptions.RemoveEmptyEntries);
                foreach(var item in stringList)
                {
                    this.lblInfoShower.Text += item + "\r\n";
                }
                //this.lblInfoShower.Text += AutoUpdater.Instance.versionInfo.Description + "\r\n";
                this.lblVersion.Content = AutoUpdater.Instance.versionInfo.VersionName;
            }
          

            AutoUpdater.Instance.AutoUpdateEvent += AutoUpdateCallback;
            AutoUpdater.Instance.RunTimeInfoEvent += RunTimeInfoCallback;
            this.gdMain.Visibility = Visibility.Visible;
            this.gdResult.Visibility = Visibility.Collapsed;
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
           System.Windows.Application.Current.Shutdown();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.KillMainAplication();
            //this.lblInfoShower.Text = string.Empty;
            this.progressBar.Visibility = Visibility.Visible;
            this.gdMain.Visibility = Visibility.Collapsed;
            this.gdResult.Visibility = Visibility.Visible;
            this.tbUpdating.Visibility = Visibility.Visible;
            this.tbUpdateOver.Visibility = Visibility.Collapsed;
            //this.btnLater.IsEnabled = false;
            //this.btnUpdate.IsEnabled = false;
            Thread thd = new Thread(AutoUpdater.Instance.StartZipUpdate);
            thd.IsBackground = true;
            thd.Start();
        }
        private void BtnLater_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #region"对内功能函数"
        /// <summary>
        /// 关闭更新主程序进程
        /// </summary>
        private void KillMainAplication()
        {
            Process[] pro = Process.GetProcessesByName(AutoUpdater.Instance.mainProcessInfo.MainProcessName);
            if (pro == null)
                return;
            foreach (var item in pro)
            {
                item.Kill();
            }
        }
        #endregion

        private void AutoUpdateCallback(object sender, AutoUpdateEventArgs e)
        {
            Action method = delegate
            {
                //switch (e.ProgressBarStyle)
                //{
                //    case ProgressBarStateEnum.UnEnable:
                //        this.progressBar.Visibility = System.Windows.Visibility.Visible;
                //        break;
                //    case ProgressBarStateEnum.Continue:
                //        this.progressBar.Visibility = System.Windows.Visibility.Hidden;
                //        this.progressBar.IsIndeterminate = false;
                //        break;
                //    case ProgressBarStateEnum.Marquee:
                //        this.progressBar.Visibility = System.Windows.Visibility.Visible;
                //        this.progressBar.IsIndeterminate = true;
                //        break;
                //}
                if (e.TotleSize != 0)
                {
                    this.progressBar.Value = e.UpdateSize * 100 / e.TotleSize;
                }
                //this.lblInfoShower.Text += e.Info + "\r\n";
                this.tbUpdating.Text = e.Info;
            };
            this.Dispatcher.Invoke(method);

        }

        private void RunTimeInfoCallback(object sender, RunTimeStateArgs e)
        {
            Action method = delegate
            {
                this.progressBar.Visibility = Visibility.Hidden;
                //if (System.Windows.MessageBox.Show(e.Info, "提示", MessageBoxButton.OK) == MessageBoxResult.OK)
                //{
                //    Process.Start(AutoUpdater.Instance.mainProcessInfo.UpdateApplicationEntryAssembly);
                //    System.Windows.Application.Current.Shutdown();
                //}
                this.progressBar.Visibility = Visibility.Collapsed;
                this.gdMain.Visibility = Visibility.Collapsed;
                this.gdResult.Visibility = Visibility.Visible;
                this.tbUpdating.Visibility = Visibility.Collapsed;
                this.tbUpdateOver.Visibility = Visibility.Visible;
                Process.Start(AutoUpdater.Instance.mainProcessInfo.UpdateApplicationEntryAssembly);
                System.Windows.Application.Current.Shutdown();

            };
            this.Dispatcher.Invoke(method);



        }

        private void gdMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
