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

namespace CSUL.ViewModels.ModViewModels
{
    public class ModModel : BaseViewModel
    {
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
            RefreshData();
        }

        public Visibility ShowNoEx { get; } = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

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
                    zip.ExtractArchive(Path.Combine(FileManager.Instance.ModDir.FullName, path.Split('\\').Last().Split('.')[0])); ;
                    MessageBox.Show("安装完成");
                }
                catch (Exception e)
                {
                    MessageBox.Show($"插件{path}安装失败，原因: \n{e.Message}", "安装出错");
                }
            }
            RefreshData();
        }
    }
}