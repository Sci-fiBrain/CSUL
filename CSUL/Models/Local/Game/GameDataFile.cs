/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: GameDataFile.cs
 *  创建时间: 2023年12月14日 13:12
 *  创建开发: ScifiBrain
 *  文件介绍: 游戏数据文件静态类 用于提供游戏数据文件的静态方法
 *  --------------------------------------
 */

using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSUL.Models.Local.Game
{
    /// <summary>
    /// 游戏数据文件静态类
    /// </summary>
    internal static class GameDataFile
    {
        #region ---获取游戏文件类型---

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
        /// 获取游戏文件类型
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

        #endregion ---获取游戏文件类型---

        #region ---安装游戏数据文件---

        /// <summary>
        /// 安装游戏数据文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="saveDir">存档文件夹路径</param>
        /// <param name="mapDir">地图文件夹路径</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task<InstalledGameDataFiles> Install(string filePath, string saveDir, string mapDir)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
            InstalledGameDataFiles ret = new() { MapNames = new(), SaveNames = new() };
            using TempDirectory package = new();
            if (filePath.EndsWith(".cok")) await package.AddFile(filePath);//允许直接安装单个cok文件
            else await package.Decompress(filePath);                           //允许直接安装压缩包文件
            IEnumerable<FileInfo> coks = package.DirectoryInfo.GetAllFiles().Where(x => x.Extension == ".cok");
            foreach (FileInfo cok in coks)
            {
                try
                {
                    List<string> names;
                    string targetPath;
                    switch (GetGameDataFileType(cok.FullName))
                    {
                        case GameDataFileType.Save:
                            targetPath = saveDir;
                            names = ret.SaveNames;
                            break;

                        case GameDataFileType.Map:
                            targetPath = mapDir;
                            names = ret.MapNames;
                            break;

                        default: continue;
                    }
                    string cokName = cok.Name;
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
                    await Task.Run(() => File.Copy(cok.FullName, Path.Combine(targetPath, cokName), true));
                    if (File.Exists(cok.FullName + ".cid")) await Task.Run(() => File.Copy(cok.FullName + ".cid", Path.Combine(targetPath, cokName + ".cid"), true));
                    names.Add(cokName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToFormative($"{filePath}中的{cok.Name}安装时出现问题"), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return ret;
        }

        #endregion ---安装游戏数据文件---
    }
}