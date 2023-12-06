using CSUL.Models;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSUL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += (sender, e) =>
            {
                e.Cancel = true;
                ExitProgram(0);
            };

            #region ---异常处理---

            //全局异常捕获
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                StringBuilder builder = new();
                builder.AppendLine("未捕获全局异常");
                builder.AppendLine($"异常类型: {e.ExceptionObject.GetType().Name}");
                builder.AppendLine($"运行状态: {(e.IsTerminating ? "内核即将终止" : "内核正常运行")}");
                builder.AppendLine($"异常对象: {sender?.GetType().FullName ?? "Unknow"}");
                builder.AppendLine($"异常程序: {sender?.GetType().AssemblyQualifiedName ?? "Unknow"}");
                MessageBox.Show(ExceptionManager.GetExMeg(e.ExceptionObject as Exception, builder.ToString())
                    , "未捕获全局异常", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            //Task线程异常捕获
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                StringBuilder builder = new();
                builder.AppendLine("未捕获Task异常");
                builder.AppendLine($"异常类型: {e.Exception.GetType().Name}");
                builder.AppendLine($"异常对象: {sender?.GetType().FullName}");
                builder.AppendLine($"异常程序: {sender?.GetType().AssemblyQualifiedName}");
                MessageBox.Show(ExceptionManager.GetExMeg(e.Exception, builder.ToString())
                    , "未捕获Task异常", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            Dispatcher.UnhandledException += (sender, e) =>
            {
                MessageBox.Show(ExceptionManager.GetExMeg(e.Exception)
                    , "未捕获调度异常", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };

            Dispatcher.UnhandledExceptionFilter += (sender, e) =>
            {
                MessageBox.Show(ExceptionManager.GetExMeg(e.Exception)
                    , "未捕获Filter异常", MessageBoxButton.OK, MessageBoxImage.Error);
                e.RequestCatch = false;
            };

            #endregion ---异常处理---
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
            => ExitProgram(0);

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            Version? latest = await WebManager.GetCsulVersion();
            if (latest is null) return;
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version is null) return;
            if(version < latest)
            {
                StringBuilder builder = new();
                builder.Append("CSUL有新版本更新").AppendLine();
                builder.Append("当前版本: ").Append(version).AppendLine();
                builder.Append("最新版本: ").Append(latest).AppendLine();
                builder.Append("点击确认跳转到CSUL发布页面");
                MessageBoxResult ret = MessageBox.Show(builder.ToString(), "更新提示", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if(ret == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "https://www.cslbbs.net/csul/");
                }
            }
        }

        /// <summary>
        /// 退出程序
        /// </summary>
        private static void ExitProgram(int exitCode)
        {
            MessageBoxResult ret = MessageBox.Show("确定退出吗？", "提示",
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (ret == MessageBoxResult.OK)
            {
                FileManager.Instance.Dispose();
                GameManager.Instance.Dispose();
                Environment.Exit(exitCode);
            }
        }
    }
}