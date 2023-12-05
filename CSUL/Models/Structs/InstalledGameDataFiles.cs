using System.Collections.Generic;

namespace CSUL.Models.Structs
{
    /// <summary>
    /// 安装完成的游戏数据文件
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