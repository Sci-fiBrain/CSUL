/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: NetworkData.cs
 *  创建时间: 2023年12月13日 23:59
 *  创建开发: ScifiBrain
 *  文件介绍: 涉及网络相关数据处理的静态类
 *  --------------------------------------
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CSUL.Models.Network
{
    /// <summary>
    /// 涉及网络相关数据处理的静态类
    /// </summary>
    internal static class NetworkData
    {
        private const string netBepVersionUrl = "https://minebbs-1251544790.file.myqcloud.com/bepinex.html";    //BepInEx版本信息获取地址

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
        /// 下载文件
        /// </summary>
        /// <param name="uri">文件链接</param>
        /// <param name="path">文件要下载到的路径</param>
        /// <param name="DownloadProgress">一个委托 返回已下载的字节数</param>
        public static async Task DownloadFromUri(string uri, string path, Action<long>? DownloadProgress = null)
        {
            using HttpClient http = new();
            using FileStream file = File.Create(path);
            using Stream stream = await http.GetStreamAsync(uri);
            long total = 0;
            while (true)
            {
                byte[] buffer = new byte[1024];
                int size = await stream.ReadAsync(buffer);
                if (size <= 0) break;
                DownloadProgress?.Invoke(total += size);
                await file.WriteAsync(buffer);
            }
        }

        /// <summary>
        /// 获取CSUL的最新版本
        /// </summary>
        /// <returns>CSUL的最新版本</returns>
        public static async Task<Version?> GetCsulLastestVersion()
        {
            Version? version = null;
            const string key = "Nj-arIg6bYmyhnUYMY0SW72_6a7ruUcY";
            const string uri = "https://www.cslbbs.net/api/resources/91";
            using HttpClient http = new();
            http.DefaultRequestHeaders.Add("XF-API-Key", key);
            using StreamReader stream = new(await http.GetStreamAsync(uri));
            while (!stream.EndOfStream)
            {
                if (stream.ReadLine() is not string line) continue;
                if (line.Trim().StartsWith("\"version\""))
                {
                    string ver = line.Split(':').Last().Trim('"', ' ', ',');
                    version = new Version(ver);
                    break;
                }
            }
            return version;
        }
    }
}