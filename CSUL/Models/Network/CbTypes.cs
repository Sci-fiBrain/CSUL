/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: CbTypes.cs
 *  创建时间: 2023年12月16日 10:04
 *  创建开发: ScifiBrain
 *  文件介绍: CSLBBS传参类型
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSUL.Models.Network.CB
{
    /// <summary>
    /// CSLBBS资源数据
    /// </summary>
    internal class CbResourceData
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        [JsonPropertyName("type")]
        public string ResourceType { get; set; } = default!;

        /// <summary>
        /// 前缀类型
        /// </summary>
        public string Prefix { get; set; } = default!;

        /// <summary>
        /// 资源名称
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// 资源描述
        /// </summary>
        public string TagLine { get; set; } = default!;

        /// <summary>
        /// 图标地址
        /// </summary>
        public string IconUrl { get; set; } = default!;

        /// <summary>
        /// 资源版本
        /// </summary>
        public string Version { get; set; } = default!;

        /// <summary>
        /// 文件信息
        /// </summary>
        public CbFileData[]? CurrentFiles { get; set; }

        /// <summary>
        /// 支持的加载器版本（5或6）
        /// </summary>
        public int[]? ModLoader { get; set; }
    }

    /// <summary>
    /// CSLBBS资源文件数据
    /// </summary>
    internal class CbFileData
    {
        /// <summary>
        /// 文件Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; } = default!;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 下载地址
        /// </summary>
        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; } = default!;
    }
}
