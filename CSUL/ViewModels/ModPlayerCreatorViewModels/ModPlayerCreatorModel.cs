using CSUL.Models;
using CSUL.Models.Local;
using CSUL.Models.Local.ModPlayer;
using CSUL.Models.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.ModPlayerCreatorViewModels
{
    internal class ModPlayerCreatorModel : BaseViewModel
    {
        public class BepData
        {
            public string Name { get; set; } = default!;

            public string FileName { get; set; } = default!;

            public string Uri { get; set; } = default!;
        }

        public class PlayerType
        {
            public PlayerType(ModPlayerType playerType)
            {
                Name = playerType.ToString();
                Type = playerType;
            }

            public string Name { get; }

            public ModPlayerType Type { get; }
        }

        private static readonly ComParameters CP = ComParameters.Instance;

        public ModPlayerCreatorModel()
        {
            CreatCommand = new RelayCommand(async sender =>
            {
                if (SelectedPlayerType is null)
                {
                    MessageBox.Show("请选择播放集类型", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (string.IsNullOrWhiteSpace(PlayerName))
                {
                    MessageBox.Show("播放集名称不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string playerPath = Path.Combine(CP.ModPlayers.FullName, PlayerName);
                if (Directory.Exists(playerPath))
                {
                    MessageBox.Show("该名称的播放集已存在", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                try
                {
                    ButtonEnable = false;
                    Directory.CreateDirectory(playerPath);
                    switch (SelectedPlayerType.Type)
                    {   //针对性文件创建
                        case ModPlayerType.BepInEx: await CreatBepPlayer(playerPath); break;
                        case ModPlayerType.Pmod: break;
                        default: throw new Exception("不受支持的播放集类型");
                    }

                    //创建播放器配置文件
                    ModPlayerType playerType = SelectedPlayerType.Type;
                    string config = Path.Combine(playerPath, "modPlayer.config");
                    using Stream configStream = File.Create(config);
                    using Utf8JsonWriter json = new(configStream);
                    json.WriteStartObject();
                    json.WriteString(typeof(ModPlayerType).Name, playerType.ToString());
                    json.WriteEndObject();

                    MessageBox.Show("创建成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    if (Directory.Exists(playerPath)) Directory.Delete(playerPath, true);
                    MessageBox.Show(ex.ToFormative(), "创建失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Window.Close();
            });
        }

        //是否启用按钮
        private bool buttonEnable = true;

        public bool ButtonEnable
        {
            get => buttonEnable;
            set
            {
                if (value == buttonEnable) return;
                buttonEnable = value;
                OnPropertyChanged();
            }
        }

        //播放集名称
        public string PlayerName { get; set; } = "";

        public BepData? SelectedBepData { get; set; }

        private PlayerType? selectedPlayerType;

        public PlayerType? SelectedPlayerType
        {
            get => selectedPlayerType;
            set
            {
                if (value == selectedPlayerType) return;
                if (value is null) return;
                selectedPlayerType = value;
                BepinexVersionVisbility = value.Type == ModPlayerType.BepInEx ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BepinexVersionVisbility));
            }
        }

        public Visibility BepinexVersionVisbility { get; set; } = Visibility.Collapsed;

        public IEnumerable<PlayerType> PlayerTypes { get; }
            = ((ModPlayerType[])Enum.GetValues(typeof(ModPlayerType)))[1..]
                .Select(x => new PlayerType(x));

        //BepInEx文件列表
        public IEnumerable<BepData>? BepDatas { get; } = new Func<IEnumerable<BepData>?>(() =>
        {
            IEnumerable<BepData>? ret = null;
            try
            {
                ret = NetworkData.GetNetBepInfos()?.Select(x => new BepData() { Name = $"{x.Version} {(x.IsBeta ? "测试版" : "正式版")}", Uri = x.Uri, FileName = x.FileName });
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("无法获取在线BepInEx文件信息\n请检查自己的网络或DNS\n或在官方群中下载播放集文件手动安装", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToFormative(), "获取BepInEx文件列表失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return ret;
        }).Invoke();

        //所处的Window
        public Window Window { get; set; } = default!;

        //创建命令
        public ICommand CreatCommand { get; set; }

        private async Task CreatBepPlayer(string playerPath)
        {
            if (SelectedBepData is null)
            {
                MessageBox.Show("还没有选择BepInEx版本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //创建播放集文件夹
            string bepPath = Path.Combine(playerPath, "BepInEx");
            string modPath = Path.Combine(playerPath, "Mods");
            Directory.CreateDirectory(modPath);
            Directory.CreateDirectory(playerPath);

            //下载BepInEx加载器
            using TempDirectory temp = new();
            string zipPath = Path.Combine(temp.FullName, SelectedBepData.FileName);
            using (Stream stream = File.Create(zipPath)) await NetworkData.DownloadFromUri(SelectedBepData.Uri, stream);
            using TempDirectory bepTemp = new();
            await bepTemp.Decompress(zipPath);
            bepTemp.DirectoryInfo.CopyTo(bepPath);
        }
    }
}