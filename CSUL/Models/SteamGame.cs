using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSUL.Models
{
    /// <summary>
    /// Steam有关方法
    /// </summary>
    public static class SteamGame
    {
        #region ---私有量---

        /// <summary>
        /// Steam注册表路径
        /// </summary>
        private const string SteamRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam";

        /// <summary>
        /// Steam库的相对路径
        /// </summary>
        private const string RelativeSteamLibrayPath = @"steamapps\common";

        /// <summary>
        /// Steam安装路径
        /// </summary>
        private static string SteamPath
        {
            get => Registry.GetValue(SteamRegistryKey, "InstallPath", null) as string
                ?? throw new RegistryNotFoundException("steam");
        }

        #endregion ---私有量---

        #region ---公共方法---

        /// <summary>
        /// 获取游戏安装路径
        /// </summary>
        /// <param name="gameName">游戏名称</param>
        /// <returns>游戏安装路径</returns>
        public static string GetGameInstallPath(string gameName)
        {
            string[] gamePaths = GetSteamLibrayPaths().SelectMany(x => Directory.GetDirectories(x)).ToArray();
            string targetPath = gamePaths.FirstOrDefault(x => x.Contains(gameName))
                ?? throw new FileNotFoundException("Get game install path failed", gameName);
            return targetPath;
        }

        /// <summary>
        /// 尝试获取游戏安装路径
        /// </summary>
        /// <param name="gameName">游戏名称</param>
        /// <param name="path">游戏安装路径</param>
        /// <returns>是否获取成功</returns>
        public static bool TryGetGameInstallPath(string gameName, out string? path)
        {
            try
            {
                path = GetGameInstallPath(gameName);
                return true;
            }
            catch { }
            path = null;
            return false;
        }

        #endregion ---公共方法---

        #region ---私有方法---

        /// <summary>
        /// 获取Steam游戏库文件夹路径
        /// </summary>
        /// <returns></returns>
        private static List<string> GetSteamLibrayPaths()
        {
            string vdfPath = Path.Combine(SteamPath, "steamapps", "libraryfolders.vdf");
            using StreamReader reader = new(new FileStream(vdfPath, FileMode.Open, FileAccess.Read));
            List<string> paths = new();
            while (!reader.EndOfStream)
            {
                if (reader.ReadLine()?.Trim('\t') is not string data) continue;
                if (!data.Contains("\"path\"")) continue;
                string rootPath = data.TrimEnd('\"').Split('\"').Last();
                paths.Add(Path.Combine(rootPath.Replace(@"\\", @"\"), RelativeSteamLibrayPath));
            }
            return paths;
        }

        #endregion ---私有方法---
    }

    /// <summary>
    /// 注册表未找到异常
    /// </summary>
    public class RegistryNotFoundException : Exception
    {
        public RegistryNotFoundException(string? name) : base($"Couldn't fint {name}")
        {
        }
    }
}