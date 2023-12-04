using CSUL.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CSUL.Models
{
    public class ModInfo : BaseViewModel
    {
        #region 公有字段
        public ModInfoCollection? Parent = null;
        #endregion

        #region 私有字段
        private ICommand enableCommand;
        private ICommand disableCommand;
        private string name;
        private string version = "?";
        private string mainFile = "";
        private bool isMainFileFound = false;
        private bool isEnabled = false;
        private string modPath = default!;
        private string lastWriteTime = default!;
        private bool isSingleFile = false;
        private string guid;
        private bool isDuplicated = false;
        #endregion

        #region 构造函数
        public ModInfo(string name)
        {
            Name = name;
        }
        #endregion

        #region 属性
        /// <summary>
        /// mod名称
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// mod版本号
        /// </summary>
        public string Version
        {
            get => version;
            set
            {
                version = value;
                OnPropertyChanged(nameof(Version));
            }
        }

        /// <summary>
        /// mod的GUID
        /// </summary>
        public string GUID
        {
            get => guid;
            set
            {
                guid = value;
                OnPropertyChanged(nameof(GUID));
            }
        }

        /// <summary>
        /// 主文件路径
        /// </summary>
        public string MainFile
        {
            get => mainFile;
            private set
            {
                mainFile = value;
                OnPropertyChanged(nameof(MainFile));
            }
        }

        /// <summary>
        /// 是否找到主文件
        /// </summary>
        public bool IsMainFileFound
        {
            get => isMainFileFound;
            private set
            {
                isMainFileFound = value;
                OnPropertyChanged(nameof(IsMainFileFound));
            }
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            private set
            {
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        /// <summary>
        /// 是否重复
        /// </summary>
        public bool IsDuplicated
        {
            get => isDuplicated;
            set
            {
                isDuplicated = value;
                OnPropertyChanged(nameof(IsDuplicated));
            }
        }

        /// <summary>
        /// 模组路径
        /// </summary>
        public string ModPath
        {
            get => modPath;
            private set
            {
                modPath = value;
                OnPropertyChanged(nameof(ModPath));
            }
        }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public string LastWriteTime
        {
            get => lastWriteTime;
            set
            {
                lastWriteTime = value;
                OnPropertyChanged(nameof(LastWriteTime));
            }
        }

        /// <summary>
        /// 是否为单文件
        /// </summary>
        public bool IsSingleFile
        {
            get => isSingleFile;
            private set
            {
                isSingleFile = value;
                OnPropertyChanged(nameof(IsSingleFile));
            }
        }
        #endregion

        #region 公有方法
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

        /// <summary>
        /// 删除mod文件
        /// </summary>
        /// <returns>(是否成功, 错误信息)</returns>
        public (bool Success, Exception? e) Delete()
        {
            FileSystemInfo mod = IsSingleFile ? new FileInfo(ModPath) : new DirectoryInfo(ModPath);
            try
            {
                mod.Delete(); return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }
        #endregion

        #region 静态方法
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
                var mods = FromDirectory(directory);
                foreach (var mod in mods)
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
                var mods = FromDirectory(directory);
                foreach (var mod in mods)
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
            string fileName = Path.GetFileNameWithoutExtension(file.FullName);
            if (!file.Exists)
            {
                throw new FileNotFoundException($"\"{file.FullName}\" 文件不存在！");
            }

            string? typeName = null;
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
                Assembly asm;
                using (FileStream stream = new FileStream(file.FullName, FileMode.Open))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        int res;
                        byte[] b = new byte[4096];
                        while ((res = stream.Read(b, 0, b.Length)) > 0)
                        {
                            memStream.Write(b, 0, b.Length);
                        }
                        asm = Assembly.Load(memStream.ToArray());
                    }
                }
                var type = asm.GetType(typeName);
                if (type != null)
                {
                    ModInfo info;
                    var nameField = type.GetField("PLUGIN_NAME");
                    var versionField = type.GetField("PLUGIN_VERSION");
                    var idField = type.GetField("PLUGIN_GUID");
                    if (nameField != null)
                    {
                        string? name = nameField.GetRawConstantValue() as string;
                        if (!string.IsNullOrEmpty(name))
                        {
                            info = new ModInfo(name);
                            if (versionField != null)
                            {
                                string? version = versionField.GetRawConstantValue() as string;
                                if (!string.IsNullOrEmpty(version))
                                {
                                    info.Version = version;
                                }
                            }
                            if (idField != null)
                            {
                                string? id = idField.GetRawConstantValue() as string;
                                if (!string.IsNullOrEmpty(id))
                                {
                                    info.GUID = id;
                                }
                            }
                            info.SetMainFile(file.FullName);
                            info.LastWriteTime = file.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss");
                            if (setModPath)
                            {
                                info.SetModPath(file.FullName);
                            }
                            return info;
                        }

                    }

                }
            }
            return null;
        }
        #endregion

        #region CommandMethod
        /// <summary>
        /// 禁用mod
        /// </summary>
        /// <param name="sender">Command传入的参数</param>
        private void Disable(object? sender)
        {
            if (!IsEnabled) { return; }
            FileInfo file = new FileInfo(MainFile);
            if (file.Exists && MainFile.ToLower().EndsWith(".dll"))
            {
                string newFileName = MainFile + "off";
                file.MoveTo(newFileName);
                IsEnabled = false;
                MainFile = newFileName;
                if (Parent != null)
                {
                    Parent.CheckDuplication(GUID);
                }
            }

        }

        /// <summary>
        /// 启用mod
        /// </summary>
        /// <param name="sender">Command传入的参数</param>
        private void Enable(object? sender)
        {
            if (IsEnabled) { return; }
            FileInfo file = new FileInfo(MainFile);
            if (file.Exists && MainFile.ToLower().EndsWith(".dlloff"))
            {
                string newFileName = MainFile.Replace("dlloff", "dll");
                file.MoveTo(newFileName);
                IsEnabled = true;
                MainFile = newFileName;
                if (Parent != null)
                {
                    Parent.CheckDuplication(GUID);
                }
            }
        }

        #endregion

        #region Command
        public ICommand EnableCommand
        {
            get
            {
                if (enableCommand == null)
                {
                    enableCommand = new RelayCommand(Enable);
                }
                return enableCommand;
            }
        }
        public ICommand DisableCommand
        {
            get
            {
                if (disableCommand == null)
                {
                    disableCommand = new RelayCommand(Disable);
                }
                return disableCommand;
            }
        }
        #endregion


    }

    /// <summary>
    /// mod信息集合
    /// </summary>
    public class ModInfoCollection : ObservableCollection<ModInfo>
    {
        /// <summary>
        /// 添加多个mod信息
        /// </summary>
        /// <param name="mods">要添加的mod信息</param>
        public void AddRange(IEnumerable<ModInfo> mods)
        {
            foreach (ModInfo mod in mods)
            {
                Add(mod);
            }
        }

        /// <summary>
        /// 添加mod信息
        /// </summary>
        /// <param name="modInfo">要添加的mod信息</param>
        public new void Add(ModInfo modInfo)
        {
            base.Add(modInfo);
            modInfo.Parent = this;
            CheckDuplication(modInfo.GUID);
        }

        /// <summary>
        /// 移除mod信息
        /// </summary>
        /// <param name="modInfo">要移除的mod信息</param>
        public new void Remove(ModInfo modInfo)
        {
            base.Remove(modInfo);
            modInfo.Parent = null;
            CheckDuplication(modInfo.GUID);
        }

        /// <summary>
        /// 移除索引处的mod信息
        /// </summary>
        /// <param name="index">要移除的mod信息的索引</param>
        public new void RemoveAt(int index)
        {
            ModInfo modInfo = Items[index];
            base.RemoveAt(index);
            modInfo.Parent = null;
            var search = Items.Where(s => s.GUID == modInfo.GUID);
            CheckDuplication(modInfo.GUID);
        }

        /// <summary>
        /// 检查GUID对应的mod是否重复
        /// </summary>
        /// <param name="guid">要查重的GUID</param>
        public void CheckDuplication(string guid)
        {
            var fileSearch = Items.Where(s => s.GUID == guid);
            var enabledSearch = fileSearch.Where(s => s.IsEnabled);
            int searchCount = enabledSearch.Count();
            bool duplicated = searchCount > 1;
            foreach (var item in fileSearch)
            {
                item.IsDuplicated = duplicated;
            }
        }
    }
}
