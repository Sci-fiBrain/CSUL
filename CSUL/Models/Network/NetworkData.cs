/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: NetworkData.cs
 *  创建时间: 2023年12月13日 23:59
 *  创建开发: ScifiBrain
 *  文件介绍: 涉及网络相关数据处理的静态类
 *  --------------------------------------
 */

using CSUL.Models.Network.CB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSUL.Models.Network
{
    /// <summary>
    /// 涉及网络相关数据处理的静态类
    /// </summary>
    internal static class NetworkData
    {
        private const string netBepVersionUrl = "https://minebbs-1251544790.file.myqcloud.com/bepinex.html";    //BepInEx版本信息获取地址
        private const string ApiKey = "Nj-arIg6bYmyhnUYMY0SW72_6a7ruUcY";                                       //论坛Api key

        /// <summary>
        /// 获取BepInEx文件的版本信息
        /// </summary>
        /// <returns>包含BepInEx文件信息的集合</returns>
        public static IEnumerable<NetBepInfo> GetNetBepInfos()
        {
            using HttpClient http = new();
            string data = http.GetStringAsync(netBepVersionUrl).Result;
            string info = data[(data.IndexOf("[Start]") + 7)..data.IndexOf("[End]")].Replace("<br>", "").Trim();
            IEnumerable<NetBepInfo> infos = from line in info.Split('\n')
                                            let item = line.Split(' ')
                                            select new NetBepInfo()
                                            {
                                                Version = item[0],
                                                IsBeta = item[1].Contains("beta"),
                                                FileName = item[2],
                                                Uri = item[3]
                                            };
            return infos;
        }

        /// <summary>
        /// 下载文件到指定流
        /// </summary>
        /// <param name="uri">文件链接</param>
        /// <param name="stream">文件要下载到的流</param>
        /// <param name="DownloadProgress">一个委托 返回已下载的字节数</param>
        /// <param name="api">是否需要添加CSLBBS API请求头</param>
        public static async Task DownloadFromUri(string uri, Stream stream, Action<long>? DownloadProgress = null, bool api = false)
        {
            using HttpClient http = new();
            if (api) http.DefaultRequestHeaders.Add("XF-API-Key", ApiKey);
            using Stream webStream = await http.GetStreamAsync(uri);
            long total = 0;
            while (true)
            {
                byte[] buffer = new byte[1024];
                int size = await webStream.ReadAsync(buffer);
                if (size <= 0) break;
                DownloadProgress?.Invoke(total += size);
                await stream.WriteAsync(buffer.AsMemory()[..size]);
            }
            await stream.FlushAsync();
        }

        /// <summary>
        /// 获取CSUL的最新版本
        /// </summary>
        /// <returns>CSUL的最新版本</returns>
        public static async Task<Version?> GetCsulLastestVersion()
        {
            CbResourceData? data = await GetCbResourceData(91);
            if (data is null) return null;
            if (Version.TryParse(data.ResourceVersion, out Version? version)) return version;
            return null;
        }

        /// <summary>
        /// 获取CSLBBS资源数据
        /// </summary>
        /// <param name="id">资源id</param>
        public static async Task<CbResourceData?> GetCbResourceData(int id)
        {
            string url = "https://www.cslbbs.net/api/resources/" + id;
            using HttpClient http = new();
            http.DefaultRequestHeaders.Add("XF-API-Key", ApiKey);
            using Stream stream = await http.GetStreamAsync(url);
            using JsonDocument json = JsonDocument.Parse(stream);
            JsonElement element = json.RootElement.GetProperty("resource");
            CbResourceData? data = element.Deserialize<CbResourceData>();
            return data;
        }
    }
}