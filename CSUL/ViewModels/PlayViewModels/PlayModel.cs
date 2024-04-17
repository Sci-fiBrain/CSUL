using CSUL.Models;
using CSUL.Models.Local;
using CSUL.Models.Local.Game;
using CSUL.Models.Local.GameEx;
using CSUL.Models.Local.ModPlayer;
using CSUL.Models.Local.ModPlayer.Pmod;
using CSUL.Models.Network;
using CSUL.Models.Network.CB;
using CSUL.Windows;
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

        private PlayerLogParser? logParser = null;

        public PlayModel()
        {
            PlayGameCommand = new RelayCommand(StartGame);
            RefreshCommand = new RelayCommand(sender =>
            {
                OnPropertyChanged(nameof(SelectedModPlayer));
            });
        }

        public ICommand PlayGameCommand { get; }
        public ICommand RefreshCommand { get; }

#pragma warning disable CA1822

        public string? ChinesizationVersion
        {
            get
            {
                //if (File.Exists(jsName))
                //    return Chinesization.GetVersion(File.ReadAllText(jsName))?.ToString();
                return null;
            }
        }

#pragma warning restore CA1822

        public bool StartChinesization
        {
            get => CP.StartChinesization;
            set
            {
                CP.StartChinesization = value;
                OnPropertyChanged();
            }
        }

        public bool AlwaysReloadPmod
        {
            get => CP.AlwaysReloadPmod;
            set
            {
                CP.AlwaysReloadPmod = value;
                OnPropertyChanged();
            }
        }

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

        private async void StartGame(object? obj)
        {
            await Task.Delay(500);
            if (!File.Exists(Path.Combine(CP.GameRoot.FullName, "Cities2.exe")))
            {
                MessageBox.Show("未找到Cities2.exe文件\n请检查游戏安装目录是否设置正确", "游戏打开失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Process.GetProcessesByName("Cities2") is Process[] processes && processes.Length > 0)
            {
                MessageBoxResult result = MessageBox.Show("检测到正在运行的天际线2进程\n" +
                    "-------------------------------------------\n" +
                    "是: 强制终止已存在的游戏进程，请确保游戏数据已保存\n" +
                    "否: 忽略并继续启动游戏，启动过程可能会出现问题\n" +
                    "取消: 不启动游戏\n",
                    "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {   //终止游戏进程
                    try
                    {
                        foreach (Process process in processes)
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        switch (ex)
                        {
                            case System.ComponentModel.Win32Exception:
                            case UnauthorizedAccessException:
                                {   //权限不足
                                    //配置cmd启动参数
                                    StringBuilder builder = new();
                                    builder.Append("/C ");
                                    foreach (Process process in processes)
                                    {
                                        builder.Append($"taskkill /F /PID {process.Id} & ");
                                    }

                                    ProcessStartInfo startInfo = new()
                                    {   //管理员模式启动cmd 强制终止进程
                                        FileName = "cmd.exe",
                                        Verb = "runas",
                                        Arguments = builder.ToString(),
                                    };
                                    try
                                    {
                                        Process.Start(startInfo);
                                    }
                                    catch (System.ComponentModel.Win32Exception) { }
                                    break;
                                }
                            default: throw;
                        }
                    }
                }
                else if (result == MessageBoxResult.Cancel) return;
            }
            ButtonEnabled = false;
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
            StartInfoBox startInfoBox = new() { Text = "启动游戏" };
            startInfoBox.Show();
            startInfoBox.Topmost = false;
            await Task.Delay(500);

            try
            {
                #region 检查模组是否被占用

                try
                {
                    startInfoBox.Text = "检查Pmod文件占用情况";
                    await Task.Delay(500);
                    FileInfo[] fileInfos = CP.Pmod.GetAllFiles();
                    if (CP.AlwaysReloadPmod || fileInfos.Any(file => file.IsInUse()))
                    {   //模组文件被占用
                        startInfoBox.Text = "解除Pmod文件占用";
                        await GameDataManager.ReloadPmodData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToFormative(), "解除Pmod文件被占用时出现问题", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                #endregion 检查模组是否被占用

                #region 运行播放集

                BaseModPlayer? player = CP.ModPlayerManager.GetModPlayers().FirstOrDefault(x => x.PlayerName == CP.SelectedModPlayer);

                string loadConfig = Path.Combine(CP.GameRoot.FullName, "modPlayer.load");
                if (File.Exists(loadConfig))
                {   //清理旧播放集
                    startInfoBox.Text = "清理旧播放集";
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

                if (player is not null and not NullModPlayer)
                {   //播放集
                    try
                    {
                        //装载播放集
                        startInfoBox.Text = "装载播放集";
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

                #endregion 运行播放集

                #region 运行猫猫工具包

                await LoadNyanChinesization(startInfoBox);
                await LoadNyanTool(184, "喵小夕汉化", "I18nCN", startInfoBox);
                await LoadNyanTool(185, "猫猫快捷键", "mioHotkeysMod", startInfoBox);

                #endregion 运行猫猫工具包

                #region 运行日志解析

                try
                {
                    startInfoBox.Text = "加载日志解析器";
                    logParser?.Dispose();
                    logParser = new(CP.PlayerLog);
                    logParser.StartListening();
                }
                catch (IOException)
                {
                    MessageBox.Show("本次游戏将不会自动解析游戏日志", "游戏日志读取失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                #endregion 运行日志解析

                startInfoBox.Text = "启动游戏进程";
                //启动游戏进程
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
            startInfoBox.Close();
        }

        /// <summary>
        /// 加载猫猫官方模组平台汉化
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private static async Task LoadNyanChinesization(StartInfoBox box)
        {
            try
            {
                if (CP.StartChinesization)
                {   //激活
                    box.Text = $"获取喵小夕模组平台汉化在线信息";
                    CbResourceData? data = await NetworkData.GetCbResourceData(380);
                    CbFileData? fileData = data?.Files?.FirstOrDefault(x => x.FileName.Contains("源码"));
                    if (fileData is null) return;
                    async Task<string?> GetCnText()
                    {   //获取在线汉化数据方法
                        try
                        {
                            using MemoryStream stream = new(fileData.Size);
                            await NetworkData.DownloadFromUri(fileData.Url, stream, api: true);
                            string text = await Task.Run(() => Encoding.UTF8.GetString(stream.ToArray()));
                            return text;
                        }
                        catch
                        {
                            return null;
                        }
                    }

                    if (Chinesization.IsInstalled())
                    {
                        if (!Chinesization.TryGetInstalledVersion(out Version? version)) return;
                        if (!Version.TryParse(data?.ResourceVersion, out Version? latestVersion)) return;
                        if (latestVersion > version)
                        {
                            box.Text = $"更新喵小夕模组平台汉化";
                            await Task.Delay(1000);
                            if (await GetCnText() is not string text) return;
                            Chinesization.RemoveOutdate();
                            Chinesization.Chinesize(text);
                        }
                    }
                    else
                    {
                        box.Text = $"装载喵小夕模组平台汉化";
                        await Task.Delay(1000);
                        if (await GetCnText() is not string text) return;
                        Chinesization.Chinesize(text);
                    }
                }
                else
                {   //不激活
                    if (Chinesization.IsInstalled())
                    {
                        box.Text = $"移除喵小夕模组平台汉化";
                        await Task.Delay(1000);
                        Chinesization.RemoveOutdate();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToFormative(), $"喵小夕模组平台汉化装载出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 加载猫猫工具包
        /// </summary>
        /// <param name="id">资源id</param>
        /// <param name="name">资源名称</param>
        /// <param name="fileName">文件夹名称</param>
        /// <param name="box">显示box</param>
        private static async Task LoadNyanTool(int id, string name, string fileName, StartInfoBox box)
        {
            try
            {
                string path = Path.Combine(CP.Pmod.FullName, fileName);
                if (CP.StartChinesization)
                {
                    CbResourceData? data = null;
                    CbFileData? fileData = null;
                    try
                    {   //获取在线数据
                        box.Text = $"获取{name}在线信息";
                        data = await NetworkData.GetCbResourceData(id);
                        fileData = data?.Files?.FirstOrDefault();
                        if (fileData is null) data = null;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToFormative(), $"{name}最新文件获取失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    PmodData? modData = null;
                    async Task Install()
                    {
                        box.Text = "装载" + name;
                        await Task.Delay(500);
                        using TempDirectory temp = new();
                        string zipPath = Path.Combine(temp.FullName, fileData!.FileName);
                        using (Stream stream = File.Create(zipPath))
                        {
                            await NetworkData.DownloadFromUri(fileData.Url, stream, api: true);
                        }
                        await temp.Decompress(zipPath);
                        temp.DirectoryInfo.GetDirectories().First().CopyTo(path);
                        modData = new(path)
                        {
                            ModVersion = data.ResourceVersion
                        };
                        modData.SaveData();
                    }

                    if (Directory.Exists(path))
                    {   //文件存在 检测更新
                        modData = new(path);
                        if (Version.TryParse(modData.ModVersion, out Version? version) && Version.TryParse(data?.ResourceVersion, out Version? latestVersion))
                        {
                            box.Text = "检查" + name + "更新";
                            await Task.Delay(1000);
                            if (latestVersion > version)
                            {   //需要更新
                                modData.Delete();
                                await Install();
                            }
                        }
                    }
                    else
                    {   //不存在 新安装
                        if (data is not null) await Install();
                    }
                }
                else
                {   //不激活 => 删除
                    if (Directory.Exists(path))
                    {
                        box.Text = $"移除{name}";
                        await Task.Delay(1000);
                        Directory.Delete(path, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToFormative(), $"{name}装载出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}