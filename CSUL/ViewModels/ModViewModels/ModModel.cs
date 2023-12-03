using CSUL.Models;
using CSUL.UserControls.DragFiles;
using CSUL.Windows;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace CSUL.ViewModels.ModViewModels
{   //ModModel 构造函数、方法、子类
    public partial class ModModel : BaseViewModel
    {
        #region ---模组信息条目---

        public class ItemData
        {
            /// <summary>
            /// 模组信息
            /// </summary>
            public PluginInfo PluginInfo { get; set; } = default!;

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
            AddCommand = new RelayCommand((sender) =>
            {
                InstallFile((sender as DragFilesEventArgs ?? throw new ArgumentNullException()).Paths);
            });
            DownloadCommand = new RelayCommand(DownloadBepInEx);
            RemoveCommand = new RelayCommand(RemoveBepInEx);
            CheckMods = new RelayCommand(CheckModCompatibility);
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
            sb.Append("模组名称: ").Append(data.PluginInfo).AppendLine();
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
        private async void DownloadBepInEx(object? sender)
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
                        Process.Start("Explorer.exe", FileManager.TempDirPath);
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
                            allData[i] = (ModData[i].PluginInfo.Name, modVersion?.ToString() ?? "Unknow");
                            break;

                        case BepInExCheckResult.Passed:
                            pass.Add(i);
                            allData[i] = (ModData[i].PluginInfo.Name, modVersion?.ToString() ?? "Unknow");
                            break;

                        default: throw new Exception();
                    }
                }
                catch
                {
                    unknow.Add(i);
                    allData[i] = (ModData[i].PluginInfo.Name, "Unknow");
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
            FileInfo[] files = FileManager.Instance.ModDir.GetFiles("*.dll*");
            items.AddRange(from file in files
                           select new ItemData()
                           {    //添加单文件的插件
                               PluginInfo = GetPluginInfoFromFile(file),
                               Path = file.FullName,
                               LastWriteTime = file.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss"),
                               Delete = file.Delete,
                               IsFile = true,
                           });
            DirectoryInfo[] dirs = FileManager.Instance.ModDir.GetDirectories();
            items.AddRange(from dir in dirs
                           select new ItemData()
                           {    //添加嵌套文件夹的插件
                               PluginInfo = GetPluginInfoFromDir(dir),
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
                    string targetName = path.Split('\\').Last();
                    targetName = targetName[..targetName.LastIndexOf('.')];
                    string modPath = Path.Combine(FileManager.Instance.ModDir.FullName, targetName);
                    zip.ExtractArchive(modPath);
                    switch (ExFileManager.ChickModBepInExVersioin(modPath, out Version? modVersion, out Version? bepVersion))
                    {
                        case BepInExCheckResult.Passed:
                            MessageBox.Show($"模组 {targetName} 安装完成\n" +
                                $"版本兼容性已检查", "安装成功", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;

                        case BepInExCheckResult.UnkownMod:
                            MessageBox.Show($"文件 {targetName} 已安装\n" +
                                $"BepInEx版本: {bepVersion}\n" +
                                "但未能成功获取模组BepInEx版本\n" +
                                "请检查该文件是否为模组文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;

                        case BepInExCheckResult.UnknowBepInEx:
                            MessageBox.Show($"模组 {targetName} 已安装\n" +
                                $"插件版本: {modVersion}\n" +
                                "但未能获取已安装BepInEx的版本信息\n" +
                                "请自行检查模组兼容性", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;

                        case BepInExCheckResult.WrongVersion:
                            MessageBox.Show($"模组 {targetName} 已安装" +
                                $"但模组版本与BepInEx不符\n" +
                                $"BepInEx版本: {bepVersion}\n" +
                                $"插件版本: {modVersion}\n" +
                                $"该情况可能会引发兼容性问题", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;

                        default:
                            MessageBox.Show("未知兼容性检查结果");
                            break;
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

        private static PluginInfo GetPluginInfoFromDir(DirectoryInfo directory)
        {
            PluginInfo pluginInfo = new PluginInfo(directory.Name, "?");
            FileInfo[] files = directory.GetFiles("*.dll*");
            foreach (FileInfo file in files)
            {
                PluginInfo info = GetPluginInfoFromFile(file);
                if (info.IsMainFileFound)
                {
                    pluginInfo = info;
                    break;
                }
            }
            return pluginInfo;
        }

        /// <summary>
        /// 从dll文件获取Mod信息（名称及版本）
        /// </summary>
        /// <param name="file">dll文件路径</param>
        public static PluginInfo GetPluginInfoFromFile(FileInfo file)
        {
            bool flag = false;
            string fileName = Path.GetFileNameWithoutExtension(file.FullName);
            PluginInfo pluginInfo = new PluginInfo(fileName, "?");
            if (!file.Exists)
            {
                throw new FileNotFoundException($"\"{file.FullName}\" 文件不存在！");
            }

            string typeName = null;
            using (var sr = new StreamReader(file.FullName))
            {
                using (var portableExecutableReader = new PEReader(sr.BaseStream))
                {
                    var metadataReader = portableExecutableReader.GetMetadataReader();

                    foreach (var typeDefHandle in metadataReader.TypeDefinitions)
                    {
                        var typeDef = metadataReader.GetTypeDefinition(typeDefHandle);
                        string name = metadataReader.GetString(typeDef.Name);
                        if (name.Contains("MyPluginInfo"))
                        {
                            string _namespace = metadataReader.GetString(typeDef.Namespace);
                            typeName = $"{_namespace}.{name}";
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(typeName))
            {
                Assembly asm = Assembly.LoadFile(file.FullName);
                var type = asm.GetType(typeName);
                if (type != null)
                {
                    var nameField = type.GetField("PLUGIN_NAME");
                    var versionField = type.GetField("PLUGIN_VERSION");
                    if (nameField != null)
                    {
                        string name = nameField.GetRawConstantValue() as string;
                        if (!string.IsNullOrEmpty(name))
                        {
                            pluginInfo.Name = name;
                        }
                    }
                    if (versionField != null)
                    {
                        string version = versionField.GetRawConstantValue() as string;
                        if (!string.IsNullOrEmpty(version))
                        {
                            pluginInfo.Version = version;
                        }
                    }
                    pluginInfo.SetMainFile(file.FullName);
                    flag = true;
                }
            }
            return pluginInfo;
        }

        #endregion ---私有方法---

        #region Test
        private void test(object? sender)
        {
            //PluginInfo pluginInfo;
            //GetPluginInfoFromFile(new FileInfo(@"D:\SteamLibrary\steamapps\common\Cities Skylines II\BepInEx\plugins\TrafficUnlocker\TrafficUnlocker.dll"), out pluginInfo);
        }
        private ICommand _testCommand;
        public ICommand TestCommand
        {
            get
            {
                if (_testCommand == null)
                {
                    _testCommand = new RelayCommand(test);
                }
                return _testCommand;
            }
        }
        #endregion
    }
}