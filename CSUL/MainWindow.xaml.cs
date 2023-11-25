using CSUL.Models;
using System;
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
            SevenZip.SevenZipBase.SetLibraryPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DLL",
                IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
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

        /// <summary>
        /// 退出程序
        /// </summary>
        private static void ExitProgram(int exitCode)
        {
            MessageBoxResult ret = MessageBox.Show("确定退出吗？", "提示",
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (ret == MessageBoxResult.OK)
            {
                FileManager.Instance.SaveConfig(FileManager.ConfigPath);
                Environment.Exit(exitCode);
            }
        }
    }
}