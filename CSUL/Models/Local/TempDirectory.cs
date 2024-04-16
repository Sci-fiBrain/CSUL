/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: TempDirectory.cs
 *  创建时间: 2023年12月14日 13:03
 *  创建开发: ScifiBrain
 *  文件介绍: 临时文件夹类
 *  --------------------------------------
 */

using CSUL.Models.Network.CB;
using SharpCompress.Archives;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CSUL.Models.Local
{
    /// <summary>
    /// 临时文件夹类
    /// </summary>
    internal class TempDirectory : IDisposable
    {
        #region ---构造函数---

        /// <summary>
        /// 获取<see cref="TempDirectory"/>对象实例
        /// </summary>
        public TempDirectory()
        {
            string tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempFile");
            FullName = Path.Combine(tempDirPath, $"tempDir{GetHashCode()}");
            Directory.CreateDirectory(FullName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (Directory.Exists(FullName)) Directory.Delete(FullName, true);
        }

        #endregion ---构造函数---

        #region ---公共属性---

        /// <summary>
        /// 临时文件夹所处路径
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// 临时文件夹信息实例
        /// </summary>
        public DirectoryInfo DirectoryInfo { get => new(FullName); }

        /// <summary>
        /// 该临时文件夹是否只有单个文件
        /// </summary>
        public bool OnlySingleFile { get => Directory.GetFiles(FullName).Length == 1 && Directory.GetDirectories(FullName).Length <= 0; }

        /// <summary>
        /// 该临时文件夹是否为空
        /// </summary>
        public bool IsEmpty { get => Directory.GetFiles(FullName).Length <= 0 && Directory.GetDirectories(FullName).Length <= 0; }

        #endregion ---公共属性---

        #region ---公共方法---

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="file">要解压的文件路径</param>
        public async Task Decompress(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException(file);
            await Task.Run(() =>
            {
                using Stream stream = File.OpenRead(file);
                using IArchive archive = ArchiveFactory.Open(stream);
                archive.WriteToDirectory(FullName, new()
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            });
        }

        /// <summary>
        /// 添加一个文件
        /// </summary>
        /// <param name="file">文件路径</param>
        public async Task AddFile(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException(file);
            string fileName = Path.GetFileName(file);
            await Task.Run(() => File.Copy(file, Path.Combine(FullName, fileName)));
        }

        #endregion ---公共方法---
    }
}