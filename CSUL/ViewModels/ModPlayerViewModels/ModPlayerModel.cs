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
            manager = new(CP.ModPlayers.FullName);
            manager.OnDataChanged += (sender, e) => RefreshData();
            CP.ModPlayerManager = manager;
            CreatNewModPlayerCommand = new RelayCommand(sender =>
            {
                ModPlayerCteator playerCteator = new();
                playerCteator.ShowDialog();
            });
            ComboSelectedCommand = new RelayCommand(sender =>
            {
                if (sender is not SelectionChangedEventArgs args) return;
                if (args.AddedItems.Count < 1) return;
                if (args.AddedItems[0] is not BaseModPlayer player) return;
                SelectedPlayer = player;
            });
            AddModCommand = new RelayCommand(sender =>
            {
                if (sender is not DragFilesEventArgs args) return;
                if(selectedPlayer is null)
                {
                    MessageBox.Show("还没有选择播放集", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                foreach (string path in args.Paths) selectedPlayer?.AddMod(path);
            });
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

        private BaseModPlayer? selectedPlayer;
        public BaseModPlayer? SelectedPlayer
        {
            get => selectedPlayer;
            set
            {
                if (selectedPlayer == value) return;
                selectedPlayer = value;
                OnPropertyChanged();
            }
        }

        public DragFilesType FilesType { get; } = DefaultDragFilesType.BepModFile;
        public ICommand CreatNewModPlayerCommand { get; }
        public ICommand AddModCommand { get; }
        public ICommand ComboSelectedCommand { get; }

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void RefreshData()
        {
            Players = CP.ModPlayerManager.GetModPlayers();
        }
    }
}
