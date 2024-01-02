/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: IModData.cs
 *  创建时间: 2023年12月16日 18:36
 *  创建开发: ScifiBrain
 *  文件介绍: 模组数据接口
 *  --------------------------------------
 */

namespace CSUL.Models.Local.ModPlayer
{
    /// <summary>
    /// 模组数据接口
    /// </summary>
    internal interface IModData
    {
        /// <summary>
        /// 模组名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 模组路径
        /// </summary>
        public string ModPath { get; }

        /// <summary>
        /// 是否启用该模组
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 模组版本
        /// </summary>
        public string? ModVersion { get; }

        /// <summary>
        /// 模组最新版本
        /// </summary>
        public string? LatestVersion { get; }

        /// <summary>
        /// 模组描述
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// 模组地址
        /// </summary>
        public string? ModUrl { get; }
    }
}