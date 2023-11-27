using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
                        Assembly assembly = Assembly.LoadFrom(file.FullName);
                        AssemblyName? bep = assembly.GetReferencedAssemblies().FirstOrDefault(x =>
                            x.Name?.Contains("BepInEx") is true);
                        version = bep?.Version;
                        break;
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
    }
}