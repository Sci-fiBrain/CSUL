/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: InstalledGameDataFiles.cs
 *  创建时间: 2023年12月14日 13:20
 *  创建开发: ScifiBrain
 *  文件介绍: 安装完成的游戏数据文件路径
 *  --------------------------------------
 */

using System.Collections.Generic;

namespace CSUL.Models.Local.Game
{
    /// <summary>
    /// 安装完成的游戏数据文件路径
    /// </summary>
    public readonly struct InstalledGameDataFiles
    {
        /// <summary>
        /// 已安装的地图名称
        /// </summary>
        public List<string> MapNames { get; init; }

        /// <summary>
        /// 已安装的存档名称
        /// </summary>
        public List<string> SaveNames { get; init; }
    }
}