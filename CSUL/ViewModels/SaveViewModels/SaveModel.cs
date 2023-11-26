using CSUL.Models;
using CSUL.UserControls.DragFiles;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.SaveViewModels
{
    /// <summary>
    /// SaveView的ViewModel
    /// </summary>
    public class SaveModel : BaseViewModel
    {
        public class ItemData
        {
            /// <summary>
            /// 存档Id
            /// </summary>
            public string Id { get; set; } = default!;

            /// <summary>
            /// 存档名称
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// 存档路径
            /// </summary>
            public string Path { get; set; } = default!;

            /// <summary>
            /// 最后修改时间
            /// </summary>
            public string LastWriteTime { get; set; } = default!;
        }

        public SaveModel()
        {
            //获取初始数据
            RefreshData();

            //设定命令处理方法
            DeleteCommand = new RelayCommand((sender) =>
            {
                if (sender is not ItemData data) return;
                StringBuilder sb = new();
                sb.Append("存档名称: ").Append(data.Name).AppendLine();
                sb.Append("存档ID: ").Append(data.Id).AppendLine();
                sb.Append("最后修改时间: ").Append(data.LastWriteTime).AppendLine();
                sb.Append("存档路径: ").AppendLine().Append(data.Path).AppendLine();
                var ret = MessageBox.Show(sb.ToString(), "删除存档", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (ret == MessageBoxResult.OK)
                {
                    Directory.Delete(data.Path, true);
                    MessageBox.Show("删除成功");
                    RefreshData();
                }
            });
            AddCommand = new RelayCommand((sender) =>
            {
                InstallFile((sender as DragFilesEventArgs ?? throw new ArgumentNullException()).Paths);
            });
        }

        public ICommand DeleteCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand OpenFolder { get; } = new RelayCommand((sender) => Process.Start("Explorer.exe", FileManager.Instance.SaveDir.FullName));

        private List<ItemData> saveData = default!;

        public List<ItemData> SaveData
        {
            get => saveData;
            set
            {
                if (saveData == value) return;
                saveData = value;
                OnPropertyChanged();
            }
        }

        #region ---私有方法---

        /// <summary>
        /// 将存档文件夹转为条目信息
        /// </summary>
        private static ItemData ConvertToItemData(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException();
            DirectoryInfo info = new(path);
            ItemData data = new()
            {
                Id = info.Name,
                Name = info.GetFiles().FirstOrDefault(x => x.Name.EndsWith(".cok"))?.Name.Split('.')[0]
                    ?? "*可能不是地图文件*",
                Path = path,
                LastWriteTime = info.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss"),
            };
            return data;
        }

        /// <summary>
        /// 刷新存档数据
        /// </summary>
        private void RefreshData() => SaveData = (from dir in FileManager.Instance.SaveDir.GetDirectories() select ConvertToItemData(dir.FullName)).ToList();

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
                    string? testFile = zip.ArchiveFileNames.FirstOrDefault(x => x.EndsWith(".cok"));
                    if (testFile is null)
                    {
                        if (MessageBox.Show("该文件可能不是存档文件，是否继续安装？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.No) continue;
                    }
                    if (FileManager.Instance.SaveDir.GetDirectories().Any(x => x.Name == firstFile.Split('\\')[0]))
                    {
                        if (MessageBox.Show("已存在该文件，是否覆盖安装？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.No) continue;
                    }
                    zip.ExtractArchive(FileManager.Instance.SaveDir.FullName);
                    MessageBox.Show("安装完成");
                }
                catch (Exception e)
                {
                    MessageBox.Show($"存档{path}安装失败，原因: \n{ExceptionManager.GetExMeg(e)}", "安装出错");
                }
            }
            RefreshData();
        }

        #endregion ---私有方法---
    }
}