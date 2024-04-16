/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: FileEx.cs
 *  创建时间: 2024年4月17日 0:30
 *  创建开发: ScifiBain
 *  文件介绍: 文件扩展方法
 *  --------------------------------------
 */

using System.IO;

namespace CSUL.Models
{
    internal static class FileEx
    {
        /// <summary>
        /// 文件是否正在被使用
        /// </summary>
        public static bool IsInUse(this FileInfo file) => IsInUse(file.FullName);

        /// <summary>
        /// 文件是否正在被使用
        /// </summary>
        /// <param name="path">文件路径</param>
        public static bool IsInUse(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            try
            {
                using FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}