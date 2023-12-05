//与Web相关的结构体类
namespace CSUL.Models.Structs
{
    #region ---BepInEx文件信息---

    /// <summary>
    /// Bepinex文件信息
    /// </summary>
    public struct BepinexInfo
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 是否为测试版
        /// </summary>
        public bool IsBeta { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 下载链接
        /// </summary>
        public string Uri { get; set; }
    }

    #endregion ---BepInEx文件信息---
}