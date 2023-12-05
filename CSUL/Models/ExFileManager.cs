using CSUL.Models.Enums;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace CSUL.Models
{
    /// <summary>
    /// 文件管理扩展类
    /// </summary>
    public static class ExFileManager
    {
        /// <summary>
        /// 递归复制文件夹
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="path"></param>
        public static void CopyTo(this DirectoryInfo dir, string path)
        {
            void RecursionCopy(DirectoryInfo root, string relativePath)
            {   //递归复制
                DirectoryInfo total = new(Path.Combine(path, relativePath));
                if (!total.Exists) total.Create();
                foreach (FileInfo file in root.GetFiles())
                    file.CopyTo(Path.Combine(path, relativePath, file.Name), true);
                foreach (DirectoryInfo dir in root.GetDirectories())
                    RecursionCopy(dir, relativePath += $"{dir.Name}\\");
            }
            RecursionCopy(dir, "");
        }

        /// <summary>
        /// 递归获取文件夹下的所有文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static FileInfo[] GetAllFiles(this DirectoryInfo dir)
        {
            List<FileInfo> files = new();
            void RecursionSearch(DirectoryInfo root)
            {
                files.AddRange(root.GetFiles());
                foreach (DirectoryInfo dir in root.GetDirectories())
                    RecursionSearch(dir);
            }
            RecursionSearch(dir);
            return files.ToArray();
        }

        /// <summary>
        /// 尝试获取BepInEx模组的BepInEx的版本
        /// </summary>
        /// <param name="path">模组安装路径</param>
        /// <returns>模组版本</returns>
        public static Version? GetBepModVersion(string path)
        {
            static Version? RecursionSearch(string root)
            {   //递归搜索
                Version? version = null;
                DirectoryInfo dir = new(root);
                //防止文件夹不存在
                if (!dir.Exists) return null;
                //找到当前文件夹的所有dll文件
                IEnumerable<FileInfo> files = from file in dir.GetFiles()
                                              where file.Name.EndsWith(".dll")
                                              select file;
                foreach (FileInfo file in files)
                {   //搜索BepInEx版本信息
                    try
                    {
                        version = GetFileModVersion(file.FullName);
                        if (version is not null) break;
                    }
                    catch { }
                }
                if (version is null)
                {   //当前文件夹没有搜索到
                    foreach (DirectoryInfo cDir in dir.GetDirectories())
                    {   //递归搜索子文件夹
                        version = RecursionSearch(cDir.FullName);
                        if (version is not null) break;
                    }
                }
                return version;
            }
            Version? ret = RecursionSearch(path);
            return ret;
        }

        /// <summary>
        /// 获取单个模组文件的BepInEx版本信息
        /// </summary>
        /// <param name="filePath">模组文件*.dll路径</param>
        /// <returns>模组BepInEx版本</returns>
        public static Version? GetFileModVersion(string filePath)
        {
            Version? version = null;
            try
            {
                AssemblyLoadContext custom = new(null, true);
                using Stream stream = File.OpenRead(filePath);
                Assembly assembly = custom.LoadFromStream(stream);
                AssemblyName? bep = assembly.GetReferencedAssemblies().FirstOrDefault(x =>
                    x.Name?.Contains("BepInEx") is true);
                version = bep?.Version;
                custom.Unload();
            }
            catch { }
            return version;
        }

        /// <summary>
        /// 检查Mod的BepInEx版本
        /// </summary>
        /// <param name="path">mod路径</param>
        /// <param name="modVerison">mod的BepInEx版本</param>
        /// <param name="bepInExVersion">BepInEx的版本</param>
        /// <param name="isFile">是否为单个文件</param>
        /// <param name="knowBepVersion">已知的BepInEx版本号</param>
        /// <returns></returns>
        public static BepInExCheckResult ChickModBepInExVersioin(string path, out Version? modVerison, out Version? bepInExVersion, bool isFile = false, Version? knowBepVersion = null)
        {
            //区分单文件和子文件夹模组
            if (isFile) modVerison = GetFileModVersion(path);
            else modVerison = GetBepModVersion(path);

            //已知版本，加快检查速度
            if (knowBepVersion is not null) bepInExVersion = knowBepVersion;
            else bepInExVersion = FileManager.Instance.BepVersion;

            if (modVerison is null) return BepInExCheckResult.UnkownMod;
            if (bepInExVersion is null) return BepInExCheckResult.UnknowBepInEx;
            if (modVerison.Major != bepInExVersion.Major) return BepInExCheckResult.WrongVersion;
            return BepInExCheckResult.Passed;
        }

        /// <summary>
        /// 获取游戏文件类型
        /// </summary>
        /// <param name="path">游戏文件路径</param>
        public static GameDataFileType GetGameDataFileType(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            using Stream stream = File.OpenRead(path);
            return GetGameDataFileType(stream);
        }

        /// <summary>
        /// 获取游戏文件路径
        /// </summary>
        /// <param name="stream">包含游戏文件的流</param>
        public static GameDataFileType GetGameDataFileType(Stream stream)
        {
            try
            {
                IEnumerable<ZipArchiveEntry> entrys = ZipArchive.Open(stream).Entries;
                if (entrys.Any(x => x.Key.EndsWith(".MapData")))
                    return GameDataFileType.Map;
                else if (entrys.Any(x => x.Key.EndsWith(".SaveGameData")))
                    return GameDataFileType.Save;
                else return GameDataFileType.Unknown;
            }
            catch { return GameDataFileType.Unknown; }
        }
    }
}