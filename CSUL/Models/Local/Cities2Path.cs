/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: Cities2Path.cs
 *  创建时间: 2023年12月14日 12:15
 *  创建开发: ScifiBrain
 *  文件介绍: 天际线2的路径获取类
 *  --------------------------------------
 */

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace CSUL.Models.Local
{
    /// <summary>
    /// 天际线2路径类
    /// </summary>
    internal static class Cities2Path
    {
        #region ---公共方法---

        #region --游戏路径获取--

        /// <summary>
        /// 尝试从Steam获取天际线2路径
        /// </summary>
        /// <param name="path">天际线2路径</param>
        /// <returns>是否获取成功</returns>
        public static bool TryFromSteam([NotNullWhen(true)] out string? path)
        {
            try
            {
                path = GetFromSteam("Cities Skylines II");
                return true;
            }
            catch
            {
                path = null;
                return false;
            }
        }

        /// <summary>
        /// 尝试从微软商店获取天际线2路径
        /// </summary>
        /// <param name="path">天际线2路径</param>
        /// <returns>是否获取成功</returns>
        public static bool TryFromMicrosoft([NotNullWhen(true)] out string? path)
        {
            path = null;
            return false;
        }

        /// <summary>
        /// 尝试从Xbox获取天际线2路径
        /// </summary>
        /// <param name="path">天际线2路径</param>
        /// <returns>是否获取成功</returns>
        public static bool TryFromXbox([NotNullWhen(true)] out string? path)
        {
            path = null;
            return false;
        }

        #endregion --游戏路径获取--

        #region --游戏数据路径获取--

        /// <summary>
        /// 尝试获取天际线2数据路径
        /// </summary>
        /// <param name="path">天际线2数据路路径</param>
        /// <returns>是否获取成功</returns>
        public static bool TryGetGameDataPath([NotNullWhen(true)] out string? path)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string localLow = Path.Combine(Path.GetDirectoryName(appData)!, "LocalLow");
                path = Path.Combine(localLow, "Colossal Order", "Cities Skylines II");
                if (!Directory.Exists(path)) throw new IOException("Cities Skylines II is not existed");
                return true;
            }
            catch
            {
                path = null;
                return false;
            }
        }

        #endregion --游戏数据路径获取--

        #region --游戏平台路径获取--

        /// <summary>
        /// 尝试获取Steam路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TryGetSteamPath([NotNullWhen(true)] out string? path)
        {
            try
            {
                path = Path.Combine(GetSteamDirPath(), "steam.exe");
                return true;
            }
            catch
            {
                path = null;
                return false;
            }
        }

        #endregion --游戏平台路径获取--

        #endregion ---公共方法---

        #region ---私有方法---

        /// <summary>
        /// 获取Steam安装文件夹路径
        /// </summary>
        /// <returns></returns>
        private static string GetSteamDirPath()
        {
            const string steamRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam";
            string steamPath = Registry.GetValue(steamRegistryKey, "InstallPath", null) as string
                ?? throw new RegistryNotFoundException("steam");
            return steamPath;
        }

        /// <summary>
        /// 从Steam获取游戏路径
        /// </summary>
        /// <param name="gameName">游戏名称</param>
        /// <returns>游戏路径</returns>
        private static string GetFromSteam(string gameName)
        {
            string vdfPath = Path.Combine(GetSteamDirPath(), "steamapps", "libraryfolders.vdf");
            using Stream stream = new FileStream(vdfPath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new(stream);
            List<string> librayPaths = new(); //Steam所有游戏库的路径
            while (!reader.EndOfStream)
            {
                if (reader.ReadLine()?.Trim('\t') is not string data) continue;
                if (!data.Contains("\"path\"")) continue;
                string rootPath = data.TrimEnd('\"').Split('\"').Last();
                librayPaths.Add(Path.Combine(rootPath.Replace(@"\\", @"\"), @"steamapps\common"));
            }
            IEnumerable<string> gamePaths = librayPaths.SelectMany(Directory.GetDirectories);   //Steam所有游戏的路径
            string gamePath = gamePaths.FirstOrDefault(x => x.Contains(gameName))
                ?? throw new FileNotFoundException("Get game install path from steam failed", gameName);
            return gamePath;
        }

        #endregion ---私有方法---
    }
}