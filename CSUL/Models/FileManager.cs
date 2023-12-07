using CSUL.Models.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSUL.Models
{
    /// <summary>
    /// 天际线2相关文件管理器
    /// </summary>
    public class FileManager : IDisposable
    {
        #region ---公共静态属性---

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string ConfigPath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSUL.config");

        /// <summary>
        /// 获取<see cref="FileManager"/>实例
        /// </summary>
        public static FileManager Instance { get; } = new();

        #endregion ---公共静态属性---

        #region ---构造函数---

        /// <summary>
        /// 实例化<see cref="FileManager"/>对象
        /// </summary>
        private FileManager()
        {
            if (File.Exists(ConfigPath))
            {   //存在配置文件则读取
                try
                {
                    this.LoadConfig(ConfigPath);
                    SetOtherDataDir();
                    SetOtherRootDir();
                    return;
                }
                catch { }
            }
            try
            {
                //得到游戏数据文件路径
                string gameData, gameRoot;
                try
                {   //尝试自动获取数据路径
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string localLow = Path.Combine(appData[0..appData.LastIndexOf('\\')], "LocalLow");
                    gameData = Path.Combine(localLow, "Colossal Order", "Cities Skylines II");
                    if (!Directory.Exists(gameData)) throw new IOException("Cities Skylines II is not existed");
                }
                catch (Exception e)
                {   //获取失败，创建虚假目录，防止程序崩溃
                    MessageBox.Show($"原因:\n{e.Message}\n请手动设定目录", "游戏数据目录获取失败",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    gameData = Path.Combine(_tempDirPath, "fakeData");
                    if (!Directory.Exists(gameData)) Directory.CreateDirectory(gameData);
                }

                try
                {   //尝试自动获取游戏根目录
                    gameRoot = SteamGame.GetGameInstallPath("Cities Skylines II");
                }
                catch (Exception e)
                {   //获取失败，创建虚假目录，防止程序崩溃
                    MessageBox.Show($"原因:\n{e.Message}\n请手动设定目录", "游戏安装目录获取失败",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    gameRoot = Path.Combine(_tempDirPath, "fakeRoot");
                    if (!Directory.Exists(gameData)) Directory.CreateDirectory(gameData);
                }
                //初始化各文件夹信息对象
                GameRootDir = new(gameRoot);
                GameDataDir = new(gameData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionManager.GetExMeg(ex), "FileManager加载错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.SaveConfig(ConfigPath);
        }

        #endregion ---构造函数---

        #region ---私有字段---

        private DirectoryInfo gameRootDir = default!;
        private DirectoryInfo gameDataDir = default!;
        private DirectoryInfo mapDir = default!;
        private DirectoryInfo saveDir = default!;
        private DirectoryInfo bepInExDir = default!;
        private DirectoryInfo modDir = default!;

        /// <summary>
        /// 临时文件夹路径
        /// </summary>
        private static readonly string _tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempFile");

        #endregion ---私有字段---

        #region ---公共属性---

        /// <summary>
        /// 游戏安装文件夹
        /// </summary>
        [Config]
        public DirectoryInfo GameRootDir
        {
            get => gameRootDir = new(gameRootDir.FullName);
            set
            {
                SetDirInfo(ref gameRootDir, value);
                SetOtherRootDir();
            }
        }

        /// <summary>
        /// 游戏数据文件夹
        /// </summary>
        [Config]
        public DirectoryInfo GameDataDir
        {
            get => gameDataDir = new(gameDataDir.FullName);
            set
            {
                SetDirInfo(ref gameDataDir, value);
                SetOtherDataDir();
            }
        }

        /// <summary>
        /// 地图文件夹
        /// </summary>
        public DirectoryInfo MapDir
        {
            get => mapDir = new(mapDir.FullName);
            set => SetDirInfo(ref mapDir, value);
        }

        /// <summary>
        /// 存档文件夹
        /// </summary>
        public DirectoryInfo SaveDir
        {
            get => saveDir = new(saveDir.FullName);
            set => SetDirInfo(ref saveDir, value);
        }

        /// <summary>
        /// 模组加载器文件夹
        /// </summary>
        public DirectoryInfo BepInExDir
        {
            get => bepInExDir = new(bepInExDir.FullName);
            set => SetDirInfo(ref bepInExDir, value);
        }

        /// <summary>
        /// 模组文件夹
        /// </summary>
        public DirectoryInfo ModDir
        {
            get => modDir = new(modDir.FullName);
            set => SetDirInfo(ref modDir, value);
        }

        /// <summary>
        /// BepInEx是否不存在
        /// </summary>
        public bool NoBepInEx
        {
            get
            {
                return !File.Exists(Path.Combine(GameRootDir.FullName, "winhttp.dll"));
            }
        }

        /// <summary>
        /// 获取BepInEx的版本号
        /// </summary>
        public Version? BepVersion
        {
            get
            {
                try
                {
                    DirectoryInfo coreDir = new(Path.Combine(BepInExDir.FullName, "core"));
                    if (!coreDir.Exists) return null;
                    FileInfo? bepFile = coreDir.GetFiles().FirstOrDefault(x =>
                        x.Name.StartsWith("BepInEx") && x.Name.EndsWith(".dll"));
                    if (bepFile == null) return null;
                    //Assembly assembly = Assembly.LoadFrom(bepFile.FullName);
                    //AssemblyName? bep = assembly.GetReferencedAssemblies().FirstOrDefault(x =>
                    //    x.Name?.Contains("BepInEx") is true);
                    //Version? version = bep?.Version;

                    //下面这种方法比较高效
                    FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(bepFile.FullName);
                    string? version = fileVersion.FileVersion;
                    return version is null ? null : new Version(version);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 游戏路径
        /// </summary>
        public string? GamePath { get; set; }

        #endregion ---公共属性---

        #region ---公共方法---

        /// <summary>
        /// 安装游戏数据文件（地图、存档）
        /// </summary>
        public async Task<InstalledGameDataFiles> InstallGameDataFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
            InstalledGameDataFiles ret = new() { MapNames = new(), SaveNames = new() };
            using TempPackage package = new();
            await package.Decompress(filePath);
            List<string> coks = new();
            void RecursionSearch(string root)
            {   //递归搜索方法
                foreach (string file in Directory.GetFiles(root))
                {
                    if (Path.GetExtension(file) == ".cok" && File.Exists(file + ".cid"))
                    {
                        coks.Add(file);
                    }
                }
                foreach (string dir in Directory.GetDirectories(root)) RecursionSearch(dir);
            }
            await Task.Run(() => RecursionSearch(package.FullName));
            foreach (string cok in coks)
            {
                try
                {
                    List<string> names;
                    string targetPath;
                    switch (TempPackage.GetGameDataFileType(cok))
                    {
                        case Enums.GameDataFileType.Save:
                            targetPath = SaveDir.FullName;
                            names = ret.SaveNames;
                            break;

                        case Enums.GameDataFileType.Map:
                            targetPath = MapDir.FullName;
                            names = ret.MapNames;
                            break;

                        default: continue;
                    }
                    string cokName = Path.GetFileName(cok);
                    if (File.Exists(Path.Combine(targetPath, cokName))) await Task.Run(() =>
                    {   //获得不重复的文件名
                        string reEx = cokName[..cokName.LastIndexOf('.')];    //去除扩展名
                        for (long i = 1; i < long.MaxValue; i++)
                        {
                            if (!File.Exists(Path.Combine(targetPath, $"{reEx}({i}).cok")))
                            {
                                cokName = $"{reEx}({i}).cok";
                                break;
                            }
                        }
                    });
                    await Task.Run(() => File.Copy(cok, Path.Combine(targetPath, cokName), true));
                    await Task.Run(() => File.Copy(cok + ".cid", Path.Combine(targetPath, cokName + ".cid"), true));
                    names.Add(cokName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ExceptionManager.GetExMeg(ex, $"{filePath}中的{cok}安装时出现问题"), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return ret;
        }

        #endregion ---公共方法---

        #region ---私有方法---

        /// <summary>
        /// 修改文件夹信息
        /// </summary>
        /// <param name="target">被修改的对象</param>
        /// <param name="source">目标值</param>
        private static void SetDirInfo(ref DirectoryInfo target, DirectoryInfo value)
        {
            if (target == value) return;
            if (!value.Exists) value.Create();
            target = value;
        }

        /// <summary>
        /// 修改其他数据文件路径
        /// </summary>
        private void SetOtherDataDir()
        {
            MapDir = new(Path.Combine(GameDataDir.FullName, "Maps"));
            SaveDir = new(Path.Combine(GameDataDir.FullName, "Saves"));
        }

        /// <summary>
        /// 修改其他安装文件路径
        /// </summary>
        private void SetOtherRootDir()
        {
            BepInExDir = new(Path.Combine(GameRootDir.FullName, "BepInEx"));
            GamePath = Path.Combine(GameRootDir.FullName, "Cities2.exe");
            ModDir = new(Path.Combine(BepInExDir.FullName, "plugins"));
        }

        #endregion ---私有方法---
    }
}