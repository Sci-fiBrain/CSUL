using System;
using System.IO;

namespace CSUL.Models
{
    /// <summary>
    /// 天际线2相关文件管理器
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSUL.config");

        /// <summary>
        /// 临时文件夹路径
        /// </summary>
        public static readonly string TempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempFile");

        /// <summary>
        /// 获取<see cref="FileManager"/>实例
        /// </summary>
        public static FileManager Instance { get; } = new();

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
                    return;
                }
                catch { }
            }
            //得到游戏数据文件路径
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localLow = Path.Combine(appData[0..appData.LastIndexOf('\\')], "LocalLow");
            if (!Directory.Exists(localLow)) throw new IOException("LocalLow is not existed");
            string game = Path.Combine(localLow, "Colossal Order", "Cities Skylines II");
            if (!Directory.Exists(game)) throw new IOException("Cities Skylines II is not existed");

            //初始化各文件夹信息对象
            GameRootDir = new(SteamGame.GetGameInstallPath("Cities Skylines II"));
            GamePath = Path.Combine(gameRootDir.FullName, "Cities2.exe");
            GameDataDir = new(game);
        }

        #endregion ---构造函数---

        #region ---私有字段---

        private DirectoryInfo gameRootDir = default!;
        private DirectoryInfo gameDataDir = default!;
        private DirectoryInfo mapDir = default!;
        private DirectoryInfo saveDir = default!;
        private DirectoryInfo bepInExDir = default!;
        private DirectoryInfo modDir = default!;

        #endregion ---私有字段---

        #region ---公共属性---

        /// <summary>
        /// 游戏安装文件夹
        /// </summary>
        [Config]
        public DirectoryInfo GameRootDir
        {
            get => gameRootDir;
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
            get => gameDataDir;
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
            get => mapDir;
            set => SetDirInfo(ref mapDir, value);
        }

        /// <summary>
        /// 存档文件夹
        /// </summary>
        public DirectoryInfo SaveDir
        {
            get => saveDir;
            set => SetDirInfo(ref saveDir, value);
        }

        /// <summary>
        /// 模组加载器文件夹
        /// </summary>
        public DirectoryInfo BepInExDir
        {
            get => bepInExDir;
            set => SetDirInfo(ref bepInExDir, value);
        }

        /// <summary>
        /// 模组文件夹
        /// </summary>
        public DirectoryInfo ModDir
        {
            get => modDir;
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
        /// 游戏路径
        /// </summary>
        public string? GamePath { get; set; }

        #endregion ---公共属性---

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
            ModDir = new(Path.Combine(BepInExDir.FullName, "plugins"));
        }

        #endregion ---私有方法---
    }
}