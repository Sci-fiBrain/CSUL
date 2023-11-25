using CSUL.Models;
using Microsoft.Win32;
using SevenZip;
using System;
using System.Collections.Generic;
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
        public class ItemData
        {
            /// <summary>
            /// 地图Id
            /// </summary>
            public string Id { get; set; } = default!;

            /// <summary>
            /// 地图名称
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// 地图路径
            /// </summary>
            public string Path { get; set; } = default!;

            /// <summary>
            /// 最后修改时间
            /// </summary>
            public string LastWriteTime { get; set; } = default!;
        }

        public MapModel()
        {
            //获取初始数据
            RefreshData();

            //设定命令处理方法
            DropCommand = new RelayCommand((sender) =>
            {
                string[] files = EventArgsHandels.DragHandel(sender as DragEventArgs, ".zip", ".rar", ".7z");
                InstallFile(files);
            });
            ClickCommand = new RelayCommand((sender) =>
            {
                OpenFileDialog dialog = new()
                {
                    Title = "选择地图文件",
                    Multiselect = true,
                    Filter = "压缩文件|*.zip;*.rar;*.7z"
                };
                if (dialog.ShowDialog() is true)
                {
                    InstallFile(dialog.FileNames);
                }
            });
            DeleteCommand = new RelayCommand((sender) =>
            {
                if (sender is not ItemData data) return;
                StringBuilder sb = new();
                sb.Append("地图名称: ").Append(data.Name).AppendLine();
                sb.Append("地图ID: ").Append(data.Id).AppendLine();
                sb.Append("最后修改时间: ").Append(data.LastWriteTime).AppendLine();
                sb.Append("地图路径: ").AppendLine().Append(data.Path).AppendLine();
                var ret = MessageBox.Show(sb.ToString(), "删除存档", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (ret == MessageBoxResult.OK)
                {
                    Directory.Delete(data.Path, true);
                    MessageBox.Show("删除成功");
                    RefreshData();
                }
            });
        }

        public ICommand DropCommand { get; }
        public ICommand ClickCommand { get; }
        public ICommand DeleteCommand { get; }

        private List<ItemData> mapData = default!;
        public List<ItemData> MapData
        {
            get => mapData;
            set
            {
                if (mapData == value) return;
                mapData = value;
                OnPropertyChanged();
            }
        }

        #region ---私有方法---
        /// <summary>
        /// 将地图文件夹转为条目信息
        /// </summary>
        private static ItemData ConvertToItemData(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException();
            DirectoryInfo info = new(path);
            ItemData data = new()
            {
                Id = info.Name,
                Name = info.GetFiles().FirstOrDefault(x => x.Name.EndsWith(".MapData"))?.Name.Split('.')[0]
                    ?? "*可能不是地图文件*",
                Path = path,
                LastWriteTime = info.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss"),
            };
            return data;
        }

        /// <summary>
        /// 刷新地图数据
        /// </summary>
        private void RefreshData() => MapData = (from dir in FileManager.Instance.MapDir.GetDirectories() select ConvertToItemData(dir.FullName)).ToList();

        /// <summary>
        /// 安装文件
        /// </summary>
        /// <param name="paths"></param>
        private void InstallFile(string[] paths)
        {
            foreach (string path in paths)
            {
                try
                {
                    if (!File.Exists(path)) continue;
                    SevenZipExtractor zip = new(path);
                    string firstFile = zip.ArchiveFileNames.FirstOrDefault()
                        ?? throw new Exception("该压缩包不含任何文件");
                    string? testFile = zip.ArchiveFileNames.FirstOrDefault(x => x.EndsWith(".MapData"));
                    if (testFile is null)
                    {
                        if (MessageBox.Show("该文件可能不是地图文件，是否继续安装？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.No) continue;
                    }
                    if (FileManager.Instance.MapDir.GetDirectories().Any(x => x.Name == firstFile.Split('\\')[0]))
                    {
                        if (MessageBox.Show("已存在该文件，是否覆盖安装？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.No) continue;
                    }
                    zip.ExtractArchive(FileManager.Instance.MapDir.FullName);
                    MessageBox.Show("安装完成");
                }
                catch (Exception e)
                {
                    MessageBox.Show($"地图{path}安装失败，原因: \n{e.Message}", "安装出错");
                }
            }
            RefreshData();
        }
        #endregion
    }
}
