﻿/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ViceCommand.cs
 *  创建时间: 2023年12月16日 9:45
 *  创建开发: ScifiBrain
 *  文件介绍: 副进程传参处理
 *  --------------------------------------
 */

using CSUL.Models.Network.CB;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ParPairs = System.Collections.Generic.Dictionary<string, string>;

namespace CSUL
{
    /// <summary>
    /// 副进程传参处理
    /// </summary>
    internal class ViceCommand
    {
        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private ViceCommand()
        { }

        /// <summary>
        /// 解析命令
        /// </summary>
        public static async Task Parse(string command)
        {
            int index = command.IndexOf('?');
            Queue<string> keys;
            ParPairs pars;
            if (index == -1 || index + 1 == command.Length)
            {   //无参数
                keys = new(command.Split("/").Where(x => x.Length > 0));
                pars = new();
            }
            else
            {   //有参数
                keys = new(command[..index].Split("/").Where(x => x.Length > 0));
                IEnumerable<string> data = command[(index + 1)..].Split('&').Where(x => x.Length > 0);
                pars = await GetDicFromData(data);
            }
            switch (keys.Dequeue())
            {
                case "installbepmod": await InstallBepMod(pars); break;
                case "installbepmods": await InstallBepMods(pars); break;
            }
        }

        #region ---私有方法---

        /// <summary>
        /// 安装BepInEx模组
        /// </summary>
        private static async Task InstallBepMod(ParPairs pairs)
        {
            if (!pairs.TryGetValue("json", out string? json)) return;


        }

        /// <summary>
        /// 安装多个模组
        /// </summary>
        private static async Task InstallBepMods(ParPairs pairs)
        {
            if (!pairs.TryGetValue("json", out string? json)) return;

        }

        /// <summary>
        /// 从参数数据得到字典
        /// </summary>
        private static async Task<ParPairs> GetDicFromData(IEnumerable<string> data)
        {
            return await Task.Run(() =>
            {
                IEnumerable<KeyValuePair<string, string>> pairs = data.Select(x =>
                {
                    int index = x.IndexOf('=');
                    if (index == -1) return default;
                    if (index == x.Length - 1) return default;
                    return KeyValuePair.Create(x[..index], x[(index + 1)..]);
                });
                return new ParPairs(pairs);
            });
        }

        /// <summary>
        /// 转换json字符串
        /// </summary>
        private static async Task<T?> ConvertJson<T>(string json)
        {
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
            return await JsonSerializer.DeserializeAsync<T>(stream, options);
        }

        #endregion ---私有方法---
    }
}