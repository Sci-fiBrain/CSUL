/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: CbTypes.cs
 *  创建时间: 2023年12月16日 10:04
 *  创建开发: ScifiBrain
 *  文件介绍: CSLBBS传参类型
 *  --------------------------------------
 */

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSUL.Models.Network.CB
{
    /// <summary>
    /// CSLBBS资源数据
    /// </summary>
    internal class CbResourceData
    {
        #region ---公共属性---

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
        [JsonPropertyName("custom_fields")]
        [JsonConverter(typeof(CustomConverter))]
        public CbCustomInfo CustomInfo { get; set; } = default!;

        #endregion ---公共属性---

        #region ---转换类型---

        #region --时间转换--

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

        #endregion --时间转换--

        #region --自定义字段转换--

        private class CustomConverter : JsonConverter<CbCustomInfo>
        {
            public override CbCustomInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                CbResourceType resourceType = CbResourceType.Default;
                try
                {
                    JsonElement json = JsonElement.ParseValue(ref reader);

                    //地图存档
                    if (json.TryGetProperty("Map_or_Save", out JsonElement mapOrSave))
                    {
                        switch (mapOrSave.GetString())
                        {
                            case "Map": resourceType = CbResourceType.Map; break;
                            case "Save": resourceType = CbResourceType.Save; break;
                        }
                        return new CbCustomInfo() { ResourceType = resourceType };
                    }

                    //BepInEx模组
                    if (json.TryGetProperty("modLoaderItem", out JsonElement loader))
                    {
                        resourceType = CbResourceType.BepMod;
                        int[] bepVersion = loader.EnumerateObject().First().Name switch
                        {   //获取加载器版本
                            "5and6" => new int[] { 5, 6 },
                            "5" => new int[] { 5 },
                            "6" => new int[] { 6 },
                            _ => Array.Empty<int>()
                        };
                        int[]? dependBepIds = null;
                        if (json.TryGetProperty("depend_mod", out JsonElement depends))
                        {   //有前置模组
                            try
                            {
                                dependBepIds = depends.EnumerateObject().Select(x => int.Parse(x.Name)).ToArray();
                            }
                            catch { }
                        }
                        return new CbCustomBepModInfo() { ResourceType = resourceType, BepInExVersion = bepVersion, DependBepModIds = dependBepIds };
                    }
                }
                catch { }
                return new CbCustomInfo { ResourceType = resourceType };
            }

            public override void Write(Utf8JsonWriter writer, CbCustomInfo value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        #endregion --自定义字段转换--

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
        /// 前置BepInEx模组的Id
        /// </summary>
        public int[]? DependBepModIds { get; set; }

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