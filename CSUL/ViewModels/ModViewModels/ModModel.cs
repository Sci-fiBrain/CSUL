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

        #endregion ---模组信息条目---

        #region ---BepInEx信息条目---

        public class BepItemData
        {
            public string Name { get; set; } = default!;

            public string Version { get; set; } = default!;

            public Brush BackBrush { get; set; } = default!;

            public string Uri { get; set; } = default!;
        }

        #endregion ---BepInEx信息条目---

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
                    await zip.ExtractArchiveAsync(FileManager.Instance.GameRootDir.FullName);
                    ShowNoEx = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;
                    BepVersion = FileManager.Instance.BepVersion;
                    MessageBox.Show("安装完成");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ExceptionManager.GetExMeg(ex), "安装失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }) : default!;
            RemoveCommand = new RelayCommand((sender) =>
            {
                MessageBoxResult ret = MessageBox.Show("确认移除BepInEx?\n插件将会临时备份至tempFile文件夹", "警告",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (ret == MessageBoxResult.OK)
                {
                    try
                    {
                        RemoveBepInEx();
                        MessageBoxResult ret2 = MessageBox.Show("BepInEx移除成功\n是否打开插件备份文件夹", "提示",
                            MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (ret2 == MessageBoxResult.Yes)
                        {
                            Process.Start("Explorer.exe", FileManager.TempDirPath);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(ExceptionManager.GetExMeg(e), "移除失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        ShowNoEx = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;
                        BepVersion = FileManager.Instance.BepVersion;
                        BepData = GetBepDownloadData();
                    }
                }
            });
            RefreshData();
        }

        private Visibility showNoEx = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ShowNoEx
        {
            get => showNoEx;
            set
            {
                if (showNoEx == value) return;
                showNoEx = value;
                OnPropertyChanged();
            }
        }

        private Version? bepVersion = FileManager.Instance.BepVersion;

        public Version? BepVersion
        {
            get => bepVersion;
            set
            {
                if (bepVersion == value) return;
                bepVersion = value;
                OnPropertyChanged();
            }
        }

        //添加新插件
        public ICommand AddCommand { get; }

        //删除插件
        public ICommand DeleteCommand { get; }

        //下载BepInEx
        public ICommand DownloadCommand { get; }

        //打开文件夹
        public ICommand OpenFolder { get; } = new RelayCommand((sender)
            => Process.Start("Explorer.exe", FileManager.Instance.ModDir.FullName));

        //移除BepInEx
        public ICommand RemoveCommand { get; }

        public List<BepItemData>? BepData { get; private set; } = FileManager.Instance.NoBepInEx ?
            GetBepDownloadData() : null;

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
            if (!FileManager.Instance.ModDir.Exists) FileManager.Instance.ModDir.Create();
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
                    //得到压缩包的文件名 mod文件夹的路径
                    string targetName = path.Split('\\').Last().Split('.')[0];
                    string modPath = Path.Combine(FileManager.Instance.ModDir.FullName, targetName);
                    zip.ExtractArchive(modPath);
                    Version? version = ExFileManager.GetBepModVersion(modPath);
                    if (version is null)
                    {
                        MessageBox.Show($"文件 {targetName} 已安装\n" +
                            "但未能成功获取模组BepInEx版本\n" +
                            "请检查该文件是否为模组文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        Version? bepVersion = FileManager.Instance.BepVersion;
                        if (bepVersion is null)
                        {
                            MessageBox.Show($"模组 {targetName} 已安装\n" +
                                "但未能获取已安装BepInEx的版本信息\n" +
                                "请自行检查模组兼容性", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else if (version.Major != bepVersion.Major)
                        {
                            MessageBox.Show($"模组 {targetName} 已安装" +
                                $"但模组版本与BepInEx不符\n" +
                                $"BepInEx版本: {bepVersion}\n" +
                                $"插件版本: {version}\n" +
                                $"该情况可能会引发兼容性问题", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show($"模组 {targetName} 安装完成\n" +
                                $"版本兼容性已检查", "安装成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"插件{path}安装失败，原因: \n{ExceptionManager.GetExMeg(e)}", "安装出错",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            RefreshData();
        }

        /// <summary>
        /// 移除BepInEx
        /// </summary>
        private static void RemoveBepInEx()
        {
            if (!FileManager.Instance.GameRootDir.Exists) return;
            if (FileManager.Instance.ModDir.Exists)
            {   //备份插件 防止误删
                FileManager.Instance.ModDir.CopyTo(Path.Combine(FileManager.TempDirPath, FileManager.Instance.ModDir.Name));
            }
            if (FileManager.Instance.BepInExDir.Exists)
            {
                FileManager.Instance.BepInExDir.Delete(true);
            }
            FileInfo dll = new(Path.Combine(FileManager.Instance.GameRootDir.FullName, "winhttp.dll"));
            if (dll.Exists) dll.Delete();
        }

        /// <summary>
        /// 获取Bep下载数据
        /// </summary>
        private static List<BepItemData> GetBepDownloadData()
        {
            return (from item in WebManager.GetBepinexInfos()
                    select new BepItemData
                    {
                        Uri = item.Uri,
                        Version = $"安装 {item.Version} {(item.IsBeta ? "测试版" : "正式版")}",
                        Name = item.FileName,
                        BackBrush = new SolidColorBrush(Color.FromRgb(242, 242, 242))
                    }).ToList();
        }
    }
}