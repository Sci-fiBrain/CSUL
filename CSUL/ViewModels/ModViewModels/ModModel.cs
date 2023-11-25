using CSUL.Models;
using CSUL.UserControls.DragFiles;
using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CSUL.ViewModels.ModViewModels
{
    public class ModModel : BaseViewModel
    {
        #region ---模组信息条目---
        public class ItemData
        {
            /// <summary>
            /// 模组名称
            /// </summary>
            public string Name { get; set; } = default!;

            /// <summary>
            /// 模组路径
            /// </summary>
            public string Path { get; set; } = default!;

            /// <summary>
            /// 最后修改时间
            /// </summary>
            public string LastWriteTime { get; set; } = default!;

            /// <summary>
            /// 删除插件的方法
            /// </summary>
            public Action Delete { get; set; } = default!;
        }
        #endregion

        #region ---BepInEx信息条目---
        public class BepItemData
        {
            public string Name { get; set; } = default!;

            public string Version { get; set; } = default!;

            public Brush BackBrush { get; set; } = default!;

            public string Uri { get; set; } = default!;
            #endregion
        }

        public ModModel()
        {
            DeleteCommand = new RelayCommand((sender) =>
            {
                if (sender is not ItemData data) return;
                StringBuilder sb = new();
                sb.Append("模组名称: ").Append(data.Name).AppendLine();
                sb.Append("最后修改时间: ").Append(data.LastWriteTime).AppendLine();
                sb.Append("模组路径: ").AppendLine().Append(data.Path).AppendLine();
                var ret = MessageBox.Show(sb.ToString(), "删除存档", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (ret == MessageBoxResult.OK)
                {
                    data.Delete();
                    MessageBox.Show("删除成功");
                    RefreshData();
                }
            });
            AddCommand = new RelayCommand((sender) =>
            {
                InstallFile((sender as DragFilesEventArgs ?? throw new ArgumentNullException()).Paths);
            });
            DownloadCommand = FileManager.Instance.NoBepInEx ?
            new RelayCommand(async (sender) =>
            {
                try
                {
                    if (sender is not BepItemData data) return;
                    if (MessageBox.Show(data.Version, "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                    string path = Path.Combine(FileManager.TempDirPath, data.Name);
                    FileInfo file = new(path);
                    if (file.Directory?.Exists is true) file.Directory.Delete(true);
                    if (file.Exists) file.Delete();
                    if (file.Directory?.Exists is false) file.Directory?.Create();
                    await WebManager.DownloadFromUri(data.Uri, path);
                    SevenZipExtractor zip = new(path);
                    RemoveBepInEx();
                    await zip.ExtractArchiveAsync(FileManager.Instance.GameDataDir.FullName);
                    ShowNoEx = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;
                    MessageBox.Show("安装完成");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "安装失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }) : default!;
            RefreshData();
        }

        private Visibility showNoEx = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ShowNoEx
        {
            get => showNoEx;
            set
            {
                if(showNoEx == value) return;
                showNoEx = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DownloadCommand { get; }

        public List<BepItemData>? BepData { get; } = FileManager.Instance.NoBepInEx ?
            (from item in WebManager.GetBepinexInfos()
             select new BepItemData
             {
                 Uri = item.Uri,
                 Version = $"安装 {item.Version} {(item.IsBeta ? "测试版" : "正式版")}",
                 Name = item.FileName,
                 BackBrush = new SolidColorBrush(Color.FromRgb(242, 242, 242))
             }).ToList() : null;

        private List<ItemData> modData = default!;

        public List<ItemData> ModData
        {
            get => modData;
            set
            {
                if (modData == value) return;
                modData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void RefreshData()
        {
            List<ItemData> items = new();
            FileInfo[] files = FileManager.Instance.ModDir.GetFiles().Where(x => x.Name.EndsWith(".dll")).ToArray();
            items.AddRange(from file in files
                           select new ItemData()
                           {    //添加单文件的插件
                               Name = file.Name,
                               Path = file.FullName,
                               LastWriteTime = file.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss"),
                               Delete = file.Delete
                           });
            DirectoryInfo[] dirs = FileManager.Instance.ModDir.GetDirectories();
            items.AddRange(from dir in dirs
                           select new ItemData()
                           {    //添加嵌套文件夹的插件
                               Name = dir.Name,
                               Path = dir.FullName,
                               LastWriteTime = dir.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss"),
                               Delete = () => dir.Delete(true)
                           });
            ModData = items;
        }

        /// <summary>
        /// 安装文件
        /// </summary>
        private void InstallFile(string[] paths)
        {
            //下面不用FileManager.Instance.ModDir.Exists的原因是防止数据更新不及时
            if (!Directory.Exists(FileManager.Instance.ModDir.FullName)) FileManager.Instance.ModDir.Create();
            foreach (string path in paths)
            {
                try
                {
                    if (!File.Exists(path)) continue;
                    SevenZipExtractor zip = new(path);
                    string firstFile = zip.ArchiveFileNames.FirstOrDefault()
                        ?? throw new Exception("该压缩包不含任何文件");
                    if (FileManager.Instance.ModDir.GetDirectories().Any(x => x.Name == firstFile.Split('\\')[0]))
                    {
                        if (MessageBox.Show("已存在该文件，是否覆盖安装？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.No) continue;
                    }
                    string targetName = path.Split('\\').Last().Split('.')[0];
                    zip.ExtractArchive(Path.Combine(FileManager.Instance.ModDir.FullName, targetName));
                    MessageBox.Show($"{targetName}安装完成");
                }
                catch (Exception e)
                {
                    MessageBox.Show($"插件{path}安装失败，原因: \n{e.Message}", "安装出错");
                }
            }
            RefreshData();
        }

        /// <summary>
        /// 移除BepInEx
        /// </summary>
        private static void RemoveBepInEx()
        {
            if (!FileManager.Instance.GameDataDir.Exists) return;
            if (FileManager.Instance.ModDir.Exists)
            {   //备份插件 防止误删
                FileManager.Instance.ModDir.CopyTo(Path.Combine(FileManager.TempDirPath, FileManager.Instance.ModDir.Name));
            }
            FileManager.Instance.BepInExDir.Delete(true);
            FileInfo dll = new(Path.Combine(FileManager.Instance.GameDataDir.FullName, "winhttp.dll"));
            if (dll.Exists) dll.Delete();
        }
    }
}