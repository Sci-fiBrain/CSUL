/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: BepModData.cs
 *  创建时间: 2023年12月16日 18:57
 *  创建开发: ScifiBrain
 *  文件介绍: BepInEx模组信息
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models.Local.ModPlayer.BepInEx
{
    /// <summary>
    /// BepInEx模组信息
    /// </summary>
    internal class BepModData : IModData
    {
        public BepModData(string path)
        {

        }

        public string Name { get; set; }

        public string? ModVersion { get; set; }

        public string? LatestVersion { get; set; }

        public string? Description { get; set; }

        public string? ModUrl { get; set; }
    }
}
