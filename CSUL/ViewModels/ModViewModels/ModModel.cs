using CSUL.Models;
using CSUL.UserControls.DragFiles;
using CSUL.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CSUL.ViewModels.ModViewModels
{   //ModModel 构造函数、方法、子类
    public partial class ModModel : BaseViewModel
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

            /// <summary>
            /// 是否为单文件
            /// </summary>
            public bool IsFile { get; set; } = false;
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

        #region ---构造函数---

        public ModModel()
        {
            DeleteCommand = new RelayCommand(DeleteModFile);
            AddCommand = new RelayCommand(async (sender) =>
            {
                await InstallFile((sender as DragFilesEventArgs ?? throw new ArgumentNullException()).Paths);
            });
            DownloadCommand = new RelayCommand(async (sender) => await DownloadBepInEx(sender));
            RemoveCommand = new RelayCommand(RemoveBepInEx);
            CheckMods = new RelayCommand(CheckModCompatibility);
            Refresh = new RelayCommand(sender => RefreshData());
            RefreshData();
        }

        #endregion ---构造函数---

        #region ---ICommand方法---

        /// <summary>
        /// 删除模组文件
        /// </summary>
        private void DeleteModFile(object? sender)
        {
            if (sender is not ItemData data) return;
            StringBuilder sb = new();
            sb.Append("模组名称: ").Append(data.Name).AppendLine();
            sb.Append("最后修改时间: ").Append(data.LastWriteTime).AppendLine();
            sb.Append("模组路径: ").AppendLine().Append(data.Path).AppendLine();
            var ret = MessageBox.Show(sb.ToString(), "删除模组", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
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
        }

        /// <summary>
        /// 下载并安装BepInEx
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private async Task DownloadBepInEx(object? sender)
        {
            try
            {
                if (sender is not BepItemData data) return;
                if (MessageBox.Show(data.Version, "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                string path = Path.Combine(_tempDirPath, data.Name);
                FileInfo file = new(path);
                if (file.Directory?.Exists is true) file.Directory.Delete(true);
                if (file.Exists) file.Delete();
                if (file.Directory?.Exists is false) file.Directory?.Create();
                await WebManager.DownloadFromUri(data.Uri, path);
                using TempPackage package = new();
                await package.Decompress(path);
                RemoveBepInEx();
                ExFileManager.CopyTo(package.FullName, FileManager.Instance.GameRootDir.FullName, true);
                ShowNoEx = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;
                BepVersion = FileManager.Instance.BepVersion;
                MessageBox.Show("安装完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionManager.GetExMeg(ex), "安装失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 移除BepInEx
        /// </summary>
        private async void RemoveBepInEx(object? sender)
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
                        Process.Start("Explorer.exe", _tempDirPath);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(ExceptionManager.GetExMeg(e), "移除失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    BepData = GetBepDownloadData();
                    await Task.Delay(500);
                    ShowNoEx = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;
                    BepVersion = FileManager.Instance.BepVersion;
                }
            }
        }

        /// <summary>
        /// 检查模组兼容性
        /// </summary>
        private void CheckModCompatibility(object? sender)
        {
            if (ModData is null)
            {
                MessageBox.Show("模组安装信息获取失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ModData.Count < 1)
            {
                MessageBox.Show("还没有安装模组", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Version? knownBepVersion = FileManager.Instance.BepVersion;
            if (knownBepVersion is null)
            {
                MessageBox.Show("BepInEx版本信息获取失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Dictionary<int, (string name, string version)> allData = new();
            List<int> pass = new(), wrong = new(), unknow = new();
            for (int i = 0; i < ModData.Count; i++)
            {
                ItemData item = ModData[i];
                try
                {
                    //检查单个模组
                    BepInExCheckResult ret = ExFileManager.ChickModBepInExVersioin(item.Path,
                        out Version? modVersion, out Version? bepVersion, item.IsFile, knownBepVersion);
                    switch (ret)
                    {
                        case BepInExCheckResult.WrongVersion:
                            wrong.Add(i);
                            allData[i] = (ModData[i].Name, modVersion?.ToString() ?? "Unknow");
                            break;

                        case BepInExCheckResult.Passed:
                            pass.Add(i);
                            allData[i] = (ModData[i].Name, modVersion?.ToString() ?? "Unknow");
                            break;

                        default: throw new Exception();
                    }
                }
                catch
                {
                    unknow.Add(i);
                    allData[i] = (ModData[i].Name, "Unknow");
                }
            }
            ModCompatibilityBox.ShowBox(allData, pass, wrong, unknow);
        }

        #endregion ---ICommand方法---

        #region ---私有方法---

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
                               Delete = file.Delete,
                               IsFile = true,
                           });
            DirectoryInfo[] dirs = FileManager.Instance.ModDir.GetDirectories();
            items.AddRange(from dir in dirs
                           select new ItemData()
                           {    //添加嵌套文件夹的插件
                               Name = dir.Name,
                               Path = dir.FullName,
                               LastWriteTime = dir.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss"),
                               Delete = () => dir.Delete(true),
                               IsFile = false
                           });
            ModData = items;
        }

        /// <summary>
        /// 安装文件
        /// </summary>
        private async Task InstallFile(string[] paths)
        {
            if (!FileManager.Instance.ModDir.Exists) FileManager.Instance.ModDir.Create();
            foreach (string path in paths)
            {
                try
                {
                    using TempPackage package = new();
                    if (path.EndsWith(".dll")) await package.AddFile(path);
                    else await package.Decompress(path);
                    if (package.IsEempty) throw new Exception("该包不含任何文件");
                    string name = Path.GetFileName(path);
                    name = name[..name.LastIndexOf('.')];
                    switch (ExFileManager.ChickModBepInExVersioin(package.FullName, out Version? modVersion, out Version? bepVersion))
                    {
                        case BepInExCheckResult.Passed:
                            break;

                        case BepInExCheckResult.UnkownMod:
                            if (MessageBox.Show($"文件 {name} 安装警告\n" +
                                $"BepInEx版本: {bepVersion}\n" +
                                "但未能成功获取模组BepInEx版本\n" +
                                "请检查该文件是否为模组文件\n" +
                                "是否继续？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                            {
                                continue;
                            }
                            break;

                        case BepInExCheckResult.UnknowBepInEx:
                            if (MessageBox.Show($"模组 {name} 安装警告\n" +
                                $"插件版本: {modVersion}\n" +
                                "但未能获取已安装BepInEx的版本信息\n" +
                                "是否继续？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                            {
                                continue;
                            }
                            break;

                        case BepInExCheckResult.WrongVersion:
                            if (MessageBox.Show($"模组 {name} 安装警告" +
                                $"但模组版本与BepInEx不符\n" +
                                $"BepInEx版本: {bepVersion}\n" +
                                $"插件版本: {modVersion}\n" +
                                $"该情况可能会引发兼容性问题\n" +
                                $"是否继续？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                            {
                                continue;
                            }
                            break;

                        default:
                            if (MessageBox.Show("未知兼容性检查结果\n" +
                                "是否继续？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                            {
                                continue;
                            }
                            break;
                    }
                    string targetDir = FileManager.Instance.ModDir.FullName;
                    if (package.OnlySingleFile)
                    {
                        if (File.Exists(Path.Combine(targetDir, $"{name}.dll")))
                        {
                            if (MessageBox.Show($"模组{name}.dll已存在\n" +
                                $"是否覆盖安装？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel)
                            {
                                continue;
                            }
                        }
                        File.Copy(path, Path.Combine(targetDir, $"{name}.dll"), true);
                    }
                    else
                    {
                        if (Directory.Exists(Path.Combine(targetDir, name)))
                        {
                            if (MessageBox.Show($"模组{name}已存在\n" +
                                $"是否覆盖安装？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel)
                            {
                                continue;
                            }
                        }
                        Directory.CreateDirectory(Path.Combine(targetDir, name));
                        Directory.Delete(Path.Combine(targetDir, name));
                        ExFileManager.CopyTo(package.FullName, Path.Combine(targetDir, name));
                        MessageBox.Show($"模组 {name} 安装完成\n兼容性检查已结束", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
                FileManager.Instance.ModDir.CopyTo(Path.Combine(_tempDirPath, FileManager.Instance.ModDir.Name), true);
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

        #endregion ---私有方法---
    }
}