/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: NetBepInfo.cs
 *  创建时间: 2023年12月14日 0:01
 *  创建开发: ScifiBrain
 *  文件介绍: 单个BepInEx文件信息的结构体
 *  --------------------------------------
 */

namespace CSUL.Models.Network
{
    /// <summary>
    /// 单个BepInEx文件信息
    /// </summary>
    internal struct NetBepInfo
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
}