/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: GameDataFileInfo.cs
 *  创建时间: 2023年12月14日 13:11
 *  创建开发: ScifiBrain
 *  文件介绍: 游戏数据文件信息类
 *  --------------------------------------
 */

using System.IO;

namespace CSUL.Models.Local.Game
{
    /// <summary>
    /// 游戏数据文件信息类
    /// </summary>
    public class GameDataFileInfo
    {
        #region ---构造函数---

        /// <summary>
        /// 实例化<see cref="GameDataFileInfo"/>类
        /// </summary>
        /// <param name="path">游戏数据文件(.cok)路径</param>
        public GameDataFileInfo(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            DataType = GameDataFile.GetGameDataFileType(path);
            cokPath = path;
            cidPath = path + ".cid";
            if (File.Exists(cidPath))
            {
                using StreamReader reader = new(File.OpenRead(cidPath));
                Cid = reader.ReadToEnd();
            }
            else { Cid = "未找到cid文件"; }
            string name = Path.GetFileName(path);
            Name = name[..name.LastIndexOf('.')];
            LastWriteTime = File.GetLastWriteTime(path).ToString("yyyy-MM-dd-HH:mm:ss");
        }

        #endregion ---构造函数---

        #region ---公共属性---

        /// <summary>
        /// 游戏文件类型
        /// </summary>
        public GameDataFileType DataType { get; }

        /// <summary>
        /// 数据文件Id
        /// </summary>
        public string? Cid { get; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get => cokPath; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public string LastWriteTime { get; }

        #endregion ---公共属性---

        #region ---私有字段---

        /// <summary>
        /// 游戏数据文件路径
        /// </summary>
        private readonly string cokPath;

        /// <summary>
        /// 数据文件Id路径
        /// </summary>
        private readonly string cidPath;

        #endregion ---私有字段---

        #region ---公共方法---

        /// <summary>
        /// 删除该文件
        /// </summary>
        public void Delete()
        {
            if (File.Exists(cokPath)) File.Delete(cokPath);
            if (File.Exists(cidPath)) File.Delete(cidPath);
        }

        #endregion ---公共方法---
    }
}