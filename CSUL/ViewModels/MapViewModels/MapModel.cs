using CSUL.Models;
using CSUL.Models.Local.Game;
using CSUL.UserControls.DragFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.MapViewModels
{
    /// <summary>
    /// MapView的ViewModel
    /// </summary>
    public class MapModel : BaseViewModel
    {
        private static ComParameters CP { get; } = ComParameters.Instance;

        public MapModel()
        {
            //获取初始数据
            RefreshData();

            //设定命令处理方法
            DeleteCommand = new RelayCommand((sender) =>
            {
                if (sender is not GameDataFileInfo data) return;
                StringBuilder sb = new();
                sb.Append("地图名称: ").Append(data.Name).AppendLine();
                sb.Append("地图ID: ").Append(data.Cid).AppendLine();
                sb.Append("最后修改时间: ").Append(data.LastWriteTime).AppendLine();
                sb.Append("地图路径: ").AppendLine().Append(data.FilePath).AppendLine();
                MessageBoxResult ret = MessageBox.Show(sb.ToString(), "删除存档", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (ret == MessageBoxResult.OK)
                {
                    try
                    {
                        data.Delete();
                        MessageBox.Show("删除成功");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToFormative(), "文件删除失败",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    RefreshData();
                }
            });
            AddCommand = new RelayCommand(async (sender) =>
            {
                string[] paths = (sender as DragFilesEventArgs ?? throw new ArgumentNullException()).Paths;
                await GameDataFile.Install(paths);
                RefreshData();
            });
            Refresh = new RelayCommand(sender => RefreshData());
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenFolder { get; } = new RelayCommand((sender) => Process.Start("Explorer.exe", CP.Maps.FullName));
        public ICommand Refresh { get; }

        private IEnumerable<GameDataFileInfo> gameData = default!;

        public IEnumerable<GameDataFileInfo> GameData
        {
            get => gameData;
            set
            {
                if (gameData == value) return;
                gameData = value;
                OnPropertyChanged();
            }
        }

        #region ---私有方法---

        /// <summary>
        /// 刷新地图数据
        /// </summary>
        private void RefreshData() => GameData = from cok in CP.Maps.GetAllFiles()
                                                 where cok.Name.EndsWith(".cok")
                                                 let data = new GameDataFileInfo(cok.FullName)
                                                 select data;

        #endregion ---私有方法---
    }
}