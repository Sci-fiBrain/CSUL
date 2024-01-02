using CSUL.Models;
using CSUL.Models.Local;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
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
        private readonly CancellationTokenSource cancellation = new();
        private readonly Mutex mutex;

        #region ---构造函数---

        public MainWindow()
        {
            #region --基础代码初始化--

            if (File.Exists("Cities2.exe"))
            {
                MessageBox.Show("为避免未知原因导致出错及便于CSUL更新\n" +
                    "请不要将CSUL安装在游戏根目录\n" +
                    "请更改CSUL安装位置后重试\n", "CSUL安装位置错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception("请不要将CSUL安装在游戏根目录");
            }

            CatchException(Dispatcher);
            mutex = new(true, "CSUL", out bool mainProcess);

            #region -主进程-

            if (mainProcess)
            {   //主进程
                if (TryCreatRegisterFile(out string reg))
                {   //创建注册表
                    MessageBoxResult ret = MessageBox.Show("为实现论坛内容自动下载安装，需要创建一个注册表添加自定义协议\n" +
                        "在此保证该注册表内容安全，如有疑虑可查看源码\n" +
                        "是否同意创建?", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (ret == MessageBoxResult.Yes)
                    {
                        ProcessStartInfo startInfo = new() { UseShellExecute = false, CreateNoWindow = true, FileName = "cmd", Arguments = $"/c \"{reg}\"" };
                        Process.Start(startInfo)?.WaitForExit();
                    }
                }
                Task.Run(() => PipeReader(cancellation.Token), cancellation.Token);
            }

            #endregion -主进程-

            #region -副进程-

            else
            {   //副进程
                try
                {
                    string[] args = Environment.GetCommandLineArgs();
                    if (args.Length > 1)
                    {
                        using NamedPipeClientStream pipe = new(".", "CSUL_Pipe", PipeDirection.Out);
                        try
                        {   //向主进程传参
                            pipe.Connect(TimeSpan.FromSeconds(5));
                            string info = string.Join(" ", args[1..]);
                            byte[] buffer = info.StartsWith("csul://")
                                ? System.Web.HttpUtility.UrlDecodeToBytes(info[7..])
                                : Encoding.UTF8.GetBytes(info);
                            pipe.Write(buffer, 0, buffer.Length);
                            pipe.Close();
                        }
                        catch (TimeoutException) { }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToFormative(), "传参时出现问题", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                mutex.Dispose();
                Environment.Exit(0);
            }

            #endregion -副进程-

            ComParameters cp = new(GetBasePath);

            #endregion --基础代码初始化--

            #region --UI代码初始化--

            InitializeComponent();
            Closing += (sender, e) =>
            {
                e.Cancel = true;
                ExitProgram(0);
            };

            #endregion --UI代码初始化--
        }

        #endregion ---构造函数---

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
        private void ExitProgram(int exitCode)
        {
            MessageBoxResult ret = MessageBox.Show("确定退出吗？", "提示",
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (ret == MessageBoxResult.OK)
            {
                cancellation.Cancel();
                cancellation.Dispose();
                ComParameters.Instance.Dispose();
                mutex.Dispose();
                Environment.Exit(exitCode);
            }
        }

        #endregion ---UI方法---

        #region ---私有方法---

        /// <summary>
        /// 尝试创建注册表文件
        /// </summary>
        /// <param name="regPath">注册表文件的路径</param>
        /// <returns>是否创建成功</returns>
        private static bool TryCreatRegisterFile(out string regPath)
        {
            regPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSUL.reg");
            if (File.Exists(regPath)) return false;
            using StreamWriter stream = new(File.Create(regPath), Encoding.Unicode);
            stream.WriteLine("Windows Registry Editor Version 5.00");
            stream.WriteLine();
            stream.WriteLine("[HKEY_CLASSES_ROOT\\csul]");
            stream.WriteLine("@=\"URL:csul\"");
            stream.WriteLine("\"URL Protocol\"=\"\"");
            stream.WriteLine();
            stream.WriteLine("[HKEY_CLASSES_ROOT\\csul\\shell]");
            stream.WriteLine();
            stream.WriteLine("[HKEY_CLASSES_ROOT\\csul\\shell\\open]");
            stream.WriteLine();
            stream.WriteLine("[HKEY_CLASSES_ROOT\\csul\\shell\\open\\command]");
            stream.WriteLine($"@=\"\\\"{System.Windows.Forms.Application.ExecutablePath.Replace("\\", "\\\\")}\\\" \\\"%1\\\"\"");
            return true;
        }

        /// <summary>
        /// 管道读取方法
        /// </summary>
        private async Task PipeReader(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using NamedPipeServerStream pipe = new("CSUL_Pipe", PipeDirection.In);
                try
                {
                    await pipe.WaitForConnectionAsync(token);
                    using MemoryStream stream = new();
                    while (pipe.IsConnected)
                    {
                        try
                        {
                            byte[] buffer = new byte[512];
                            int size = await pipe.ReadAsync(buffer, token);
                            stream.Write(buffer, 0, size);
                        }
                        catch (IOException) { break; }
                    }
                    string command = Encoding.UTF8.GetString(stream.ToArray());
                    await ViceCommand.Parse(command, Dispatcher);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToFormative(), "参数处理出现错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static (string? root, string? data) GetBasePath()
        {
            string? root = null;
            if (Cities2Path.TryFromSteam(out string? fromSteam)) root = fromSteam;
            else if (Cities2Path.TryFromMicrosoft(out string? fromMicrosoft)) root = fromMicrosoft;
            else if (Cities2Path.TryFromXbox(out string? fromXbox)) root = fromXbox;

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