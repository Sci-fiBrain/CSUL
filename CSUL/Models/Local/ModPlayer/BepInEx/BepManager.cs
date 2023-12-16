/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: BepManager.cs
 *  创建时间: 2023年12月14日 21:06
 *  创建开发: ScifiBrain
 *  文件介绍: BepInEx相关的处理方法
 *  --------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace CSUL.Models.Local.ModPlayer.BepInEx
{
    /// <summary>
    /// BepInEx相关的处理方法
    /// </summary>
    internal static class BepManager
    {
        /// <summary>
        /// 尝试获取模组的BepInEx调用版本
        /// </summary>
        /// <param name="path">模组路径 或 模组所处文件夹的路径</param>
        /// <param name="version">使用的BepInEx版本</param>
        /// <returns>是否获取成功</returns>
        public static bool TryGetModBepVersion(string path, [NotNullWhen(true)] out Version? version)
        {
            if (TryGetModBepVersionFromFile(path, out version)) return true;
            else if (TryGetModBepVersionFromDir(path, out version)) return true;
            else return false;
        }

        /// <summary>
        /// 尝试获取BepInEx的版本
        /// </summary>
        /// <param name="bepRootPath">BepInEx根目录</param>
        /// <param name="version">BepInEx版本</param>
        /// <returns>是否获取成功</returns>
        public static bool TryGetBepVersion(string bepRootPath, [NotNullWhen(true)] out Version? version)
        {
            version = null;
            try
            {
                string corePath = Path.Combine(bepRootPath, "BepInEx", "core");
                if (!Directory.Exists(corePath)) return false;
                string? bepCore = Directory.GetFiles(corePath).Select(Path.GetFileName)
                    .FirstOrDefault(x => x!.StartsWith("BepInEx") && x!.EndsWith(".dll"));
                if (bepCore is null) return false;
                FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(Path.Combine(corePath, bepCore));
                string? ver = fileVersion.FileVersion;
                if (ver is null) return false;
                version = Version.Parse(ver);
            }
            catch { }
            return version is not null;
        }

        /// <summary>
        /// 判断BepInEx是否已安装
        /// </summary>
        /// <param name="gameRootPath">游戏安装路径</param>
        public static bool IsInstalled(string gameRootPath)
        {
            if (!Directory.Exists(gameRootPath)) return false;
            return !File.Exists(Path.Combine(gameRootPath, "winhttp.dll"));
        }

        #region ---私有方法---

        /// <summary>
        /// 从文件获取模组调用的BepInEx版本
        /// </summary>
        private static bool TryGetModBepVersionFromFile(string filePath, [NotNullWhen(true)] out Version? version)
        {
            try
            {
                if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
                AssemblyLoadContext custom = new(null, true);
                using Stream stream = File.OpenRead(filePath);
                Assembly assembly = custom.LoadFromStream(stream);
                AssemblyName? bep = assembly.GetReferencedAssemblies().FirstOrDefault(x =>
                    x.Name?.Contains("BepInEx") is true) ?? throw new Exception("模组没有引用BepInEx");
                version = bep.Version ?? throw new Exception("没有得到模组BepInEx的版本信息");
                custom.Unload();
                return true;
            }
            catch
            {
                version = null;
                return false;
            }
        }

        /// <summary>
        /// 从文件夹获取模组调用的BepInEx版本
        /// </summary>
        private static bool TryGetModBepVersionFromDir(string dirPath, [NotNullWhen(true)] out Version? version)
        {
            version = null;
            try
            {
                if (!Directory.Exists(dirPath)) throw new DirectoryNotFoundException(dirPath);
                IEnumerable<FileInfo> dlls = DirectoryEx.GetAllFiles(dirPath).Where(x => x.Name.EndsWith(".dll"));
                foreach (FileInfo dll in dlls) if (TryGetModBepVersionFromFile(dll.FullName, out version)) break;
            }
            catch { }
            return version is not null;
        }

        #endregion ---私有方法---
    }
}