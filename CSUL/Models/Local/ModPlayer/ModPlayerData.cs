/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ModPlayerData.cs
 *  创建时间: 2023年12月23日 0:36
 *  创建开发: ScifiBrain
 *  文件介绍: 模组播放器装载后的信息
 *  --------------------------------------
 */

using System.Collections.Generic;

namespace CSUL.Models.Local.ModPlayer
{
    /// <summary>
    /// 模组播放集装载后的信息
    /// </summary>
    internal class ModPlayerData
    {
        /// <summary>
        /// 装载的文件 =>> 卸载时应该被删除的文件
        /// </summary>
        public required IEnumerable<string> Files { get; set; }

        /// <summary>
        /// 装载的文件夹 =>> 卸载时应该被删除的文件夹
        /// </summary>
        public required IEnumerable<string> Directories { get; set; }
    }
}