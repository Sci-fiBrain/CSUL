using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models
{
    /// <summary>
    /// 天际线2相关文件管理器
    /// </summary>
    public class FileManager
    {
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
            GamePath = Path.Combine(SteamGame.GetGameInstallPath("Cities Skylines II"), "Cities2.exe");

            //得到游戏数据文件路径
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localLow = Path.Combine(appData[0..appData.LastIndexOf('\\')], "LocalLow");
            if (!Directory.Exists(localLow)) throw new IOException("LocalLow is not existed");
            string game = Path.Combine(localLow, "Colossal Order", "Cities Skylines II");
            if (!Directory.Exists(game)) throw new IOException("Cities Skylines II is not existed");

            //初始化各文件夹信息对象
            GameDataDir = new(game);
            MapDir = new(Path.Combine(game, "Maps"));
            SaveDir = new(Path.Combine(game, "Saves"));
            BepInExDir = new(Path.Combine(game, "BepInEx"));
            ModDir = new(Path.Combine(game, "BepInEx", "plugins"));
        }
        #endregion

        #region ---私有字段---
        private DirectoryInfo gameDataDir = default!;
        private DirectoryInfo mapDir = default!;
        private DirectoryInfo saveDir = default!;
        private DirectoryInfo bepInExDir = default!;
        private DirectoryInfo modDir = default!;
        #endregion

        #region ---公共属性---
        /// <summary>
        /// 游戏文件夹
        /// </summary>
        public DirectoryInfo GameDataDir
        {
            get => gameDataDir;
            set => SetDirInfo(ref gameDataDir, value);
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
        /// BepInEx是否存在
        /// </summary>
        public bool BepInExExited
        {
            get
            {
                return File.Exists(Path.Combine(GameDataDir.FullName, "winhttp.dll"));
            }
        }

        /// <summary>
        /// 游戏路径
        /// </summary>
        public string? GamePath { get; }
        #endregion

        #region ---公共方法---

        #endregion

        #region ---私有方法---
        /// <summary>
        /// 修改文件夹信息
        /// </summary>
        /// <param name="target">被修改的对象</param>
        /// <param name="source">目标值</param>
        private void SetDirInfo(ref DirectoryInfo target, DirectoryInfo value)
        {
            if (target == value) return;
            if (!value.Exists) value.Create();
            target = value;
        }
        #endregion
    }
}
