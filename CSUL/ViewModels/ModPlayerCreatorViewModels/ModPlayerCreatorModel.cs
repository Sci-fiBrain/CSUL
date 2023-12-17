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
                    Directory.CreateDirectory(playerPath);
                    string bepPath = Path.Combine(playerPath, "BepInEx");
                    string modPath = Path.Combine(playerPath, "Mods");
                    Directory.CreateDirectory(modPath);
                    Directory.CreateDirectory(playerPath);
                    ButtonEnable = false;
                    using TempDirectory temp = new();
                    string zipPath = Path.Combine(temp.FullName, chosenBep.FileName);
                    using (Stream stream = File.Create(zipPath)) await NetworkData.DownloadFromUri(chosenBep.Uri, stream);
                    using TempDirectory bepTemp = new();
                    await bepTemp.Decompress(zipPath);
                    bepTemp.DirectoryInfo.CopyTo(bepPath);
                    ModPlayerType playerType = ModPlayerType.BepInEx;
                    string config = Path.Combine(playerPath, "modPlayer.config");
                    using Stream configStream = File.Create(config);
                    using Utf8JsonWriter json = new(configStream);
                    json.WriteStartObject();
                    json.WriteString(typeof(ModPlayerType).Name, playerType.ToString());
                    json.WriteEndObject();
                    MessageBox.Show("创建成功");
                }
                catch (Exception ex)
                {
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

        public string PlayerName { get; set; } = "";

        public List<PlayerType> PlayerTypes { get; } = new()
        {
            new(){Name = ModPlayerType.BepInEx.ToString()}
        };

        public IEnumerable<BepData> BepDatas { get; } = NetworkData.GetNetBepInfos().Select(x =>
        {
            return new BepData() { Name = $"{x.Version} {(x.IsBeta ? "测试版" : "正式版")}", Uri = x.Uri, FileName = x.FileName };
        });

        public Window Window { get; set; } = default!;

        public ICommand CreatCommand { get; set; }
        public ICommand ChooseBepCommand { get; set; }
    }
}