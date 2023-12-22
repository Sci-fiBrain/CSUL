using CSUL.Models;
using CSUL.Models.Local;
using CSUL.Models.Local.ModPlayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        private static ComParameters CP { get; } = ComParameters.Instance;
        private Window? window = null;

        public PlayModel()
        {
            PlayGameCommand = new RelayCommand(
                async (sender) =>
                {
                    ButtonEnabled = false;
                    await Task.Delay(500);
                    if (CP.ShowSteamInfo)
                    {
                        const string steamInfo =
                            "由于天际线2正版验证的问题，启动游戏时可能出现闪退\n" +
                            "闪退属于正常现象，目前CSUL并没有方式避免，还望谅解\n\n" +
                            "解决方案如下:\n" +
                            "1. 打开Steam正版兼容模式，再启动游戏\n" +
                            "2. 等待天际线2窗口自行关闭后，再通过自动弹出的p社启动器进入游戏\n" +
                            "3. 通过Steam进入过一次游戏后，再通过CSUL开始游戏\n" +
                            "4. 配置Steam中天际线2启动参数，跳过p社启动器\n" +
                            "5. 均通过Steam启动游戏，不通过CSUL(该方式不支持播放集)\n\n" +
                            "确认: 关闭提示\n" +
                            "取消: 关闭提示且不再弹出";
                        MessageBoxResult ret = MessageBox.Show(steamInfo, "Steam游戏提示", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (ret == MessageBoxResult.Cancel) CP.ShowSteamInfo = false;
                    }
                    if (window is not null) window.WindowState = WindowState.Minimized;
                    try
                    {
                        BaseModPlayer? player = CP.ModPlayerManager.GetModPlayers().FirstOrDefault(x => x.PlayerName == CP.SelectedModPlayer);
                        if (player is not null and not NullModPlayer)
                        {   //装载播放集
                            try
                            {
                                string loadConfig = Path.Combine(CP.GameRoot.FullName, "modPlayer.load");
                                if (File.Exists(loadConfig))
                                {   //清理旧播放集
                                    ModPlayerData? data = JsonSerializer.Deserialize<ModPlayerData>(File.ReadAllText(loadConfig));
                                    if (data is not null)
                                    {
                                        if (data.Files is IEnumerable<string> files)
                                            foreach (string file in files) if (File.Exists(file)) File.Delete(file);
                                        if (data.Directories is IEnumerable<string> dirs)
                                            foreach (string dir in dirs) if (Directory.Exists(dir)) Directory.Delete(dir, true);
                                    }
                                    File.Delete(loadConfig);
                                }

                                //装载播放集
                                ModPlayerData playerData = await player.Install(CP.GameRoot.FullName, CP.GameData.FullName);
                                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(playerData));
                                using Stream stream = File.Create(loadConfig);
                                await stream.WriteAsync(buffer);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToFormative(), "播放集装载出错", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }

                        //启动游戏
                        string arg = $"{(OpenDeveloper ? "-developerMode " : null)}{CP.StartArguemnt}";
                        if (CP.SteamCompatibilityMode)
                        {   //Steam正版兼容模式
                            string? steamPath = null;
                            if (CP.SteamPath is string path && File.Exists(path)) steamPath = path;
                            else if (!Cities2Path.TryGetSteamPath(out steamPath)) throw new FileNotFoundException("Steam.exe未找到，请检查Steam路径设置");
                            Process.Start(steamPath, $"-applaunch 949230 {arg}");
                        }
                        else Process.Start(Path.Combine(CP.GameRoot.FullName, "Cities2.exe"), arg);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToFormative(), "游戏启动出现错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    await Task.Delay(500);
                    ButtonEnabled = true;
                });
            RefreshCommand = new RelayCommand(sender =>
            {
                OnPropertyChanged(nameof(SelectedModPlayer));
            });
        }

        public ICommand PlayGameCommand { get; }
        public ICommand RefreshCommand { get; }

        public bool OpenDeveloper
        {
            get => CP.OpenDeveloper;
            set
            {
                CP.OpenDeveloper = value;
                OnPropertyChanged();
            }
        }

        public bool SteamCompatibilityMode
        {
            get => CP.SteamCompatibilityMode;
            set
            {
                CP.SteamCompatibilityMode = value;
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

#pragma warning disable CA1822
        public string? SelectedModPlayer { get => CP.SelectedModPlayer; }
#pragma warning restore CA1822

        /// <summary>
        /// 设定当前窗口
        /// </summary>
        /// <param name="window"></param>
        public void SetWindow(Window window) => this.window = window;
    }
}