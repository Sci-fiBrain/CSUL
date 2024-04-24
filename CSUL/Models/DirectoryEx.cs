/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: DirectoryEx.cs
 *  创建时间: 2023年12月14日 20:58
 *  创建开发: ScifiBrain
 *  文件介绍: Directory扩展类 提供Directory的一些扩展方法
 *  --------------------------------------
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSUL.Models
{
    /// <summary>
    /// Directory扩展类 提供Directory的一些扩展方法
    /// </summary>
    internal static class DirectoryEx
    {
        #region ---文件夹复制---

        /// <summary>
        /// 递归复制文件夹
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="destPath">目标路径</param>
        /// <param name="overwrite">是否覆写</param>
        public static void CopyTo(string path, string destPath, bool overwrite = false)
            => CopyTo(new DirectoryInfo(path), destPath, overwrite);

        /// <summary>
        /// 递归复制文件夹
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="destPath">目标路径</param>
        /// <param name="overwrite">是否覆写</param>
        public static void CopyTo(this DirectoryInfo dir, string destPath, bool overwrite = false)
        {
            if (!dir.Exists) throw new DirectoryNotFoundException(dir.FullName);
            void RecursionCopy(DirectoryInfo root, string relativePath)
            {   //递归复制
                DirectoryInfo total = new(Path.Combine(destPath, relativePath));
                if (!total.Exists) total.Create();
                foreach (FileInfo file in root.GetFiles())
                    file.CopyTo(Path.Combine(destPath, relativePath, file.Name), overwrite);
                foreach (DirectoryInfo dir in root.GetDirectories())
                    RecursionCopy(dir, relativePath + $"{dir.Name}\\");
            }
            RecursionCopy(dir, "");
        }

        #endregion ---文件夹复制---

        #region ---递归获取文件夹下的所有文件---

        /// <summary>
        /// 递归获取文件夹下的所有文件
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <param name="Ignored">忽略的文件夹</param>
        public static FileInfo[] GetAllFiles(string dirPath, params string[] Ignored)
            => GetAllFiles(new DirectoryInfo(dirPath), Ignored.Select(x => new DirectoryInfo(x)).ToArray());

        /// <summary>
        /// 递归获取文件夹下的所有文件
        /// </summary>
        /// <param name="dir">文件夹</param>
        /// <param name="Ignored">忽略的文件夹</param>
        public static FileInfo[] GetAllFiles(this DirectoryInfo dir, params DirectoryInfo[] Ignored)
        {
            if (!dir.Exists) throw new DirectoryNotFoundException(dir.FullName);
            List<FileInfo> files = new();
            void RecursionSearch(DirectoryInfo root)
            {
                files.AddRange(root.GetFiles());
                foreach (DirectoryInfo dir in root.GetDirectories().Where(x => !Ignored.Any(y => x.FullName == y.FullName)))
                {
                    RecursionSearch(dir);
                }
            }
            RecursionSearch(dir);
            return files.ToArray();
        }

        #endregion ---递归获取文件夹下的所有文件---

        #region ---判断是否为文件夹或者文件---

        /// <summary>
        /// 判断该路径是否为文件夹
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>true: 为文件夹, false: 为文件</returns>
        /// <exception cref="FileNotFoundException">该路径不存在</exception>
        public static bool IsDirectory(string path)
        {
            if (Directory.Exists(path)) return true;
            else if (File.Exists(path)) return false;
            else throw new FileNotFoundException(path);
        }

        #endregion ---判断是否为文件夹或者文件---
    }
}