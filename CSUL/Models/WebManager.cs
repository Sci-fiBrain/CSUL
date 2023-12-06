using CSUL.Models.Structs;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CSUL.Models
{
    /// <summary>
    /// Web相关管理类
    /// </summary>
    public class WebManager
    {
        private const string versionUri = "https://minebbs-1251544790.file.myqcloud.com/bepinex.html";

        /// <summary>
        /// 获取Bepinex版本列表
        /// </summary>
        public static BepinexInfo[] GetBepinexInfos()
        {
            using HttpClient http = new();
            string data = http.GetStringAsync(versionUri).Result;
            string info = data[(data.IndexOf("[Start]") + 7)..data.IndexOf("[End]")].Replace("<br>", "").Trim();
            BepinexInfo[] infos = (from line in info.Split('\n')
                                   let item = line.Split(' ')
                                   select new BepinexInfo()
                                   {
                                       Version = item[0],
                                       IsBeta = item[1].Contains("beta"),
                                       FileName = item[2],
                                       Uri = item[3]
                                   }).ToArray();
            return infos;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="uri">文件链接</param>
        /// <param name="path">文件路径</param>
        public static async Task DownloadFromUri(string uri, string path)
        {
            using HttpClient http = new();
            using FileStream file = File.Create(path);
            using Stream reStream = await http.GetStreamAsync(uri);
            byte[] buffer = new byte[1024];
            int size;
            await Task.Run(() =>
            {
                while ((size = reStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    file.Write(buffer, 0, size);
                }
            });
        }

        /// <summary>
        /// 得到CSUL当前的最新版本
        /// </summary>
        /// <returns></returns>
        public static async Task<Version?> GetCsulVersion()
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