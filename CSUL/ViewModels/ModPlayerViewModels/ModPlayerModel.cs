using CSUL.Models;
using CSUL.Models.Local.ModPlayer;
using CSUL.UserControls.DragFiles;
using CSUL.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                if (SelectedPlayer is null)
                {
                    MessageBox.Show("还没有选择播放集", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                foreach (string path in args.Paths) await SelectedPlayer.AddMod(path);
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
