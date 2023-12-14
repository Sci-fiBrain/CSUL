using CSUL.Models;
using CSUL.Models.Local;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CSUL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //基础代码初始化
            CatchException(Dispatcher);
            ComParameters cp = new(GetBasePath);

            //UI代码初始化
            InitializeComponent();
            Closing += (sender, e) =>
            {
                e.Cancel = true;
                ExitProgram(0);
            };
        }

        #region ---UI方法---

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

        /// <summary>
        /// 退出程序
        /// </summary>
        private static void ExitProgram(int exitCode)
        {
            MessageBoxResult ret = MessageBox.Show("确定退出吗？", "提示",
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (ret == MessageBoxResult.OK)
            {
                ComParameters.Instance.Dispose();
                Environment.Exit(exitCode);
            }
        }

        #endregion ---UI方法---

        #region ---私有方法---

        private static (string? root, string? data) GetBasePath()
        {
            string? root = null;
            if (Cities2Path.TryFromSteam(out string? fromSteam)) root = fromSteam;
            else if(Cities2Path.TryFromMicrosoft(out string? fromMicrosoft)) root = fromMicrosoft;
            else if(Cities2Path.TryFromXbox(out string? fromXbox)) root = fromXbox;

            Cities2Path.TryGetGameDataPath(out string? gameDataPath);
            return (root, gameDataPath);
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        private static void CatchException(Dispatcher dispatcher)
        {
            //全局异常捕获
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                StringBuilder builder = new();
                builder.AppendLine("未捕获全局异常");
                builder.AppendLine($"异常类型: {e.ExceptionObject.GetType().Name}");
                builder.AppendLine($"运行状态: {(e.IsTerminating ? "内核即将终止" : "内核正常运行")}");
                builder.AppendLine($"异常对象: {sender?.GetType().FullName ?? "Unknow"}");
                builder.AppendLine($"异常程序: {sender?.GetType().AssemblyQualifiedName ?? "Unknow"}");
                MessageBox.Show((e.ExceptionObject as Exception).ToFormative(builder.ToString())
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
                MessageBox.Show(e.Exception.ToFormative(builder.ToString())
                    , "未捕获Task异常", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            dispatcher.UnhandledException += (sender, e) =>
            {
                MessageBox.Show(e.Exception.ToFormative()
                    , "未捕获调度异常", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };

            dispatcher.UnhandledExceptionFilter += (sender, e) =>
            {
                MessageBox.Show(e.Exception.ToFormative()
                    , "未捕获Filter异常", MessageBoxButton.OK, MessageBoxImage.Error);
                e.RequestCatch = false;
            };
        }

        #endregion ---私有方法---
    }
}