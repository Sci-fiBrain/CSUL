using CSUL.Models;
using CSUL.Models.Local.ModPlayer;
using CSUL.Models.Local.ModPlayer.BepInEx;
using CSUL.UserControls.DragFiles;
using CSUL.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.ModPlayerViewModels
{
    internal class ModPlayerModel : BaseViewModel
    {
        private static readonly ComParameters CP = ComParameters.Instance;
        private readonly ModPlayerManager manager;

        public ModPlayerModel()
        {
            manager = CP.ModPlayerManager;
            CreatNewModPlayerCommand = new RelayCommand(sender =>
            {   //创建播放集
                ModPlayerCteator playerCteator = new();
                playerCteator.ShowDialog();
                manager.ReloadPlayers();
                RefreshData();
            });
            AddModCommand = new RelayCommand(async sender =>
            {   //添加模组
                if (sender is not DragFilesEventArgs args) return;
                if (SelectedPlayer is null or NullModPlayer)
                {
                    MessageBox.Show("还没有选择播放集\n请选择或创建一个播放集", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                foreach (string path in args.Paths) await SelectedPlayer.AddMod(path);
                RefreshData();
            });
            DeleteModCommand = new RelayCommand(sender =>
            {   //删除模组
                if (sender is not IModData mod) return;
                if (SelectedPlayer is null or NullModPlayer)
                {
                    MessageBox.Show("还没有选择播放集\n请选择或创建一个播放集", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (MessageBox.Show(mod.Name, "确定删除", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return;
                SelectedPlayer.RemoveMod(mod);
                RefreshData();
            });
            CheckCommand = new RelayCommand(sender =>
            {   //检查模组兼容性
                if (SelectedPlayer is not BepModPlayer player)
                {
                    MessageBox.Show("还没有选择播放集\n或该播放集不是BepInEx播放集\n请选择或创建一个BepInEx播放集", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                Dictionary<int, (string name, string version)> allData = new();
                List<int> pass = new(), wrong = new(), unknow = new();
                BepModData[] mods = (BepModData[])player.ModDatas;
                for (int i = 0; i < mods.Length; i++)
                {
                    BepModData mod = mods[i];
                    try
                    {
                        if (mod.BepVersion is null)
                        {
                            unknow.Add(i);
                        }
                        else if (mod.BepVersion.Major == player.PlayerVersion?.Major)
                        {
                            pass.Add(i);
                        }
                        else
                        {
                            wrong.Add(i);
                        }
                        allData[i] = (mods[i].Name, mods[i].BepVersion?.ToString() ?? "Unknow");
                    }
                    catch
                    {
                        unknow.Add(i);
                        allData[i] = (mods[i].Name, "Unknow");
                    }
                }
                ModCompatibilityBox.ShowBox(allData, pass, wrong, unknow);
            });
            OpenFolderCommand = new RelayCommand(sender =>
            {
                if (SelectedPlayer is null or NullModPlayer)
                {
                    MessageBox.Show("还没有选择播放集\n请选择或创建一个播放集", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                Process.Start("Explorer.exe", SelectedPlayer.PlayerPath);
            });
            RemoveModPlayerCommand = new RelayCommand(sender =>
            {
                if (SelectedPlayer is null or NullModPlayer)
                {
                    MessageBox.Show("还没有选择播放集\n请选择或创建一个播放集", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (MessageBox.Show($"将移除播放集 {SelectedPlayer.PlayerName}\n" +
                    $"以及其包含的所有模组\n" +
                    $"是否继续?", "确定移除播放集吗?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) return;
                Directory.Delete(SelectedPlayer.PlayerPath, true);
                SelectedPlayer = Players.FirstOrDefault();
                manager.ReloadPlayers();
                RefreshData();
            });
            RefreshCommand = new RelayCommand(sender => RefreshData());
            RefreshData();
        }

        private List<BaseModPlayer> players = default!;

        public List<BaseModPlayer> Players
        {
            get => players;
            set
            {
                if (players == value) return;
                players = value;
                OnPropertyChanged();
            }
        }

        public BaseModPlayer? SelectedPlayer
        {
            get => Players.FirstOrDefault(x => x.PlayerName == CP.SelectedModPlayer) ?? Players.FirstOrDefault();
            set
            {
                if (SelectedPlayer == value || value is null) return;
                CP.SelectedModPlayer = value.PlayerName;
                OnPropertyChanged();
            }
        }

        public DragFilesType FilesType { get; } = DefaultDragFilesType.BepModFile;

        public ICommand CreatNewModPlayerCommand { get; }
        public ICommand AddModCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteModCommand { get; }
        public ICommand CheckCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand RemoveModPlayerCommand { get; }

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void RefreshData()
        {
            Players = CP.ModPlayerManager.GetModPlayers();
            OnPropertyChanged(nameof(SelectedPlayer));
        }
    }
}