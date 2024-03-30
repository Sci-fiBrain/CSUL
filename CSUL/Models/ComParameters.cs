/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ComParameters.cs
 *  创建时间: 2023年12月14日 21:29
 *  创建开发: ScifiBrain
 *  文件介绍: 公共参数类
 *  --------------------------------------
 */

using CSUL.Models.Local.ModPlayer;
using System;
using System.IO;
using System.Windows;

namespace CSUL.Models
{
    internal class ComParameters : IDisposable
    {
        #region ---静态变量---

        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSUL.config");

        /// <summary>
        /// 得到<see cref="ComParameters"/>实例
        /// </summary>
        public static ComParameters Instance { get; private set; } = default!;

        #endregion ---静态变量---

        #region ---构造函数---

        /// <summary>
        /// 获取<see cref="ComParameters"/>实例
        /// </summary>
        /// <param name="getBasePath">用于获取游戏根目录和游戏数据目录的方法</param>
        internal ComParameters(Func<(string?, string?)> getBasePath)
        {
            if (File.Exists(ConfigPath)) try { this.LoadConfig(ConfigPath); } catch { }
            else
            {
                (string? root, string? data) = getBasePath();
                gameRootPath = root ?? "fake";
                gameDataPath = data ?? "fake";
            }
            gameRootPath ??= "fake";
            gameDataPath ??= "fake";
            if (!GameRoot.Exists)
            {
                MessageBox.Show("游戏安装文件夹未找到，请手动设定", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                gameRootPath = Path.Combine(Template.FullName, "fakeGameRoot");
                if (!GameRoot.Exists) GameRoot.Create();
            }
            if (!GameData.Exists)
            {
                MessageBox.Show("游戏数据文件夹未找到，请手动设定", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                gameDataPath = Path.Combine(Template.FullName, "fakeGameData");
                if (!GameData.Exists) GameData.Create();
            }
            ModPlayerManager = new(ModPlayers.FullName);
            Instance = this;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.SaveConfig(ConfigPath);
        }

        #endregion ---构造函数---

        #region ---公共属性---

        #region --游戏启动相关--

        /// <summary>
        /// 游戏启动参数
        /// </summary>
        [Config]
        public string? StartArguemnt { get; set; }

        /// <summary>
        /// 是否以开发模式启动
        /// </summary>
        [Config]
        public bool OpenDeveloper { get; set; }

        /// <summary>
        /// 是否显示Steam提示信息
        /// </summary>
        [Config]
        public bool ShowSteamInfo { get; set; } = true;

        /// <summary>
        /// 是否以Steam正版兼容模式启动
        /// </summary>
        [Config]
        public bool SteamCompatibilityMode { get; set; } = false;

        /// <summary>
        /// 选中的模组播放集的名字
        /// </summary>
        [Config]
        public string? SelectedModPlayer { get; set; }

        /// <summary>
        /// 是否启用汉化
        /// </summary>
        [Config]
        public bool StartChinesization { get; set; }

        #endregion --游戏启动相关--

        #region --游戏平台相关--

        /// <summary>
        /// Steam路径
        /// </summary>
        [Config]
        public string? SteamPath { get; set; }

        #endregion --游戏平台相关--

        #region --文件夹相关--

        #region -基础文件夹-

        private string gameDataPath;

        /// <summary>
        /// 游戏数据文件夹
        /// </summary>
        [Config]
        public DirectoryInfo GameData
        {
            get => new(gameDataPath);
            set => gameDataPath = value.FullName;
        }

        private string gameRootPath;

        /// <summary>
        /// 游戏安装文件夹
        /// </summary>
        [Config]
        public DirectoryInfo GameRoot
        {
            get => new(gameRootPath);
            set => gameRootPath = value.FullName;
        }

        /// <summary>
        /// 运行文件夹
        /// </summary>
        private readonly string basePath = AppDomain.CurrentDomain.BaseDirectory;

        public DirectoryInfo Base => GetDirectoryInfo(basePath);

        #endregion -基础文件夹-

        #region -派生文件夹-

        /// <summary>
        /// 地图文件夹
        /// </summary>
        public DirectoryInfo Maps => GetDirectoryInfo(Path.Combine(gameDataPath, "Maps"));

        /// <summary>
        /// 存档文件夹
        /// </summary>
        public DirectoryInfo Saves => GetDirectoryInfo(Path.Combine(gameDataPath, "Saves"));

        /// <summary>
        /// 资产文件夹
        /// </summary>
        public DirectoryInfo Asset => GetDirectoryInfo(Path.Combine(gameDataPath, "StreamingData"));

        /// <summary>
        /// Pmod文件夹
        /// </summary>
        public DirectoryInfo Pmod => GetDirectoryInfo(Path.Combine(gameDataPath, "Mods"));

        /// <summary>
        /// BepInEx根文件夹
        /// </summary>
        public DirectoryInfo BepInEx => GetDirectoryInfo(Path.Combine(gameRootPath, "BepInEx"));

        /// <summary>
        /// BepInEx模组文件夹
        /// </summary>
        public DirectoryInfo BepMod => GetDirectoryInfo(Path.Combine(gameRootPath, "BepInEx", "plugins"));

        /// <summary>
        /// BepInEx配置文件夹
        /// </summary>
        public DirectoryInfo BepConfig => GetDirectoryInfo(Path.Combine(gameRootPath, "BepInEx", "config"));

        /// <summary>
        /// 临时文件夹
        /// </summary>
        public DirectoryInfo Template => GetDirectoryInfo(Path.Combine(basePath, "Template"));

        /// <summary>
        /// 模组播放集文件夹
        /// </summary>
        public DirectoryInfo ModPlayers => GetDirectoryInfo(Path.Combine(basePath, "ModPlayers"));

        #endregion -派生文件夹-

        #region -派生文件-

        /// <summary>
        /// 玩家日志文件
        /// </summary>
        public FileInfo PlayerLog => new(Path.Combine(gameDataPath, "Player.log"));

        #endregion

        #endregion --文件夹相关--

        #region --模组播放集管理器--

        /// <summary>
        /// 模组播放集管理器
        /// </summary>
        public ModPlayerManager ModPlayerManager { get; set; }

        #endregion --模组播放集管理器--

        #endregion ---公共属性---

        #region ---私有方法---

        private static DirectoryInfo GetDirectoryInfo(string dirPath)
        {
            DirectoryInfo directory = new(dirPath);
            if (!directory.Exists) directory.Create();
            return directory;
        }

        #endregion ---私有方法---
    }
}