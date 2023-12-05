using CSUL.Models;
using CSUL.UserControls.DragFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.MapViewModels
{
    /// <summary>
    /// MapView的ViewModel
    /// </summary>
    public class MapModel : BaseViewModel
    {
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
                var ret = MessageBox.Show(sb.ToString(), "删除存档", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (ret == MessageBoxResult.OK)
                {
                    try
                    {
                        data.Delete();
                        MessageBox.Show("删除成功");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ExceptionManager.GetExMeg(ex), "文件删除失败",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    RefreshData();
                }
            });
            AddCommand = new RelayCommand(async (sender) =>
            {
                await InstallFile((sender as DragFilesEventArgs ?? throw new ArgumentNullException()).Paths);
            });
            Refresh = new RelayCommand(sender => RefreshData());
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenFolder { get; } = new RelayCommand((sender) => Process.Start("Explorer.exe", FileManager.Instance.MapDir.FullName));
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
        private void RefreshData() => GameData = from cid in FileManager.Instance.MapDir.GetAllFiles()
                                                 where cid.Name.EndsWith(".cok")
                                                 let data = new GameDataFileInfo(cid.FullName)
                                                 select data;

        /// <summary>
        /// 安装文件
        /// </summary>
        /// <param name="paths"></param>
        private async Task InstallFile(string[] paths)
        {
            foreach (string path in paths)
            {
                try
                {
                    Models.Structs.InstalledGameDataFiles ret = await FileManager.Instance.InstallGameDataFile(path);
                    StringBuilder builder = new();
                    builder.Append($"文件{Path.GetFileName(path)}解析完成").AppendLine();
                    builder.Append($"已安装地图 {ret.MapNames.Count} 个: ").AppendLine();
                    ret.MapNames.ForEach(x => builder.AppendLine(x));
                    builder.AppendLine();
                    builder.Append($"已安装存档 {ret.SaveNames.Count} 个: ").AppendLine();
                    ret.SaveNames.ForEach(x => builder.AppendLine(x));
                    MessageBox.Show(builder.ToString(), $"提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ExceptionManager.GetExMeg(ex, $"文件{Path.GetFileName(path)}安装失败"), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            RefreshData();
        }

        #endregion ---私有方法---
    }
}