/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: CbTypes.cs
 *  创建时间: 2023年12月16日 10:04
 *  创建开发: ScifiBrain
 *  文件介绍: CSLBBS传参类型
 *  --------------------------------------
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSUL.Models.Network.CB
{
    /// <summary>
    /// CSLBBS资源数据
    /// </summary>
    internal class CbResourceData
    {
        /// <summary>
        /// 资源Id
        /// </summary>
        [JsonPropertyName("resource_id")]
        public int Id { get; set; }

        /// <summary>
        /// 资源标题
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = default!;

        /// <summary>
        /// 资源版本
        /// </summary>
        [JsonPropertyName("version")]
        public string ResourceVersion { get; set; } = default!;

        /// <summary>
        /// 资源前缀
        /// </summary>
        [JsonPropertyName("prefix")]
        public string Prefix { get; set; } = default!;

        /// <summary>
        /// 资源地址
        /// </summary>
        [JsonPropertyName("view_url")]
        public string ResourceUrl { get; set; } = default!;

        /// <summary>
        /// 资源更新时间
        /// </summary>
        [JsonPropertyName("last_update")]
        [JsonConverter(typeof(TimeConverter))]
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 资源更新次数
        /// </summary>
        [JsonPropertyName("update_count")]
        public int UpdateCount { get; set; }

        /// <summary>
        /// 资源发布时间
        /// </summary>
        [JsonPropertyName("resource_date")]
        [JsonConverter(typeof(TimeConverter))]
        public DateTime PublishTime { get; set; }

        /// <summary>
        /// 资源描述
        /// </summary>
        [JsonPropertyName("tag_line")]
        public string Desciption { get; set; } = default!;

        /// <summary>
        /// 资源文件
        /// </summary>
        [JsonPropertyName("current_files")]
        public CbFileData[]? Files { get; set; }

        /// <summary>
        /// 资源扩展信息
        /// </summary>
        [JsonIgnore]
        public CbCustomInfo CustomInfo { get; set; } = default!;

        #region ---转换类型---

        private class TimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TryGetInt64(out long value)) return DateTimeOffset.FromUnixTimeSeconds(value).LocalDateTime;
                return default!;
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(new DateTimeOffset(value).ToUnixTimeSeconds());
            }
        }

        #endregion ---转换类型---
    }

    /// <summary>
    /// CSLBBS自定义字段信息
    /// </summary>
    internal class CbCustomInfo
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        public CbResourceType ResourceType { get; set; }
    }

    /// <summary>
    /// BepInEx模组数据
    /// </summary>
    internal class CbCustomBepModInfo : CbCustomInfo
    {
        /// <summary>
        /// BepInEx版本
        /// </summary>
        public int[]? BepInExVersion { get; set; }

        /// <summary>
        /// 前置BepInEx模组
        /// </summary>
        public CbResourceData[]? DependBepMods { get; set; }

        /// <summary>
        /// 支持的游戏版本
        /// </summary>
        public string GameVersion { get; set; } = default!;
    }

    /// <summary>
    /// CSLBBS文件数据
    /// </summary>
    public class CbFileData
    {
        [JsonPropertyName("id")]
        public int FileId { get; set; }

        [JsonPropertyName("filename")]
        public string FileName { get; set; } = default!;

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("download_url")]
        public string Url { get; set; } = default!;
    }

    /// <summary>
    /// CSLBBS资源类型
    /// </summary>
    internal enum CbResourceType
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default,

        BepMod,
        Map,
        Save,

        /// <summary>
        /// 自定义电台
        /// </summary>
        CustomRadios
    }
}