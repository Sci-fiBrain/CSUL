using CSUL.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.PlayViewModels
{
    /// <summary>
    /// PlayView的ViewModel
    /// </summary>
    public class PlayModel : BaseViewModel
    {
        private Window? window = null;

        public PlayModel()
        {
            PlayGameCommand = new RelayCommand(
                async (sender) =>
                {
                    ButtonEnabled = false;
                    await Task.Delay(500);
                    if (GameManager.Instance.ShowSteamInfo)
                    {
                        const string steamInfo =
                            "由于天际线2正版验证的问题，启动游戏时可能出现闪退\n" +
                            "闪退属于正常现象，目前CSUL并没有方式避免，还望谅解\n\n" +
                            "解决方案如下:\n" +
                            "1. 等待天际线2窗口自行关闭后，再通过自动弹出的p社启动器进入游戏\n" +
                            "2. 通过Steam进入过一次游戏后，再通过CSUL开始游戏\n" +
                            "3. 配置Steam中天际线2启动参数，跳过p社启动器\n" +
                            "4. 均通过Steam启动游戏，不通过CSUL\n\n" +
                            "确认: 关闭提示\n" +
                            "取消: 关闭提示且不再弹出";
                        MessageBoxResult ret = MessageBox.Show(steamInfo, "Steam游戏提示", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (ret == MessageBoxResult.Cancel) GameManager.Instance.ShowSteamInfo = false;
                    }
                    if (window is not null) window.WindowState = WindowState.Minimized;
                    try
                    {
                        string arg = $"{(OpenDeveloper ? "-developerMode " : null)}{GameManager.Instance.StartArguemnt}";
                        GameManager.StartGame(FileManager.Instance.GamePath!, arg);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ExceptionManager.GetExMeg(ex), "游戏启动出现错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    await Task.Delay(500);
                    ButtonEnabled = true;
                });
        }

        public ICommand PlayGameCommand { get; }

        public bool OpenDeveloper
        {
            get => GameManager.Instance.OpenDeveloper;
            set
            {
                GameManager.Instance.OpenDeveloper = value;
                OnPropertyChanged();
            }
        }

        private bool buttonEnabled = true;

        public bool ButtonEnabled
        {
            get => buttonEnabled;
            set
            {
                if (buttonEnabled == value) return;
                buttonEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 设定当前窗口
        /// </summary>
        /// <param name="window"></param>
        public void SetWindow(Window window) => this.window = window;
    }
}