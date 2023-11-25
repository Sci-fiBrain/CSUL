using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                foreach(DirectoryInfo dir in root.GetDirectories())
                    RecursionCopy(dir, relativePath += $"{dir.Name}\\");
            }
            RecursionCopy(dir, "");
        }
    }
}
