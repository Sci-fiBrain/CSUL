using CSUL.Models.Enums;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSUL.Models
{
    /// <summary>
    /// 临时文件包类 用于提供一个临时的文件夹储存文件
    /// </summary>
    public class TempPackage : IDisposable
    {
        #region ---构造函数---

        /// <summary>
        /// 实例化<see cref="TempPackage"/>对象
        /// </summary>
        /// <param name="dirPath">文件要存储到的文件夹路径</param>
        /// <param name="file">要解压的文件</param>
        public TempPackage(string dirPath) => Initialize(dirPath);

        /// <summary>
        /// 实例化<see cref="TempPackage"/>对象
        /// </summary>
        /// <param name="file">要解压的文件</param>
        public TempPackage() => Initialize(Path.Combine(_tempDirPath, $"pm{Random.Shared.Next()}"));

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize(string dirPath)
        {
            fullName = dirPath;
            if (Directory.Exists(fullName)) Directory.Delete(fullName, true);
            Directory.CreateDirectory(fullName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (Directory.Exists(fullName)) Directory.Delete(fullName, true);
        }

        #endregion ---构造函数---

        #region ---私有字段---

        /// <summary>
        /// 解压文件所处的文件夹路径
        /// </summary>
        private string fullName = default!;

        /// <summary>
        /// 临时文件夹路径
        /// </summary>
        private static readonly string _tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempFile");

        #endregion ---私有字段---

        #region ---公共属性---

        /// <summary>
        /// 临时文件夹的路径
        /// </summary>
        public string FullName { get => fullName; }

        /// <summary>
        /// 只有单个文件
        /// </summary>
        public bool OnlySingleFile { get => Directory.GetFiles(fullName).Length == 1; }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEempty { get => Directory.GetFiles(fullName).Length <= 0 && Directory.GetDirectories(fullName).Length <= 0; }

        #endregion ---公共属性---

        #region ---公共方法---

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="file">要解压的文件路径</param>
        public async Task Decompress(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException(file);
            if (Directory.Exists(fullName)) Directory.Delete(fullName, true);
            Directory.CreateDirectory(fullName);
            await Task.Run(() =>
            {
                using Stream stream = File.OpenRead(file);
                using IArchive archive = ArchiveFactory.Open(stream);
                archive.WriteToDirectory(fullName, new()
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
            await Task.Run(() => File.Copy(file, Path.Combine(fullName, fileName)));
        }

        #endregion ---公共方法---

        #region ---公共静态方法---

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

        #endregion ---公共静态方法---
    }
}