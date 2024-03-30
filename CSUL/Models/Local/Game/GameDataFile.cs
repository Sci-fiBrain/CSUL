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
using System.Text;
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
            if (path.EndsWith(".Prefab")) return GameDataFileType.Asset;
            else if (!path.EndsWith(".cok")) return GameDataFileType.Unknown;
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
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task Install(params string[] filePaths)
        {
            string saveDir = ComParameters.Instance.Saves.FullName;
            string mapDir = ComParameters.Instance.Maps.FullName;
            string assetDir = ComParameters.Instance.Asset.FullName;

            foreach (string filePath in filePaths)
            {
                if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
                InstalledGameDataFiles ret = new();
                using TempDirectory package = new();
                if (filePath.EndsWith(".cok")) await package.AddFile(filePath);//允许直接安装单个cok文件
                else if (filePath.EndsWith(".Prefab")) await package.AddFile(filePath); //允许安装资产文件
                else await package.Decompress(filePath);                           //允许直接安装压缩包文件
                foreach (FileInfo file in package.DirectoryInfo.GetAllFiles())
                {
                    try
                    {
                        List<string> names;
                        string targetPath;
                        switch (GetGameDataFileType(file.FullName))
                        {
                            case GameDataFileType.Save:
                                targetPath = saveDir;
                                names = ret.SaveNames;
                                break;

                            case GameDataFileType.Map:
                                targetPath = mapDir;
                                names = ret.MapNames;
                                break;

                            case GameDataFileType.Asset:
                                targetPath = assetDir;
                                names = ret.AssetNames;
                                break;

                            default: continue;
                        }
                        if (targetPath is null) continue;
                        string fileName = file.Name;
                        if (File.Exists(Path.Combine(targetPath, fileName))) await Task.Run(() =>
                        {   //获得不重复的文件名
                            int index = fileName.LastIndexOf('.');
                            string reEx = fileName[..index];
                            string exName = fileName[index..];
                            for (long i = 1; i < long.MaxValue; i++)
                            {
                                if (!File.Exists(Path.Combine(targetPath, $"{reEx}({i}){exName}")))
                                {
                                    fileName = $"{reEx}({i}){exName}";
                                    break;
                                }
                            }
                        });
                        await Task.Run(() => File.Copy(file.FullName, Path.Combine(targetPath, fileName), true));
                        if (File.Exists(file.FullName + ".cid")) await Task.Run(() => File.Copy(file.FullName + ".cid", Path.Combine(targetPath, fileName + ".cid"), true));
                        names.Add(fileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToFormative($"{filePath}中的{file.Name}安装时出现问题"), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                //安装结果提示
                StringBuilder builder = new();
                builder.Append($"文件{Path.GetFileName(filePath)}解析完成").AppendLine();
                builder.Append($"已安装地图 {ret.MapNames.Count} 个: ").AppendLine();
                ret.MapNames.ForEach(x => builder.AppendLine(x));
                builder.AppendLine();

                builder.Append($"已安装存档 {ret.SaveNames.Count} 个: ").AppendLine();
                ret.SaveNames.ForEach(x => builder.AppendLine(x));
                builder.AppendLine();

                builder.Append($"已安装资产 {ret.AssetNames.Count} 个: ").AppendLine();
                ret.AssetNames.ForEach(x => builder.AppendLine(x));
                builder.AppendLine();
                MessageBox.Show(builder.ToString(), $"提示", MessageBoxButton.OK, MessageBoxImage.Information);
                //
            }
        }

        #endregion ---安装游戏数据文件---
    }
}