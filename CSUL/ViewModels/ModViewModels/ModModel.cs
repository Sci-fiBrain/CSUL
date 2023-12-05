using CSUL.Models;
using CSUL.UserControls.DragFiles;
using CSUL.Windows;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CSUL.ViewModels.ModViewModels
{   //ModModel 构造函数、方法、子类
    public partial class ModModel : BaseViewModel
    {

        #region ---Mod信息类---
        public class ModInfo
        {
            #region ---构造函数---
            public ModInfo(string name)
            {
                Name = name;
            }
            #endregion ---构造函数---

            #region ---公共属性---
            /// <summary>
            /// mod名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// mod版本号
            /// </summary>
            public string Version { get; set; } = "?";

            /// <summary>
            /// mod的GUID
            /// </summary>
            public string GUID { get; set; }

            /// <summary>
            /// 主文件路径
            /// </summary>
            public string MainFile { get; private set; } = "";

            /// <summary>
            /// 是否找到主文件
            /// </summary>
            public bool IsMainFileFound { get; private set; } = false;

            /// <summary>
            /// 是否启用
            /// </summary>
            public bool IsEnabled { get; set; } = false;

            /// <summary>
            /// 是否重复
            /// </summary>
            public bool IsDuplicated { get; set; } = false;

            /// <summary>
            /// 模组路径
            /// </summary>
            public string ModPath { get; private set; } = default!;

            /// <summary>
            /// 最后修改时间
            /// </summary>
            public string LastWriteTime { get; set; } = default!;

            /// <summary>
            /// 是否为单文件
            /// </summary>
            public bool IsSingleFile { get; private set; } = false;

            #endregion ---公共属性---

            #region ---公共方法---
            /// <summary>
            /// 设置mod主文件，如果成功，则设置IsMainFileFound为true。
            /// <para>同时会判断该mod是否被启用。</para>
            /// </summary>
            /// <exception cref="FileNotFoundException">所传入文件路径不存在时引发</exception>
            /// <exception cref="ArgumentException">所传入文件后缀名不是dll或dlloff时引发</exception>
            /// <param name="mainFile">主文件路径</param>
            public void SetMainFile(string mainFile)
            {
                if (Path.Exists(mainFile) && (mainFile.ToLower().EndsWith(".dll") || mainFile.ToLower().EndsWith(".dlloff")))
                {
                    MainFile = mainFile;
                    IsMainFileFound = true;
                    IsEnabled = mainFile.ToLower().EndsWith(".dll");
                }
                else
                {
                    if (!Path.Exists(mainFile)) { throw new FileNotFoundException($"\"{mainFile}\" 该文件不存在!"); }
                    else
                    {
                        throw new ArgumentException($"\"{mainFile}\" 该文件不是mod文件!");
                    }
                }
            }

            /// <summary>
            /// 设置mod路径，并判断是否为单文件
            /// </summary>
            /// <param name="modPath">mod路径</param>
            /// <exception cref="ArgumentException">当路径不存在时引发</exception>
            public void SetModPath(string modPath)
            {
                if (File.Exists(modPath))
                {
                    ModPath = modPath;
                    IsSingleFile = true;
                }
                else if (Directory.Exists(modPath))
                {
                    ModPath = modPath;
                    IsSingleFile = false;
                }
                else { throw new ArgumentException($"\"{modPath}\" 该路径不存在!"); }
            }
            #endregion ---公共方法---
        }
        #endregion ---Mod信息类---


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
            RefreshCommand = new RelayCommand(Refresh);
            DisableCommand = new RelayCommand(Disable);
            EnableCommand = new RelayCommand(Enable);
            RefreshData();
        }

        #endregion ---构造函数---

        #region ---ICommand方法---
        /// <summary>
        /// 禁用mod
        /// </summary>
        /// <param name="sender">Command传入的参数</param>
        private void Disable(object? sender)
        {
            if (sender is not ModInfo data) return;
            if (!data.IsEnabled) { return; }
            FileInfo file = new FileInfo(data.MainFile);
            if (file.Exists && data.MainFile.ToLower().EndsWith(".dll"))
            {
                string newFileName = data.MainFile + "off";
                file.MoveTo(newFileName);
            }
            RefreshData();
        }

        /// <summary>
        /// 启用mod
        /// </summary>
        /// <param name="sender">Command传入的参数</param>
        private void Enable(object? sender)
        {
            if (sender is not ModInfo data) return;
            if (data.IsEnabled) { return; }
            FileInfo file = new FileInfo(data.MainFile);
            if (file.Exists && data.MainFile.ToLower().EndsWith(".dlloff"))
            {
                string newFileName = data.MainFile.Replace("dlloff", "dll");
                file.MoveTo(newFileName);
            }
            RefreshData();
        }

        /// <summary>
        /// 删除模组文件
        /// </summary>
        private void DeleteModFile(object? sender)
        {
            if (sender is not ModInfo data) return;
            StringBuilder sb = new();
            sb.Append("模组名称: ").Append(data.Name).AppendLine();
            sb.Append("最后修改时间: ").Append(data.LastWriteTime).AppendLine();
            sb.Append("模组路径: ").AppendLine().Append(data.ModPath).AppendLine();
            var ret = MessageBox.Show(sb.ToString(), "删除模组", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (ret == MessageBoxResult.OK)
            {
                FileSystemInfo mod = data.IsSingleFile ? new FileInfo(data.ModPath) : new DirectoryInfo(data.ModPath);
                try
                {
                    mod.Delete();
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
                ModInfo item = ModData[i];
                try
                {
                    //检查单个模组
                    BepInExCheckResult ret = ExFileManager.ChickModBepInExVersioin(item.ModPath,
                        out Version? modVersion, out Version? bepVersion, item.IsSingleFile, knownBepVersion);
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

        private void Refresh(object? sender)
        {
            RefreshData();
        }

        #endregion ---ICommand方法---

        #region ---私有方法---

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void RefreshData()
        {
            List<ModInfo> modInfos = new List<ModInfo>();
            FileInfo[] files = FileManager.Instance.ModDir.GetFiles("*.dll*");
            modInfos.AddRange(from file in files select FromFile(file));
            DirectoryInfo[] dirs = FileManager.Instance.ModDir.GetDirectories();
            modInfos.AddRange(FromDirectories(dirs));
            CheckDuplication(modInfos);
            ModData = modInfos;
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
        #endregion ---私有方法---

        #region ---静态方法---
        /// <summary>
        /// 检查GUID对应的mod是否重复
        /// </summary>
        /// <param name="guid">要查重的GUID</param>
        private static void CheckDuplication(IEnumerable<ModInfo> mods)
        {
            List<string> _checked = new List<string>();
            foreach (ModInfo mod in mods)
            {
                string guid = mod.GUID;
                if (_checked.Contains(guid)) continue;
                var fileSearch = mods.Where(s => s.GUID == guid);
                var enabledSearch = fileSearch.Where(s => s.IsEnabled);
                int searchCount = enabledSearch.Count();
                bool duplicated = searchCount > 1;
                foreach (var item in fileSearch)
                {
                    item.IsDuplicated = duplicated;
                }
                _checked.Add(guid);
            }
        }

        /// <summary>
        /// 从dll文件获取Mod信息
        /// </summary>
        /// <param name="file">dll文件路径</param>
        public static ModInfo? FromFile(FileInfo file)
        {
            return GetModFromFile(file);
        }

        /// <summary>
        /// 从文件夹获取Mod信息
        /// </summary>
        /// <param name="dir">文件夹路径</param>
        /// <returns></returns>
        public static IEnumerable<ModInfo> FromDirectory(DirectoryInfo dir)
        {
            DirectoryInfo[] directories = dir.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                IEnumerable<ModInfo> mods = FromDirectory(directory);
                foreach (ModInfo mod in mods)
                {
                    yield return mod;
                }
            }
            FileInfo[] files = dir.GetFiles("*.dll*");
            foreach (FileInfo file in files)
            {
                ModInfo? mod = GetModFromFile(file, false);
                if (mod != null)
                {
                    mod.SetModPath(dir.FullName);
                    yield return mod;
                }
            }
        }

        /// <summary>
        /// 从多个文件夹获取Mod信息
        /// </summary>
        /// <param name="directories">文件夹路径</param>
        /// <returns></returns>
        public static IEnumerable<ModInfo> FromDirectories(IEnumerable<DirectoryInfo> directories)
        {
            foreach (DirectoryInfo directory in directories)
            {
                IEnumerable<ModInfo> mods = FromDirectory(directory);
                foreach (ModInfo mod in mods)
                {
                    yield return mod;
                }
            }
        }

        /// <summary>
        /// 读取dll文件信息
        /// </summary>
        /// <param name="file"></param>
        /// <param name="setModPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private static ModInfo? GetModFromFile(FileInfo file, bool setModPath = true)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException($"\"{file.FullName}\" 文件不存在！");
            }

            string? typeName = null;
            using (StreamReader sr = new(file.FullName))
            {
                using PEReader portableExecutableReader = new(sr.BaseStream);
                MetadataReader metadataReader = portableExecutableReader.GetMetadataReader();

                foreach (TypeDefinitionHandle typeDefHandle in metadataReader.TypeDefinitions)
                {
                    TypeDefinition typeDef = metadataReader.GetTypeDefinition(typeDefHandle);
                    string name = metadataReader.GetString(typeDef.Name);
                    if (name.Contains("MyPluginInfo"))
                    {
                        string _namespace = metadataReader.GetString(typeDef.Namespace);
                        typeName = $"{_namespace}.{name}";
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(typeName))
            {
                Assembly asm;
                using (FileStream stream = new(file.FullName, FileMode.Open))
                {
                    using MemoryStream memStream = new();
                    int res;
                    byte[] b = new byte[file.Length];
                    while ((res = stream.Read(b, 0, b.Length)) > 0)
                    {
                        memStream.Write(b, 0, b.Length);
                    }
                    asm = Assembly.Load(memStream.ToArray());
                }
                Type? type = asm.GetType(typeName);
                FieldInfo? nameField = type?.GetField("PLUGIN_NAME");
                FieldInfo? versionField = type?.GetField("PLUGIN_VERSION");
                FieldInfo? idField = type?.GetField("PLUGIN_GUID");
                string? name = nameField?.GetRawConstantValue() as string;
                if (!string.IsNullOrEmpty(name))
                {
                    ModInfo info = new(name);
                    string? version = versionField?.GetRawConstantValue() as string;
                    info.Version = version ?? "?";

                    string? id = idField?.GetRawConstantValue() as string;
                    info.GUID = id ?? "";

                    info.SetMainFile(file.FullName);
                    info.LastWriteTime = file.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss");
                    if (setModPath)
                    {
                        info.SetModPath(file.FullName);
                    }
                    return info;
                }
            }
            return null;
        }
        #endregion ---静态方法---

    }
}