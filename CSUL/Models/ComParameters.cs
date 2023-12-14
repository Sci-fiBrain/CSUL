/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ComParameters.cs
 *  创建时间: 2023年12月14日 21:29
 *  创建开发: ScifiBrain
 *  文件介绍: 公共参数类
 *  --------------------------------------
 */

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
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSUL_CP.config");

        private static readonly string TempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempFile");

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
            else (gameRootPath, gameDataPath) = getBasePath();
            if (!GameRoot.Exists)
            {
                MessageBox.Show("游戏安装文件夹未找到，请手动设定", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                gameRootPath = Path.Combine(TempPath, "fakeGameRoot");
                if (!GameRoot.Exists) GameRoot.Create();
            }
            if (!GameData.Exists)
            {
                MessageBox.Show("游戏数据文件夹未找到，请手动设定", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                gameDataPath = Path.Combine(TempPath, "fakeGameData");
                if (!GameData.Exists) GameData.Create();
            }
            Instance = this;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.SaveConfig(ConfigPath);
        }

        #endregion ---构造函数---

        #region ---公共属性---

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

        #region --游戏平台相关--

        /// <summary>
        /// Steam路径
        /// </summary>
        [Config]
        public string? SteamPath { get; set; }

        #endregion --游戏平台相关--

        #region --文件夹相关--

        #region -基础文件夹-

        private string? gameDataPath;

        /// <summary>
        /// 游戏数据文件夹
        /// </summary>
        [Config]
        public DirectoryInfo GameData
        {
            get => new(gameDataPath ?? "unknow");
            set => gameDataPath = value.FullName;
        }

        private string? gameRootPath;

        /// <summary>
        /// 游戏安装文件夹
        /// </summary>
        [Config]
        public DirectoryInfo GameRoot
        {
            get => new(gameRootPath ?? "unknow");
            set => gameRootPath = value.FullName;
        }

        #endregion -基础文件夹-

        #region -派生文件夹-

        /// <summary>
        /// 地图文件夹
        /// </summary>
        public DirectoryInfo Maps
        {
            get => GetDirectoryInfo(Path.Combine(gameDataPath, "Maps"));
        }

        /// <summary>
        /// 存档文件夹
        /// </summary>
        public DirectoryInfo Saves
        {
            get => GetDirectoryInfo(Path.Combine(gameDataPath, "Saves"));
        }

        /// <summary>
        /// BepInEx根文件夹
        /// </summary>
        public DirectoryInfo BepInEx
        {
            get => GetDirectoryInfo(Path.Combine(gameRootPath, "BepInEx"));
        }

        /// <summary>
        /// BepInEx模组文件夹
        /// </summary>
        public DirectoryInfo BepMod
        {
            get => GetDirectoryInfo(Path.Combine(gameRootPath, "BepInEx", "plugins"));
        }

        /// <summary>
        /// BepInEx配置文件夹
        /// </summary>
        public DirectoryInfo BepConfig
        {
            get => GetDirectoryInfo(Path.Combine(gameRootPath, "BepInEx", "config"));
        }

        #endregion -派生文件夹-

        #endregion --文件夹相关--

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