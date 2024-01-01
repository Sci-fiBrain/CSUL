using CSUL.Models;
using CSUL.Models.Local;
using CSUL.Models.Local.ModPlayer;
using CSUL.Models.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
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
            public string Name { get; set; } = default!;
        }

        private static readonly ComParameters CP = ComParameters.Instance;

        public ModPlayerCreatorModel()
        {
            CreatCommand = new RelayCommand(async sender =>
            {
                if (chosenBep is null)
                {
                    MessageBox.Show("还没有选择BepInEx版本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    //创建播放集文件夹
                    Directory.CreateDirectory(playerPath);
                    string bepPath = Path.Combine(playerPath, "BepInEx");
                    string modPath = Path.Combine(playerPath, "Mods");
                    Directory.CreateDirectory(modPath);
                    Directory.CreateDirectory(playerPath);
                    ButtonEnable = false;

                    //下载BepInEx加载器
                    using TempDirectory temp = new();
                    string zipPath = Path.Combine(temp.FullName, chosenBep.FileName);
                    using (Stream stream = File.Create(zipPath)) await NetworkData.DownloadFromUri(chosenBep.Uri, stream);
                    using TempDirectory bepTemp = new();
                    await bepTemp.Decompress(zipPath);
                    bepTemp.DirectoryInfo.CopyTo(bepPath);

                    //创建播放器配置文件
                    ModPlayerType playerType = ModPlayerType.BepInEx;
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
            ChooseBepCommand = new RelayCommand(sender =>
            {
                if (sender is not SelectionChangedEventArgs args) return;
                if (args.AddedItems.Count < 1) return;
                if (args.AddedItems[0] is not BepData bepData) return;
                chosenBep = bepData;
            });
        }

        private BepData? chosenBep = null;

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

        //播放集类型列表
        public List<PlayerType> PlayerTypes { get; } = new()
        {
            new(){Name = ModPlayerType.BepInEx.ToString()}
        };

        //BepInEx文件列表
        public IEnumerable<BepData> BepDatas { get; } = NetworkData.GetNetBepInfos().Select(x =>
        {
            return new BepData() { Name = $"{x.Version} {(x.IsBeta ? "测试版" : "正式版")}", Uri = x.Uri, FileName = x.FileName };
        });

        //所处的Window
        public Window Window { get; set; } = default!;

        //创建命令
        public ICommand CreatCommand { get; set; }

        //选择Bep版本命令
        public ICommand ChooseBepCommand { get; set; }
    }
}