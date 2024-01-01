/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ViceCommand.cs
 *  创建时间: 2023年12月16日 9:45
 *  创建开发: ScifiBrain
 *  文件介绍: 副进程传参处理
 *  --------------------------------------
 */

using CSUL.Models.Network;
using CSUL.Models.Network.CB;
using CSUL.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;
using ParPairs = System.Collections.Generic.Dictionary<string, string>;

namespace CSUL
{
    /// <summary>
    /// 副进程传参处理
    /// </summary>
    internal static class ViceCommand
    {
        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// 解析命令
        /// </summary>
        public static async Task Parse(string command, Dispatcher dispatcher)
        {
            try
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
                    case "installresources": await InstallResources(pars, dispatcher); break;
                }
            }
            catch { }
        }

        #region ---私有方法---

        /// <summary>
        /// 安装资源
        /// </summary>
        private static async Task InstallResources(ParPairs pairs, Dispatcher dispatcher)
        {
            if (!pairs.TryGetValue("json", out string? str)) return;
            using JsonDocument json = JsonDocument.Parse(str);
            //获取需要安装的所有资源id
            IEnumerable<int> ids = json.RootElement.EnumerateArray().Select(x => x.ValueKind == JsonValueKind.String ? int.Parse(x.GetString()!) : x.GetInt32());
            List<CbResourceData> datas = await NetworkData.GetCbResourceDatas(ids);
            await dispatcher.InvokeAsync(() =>
            {   //同步线程打开UI
                try
                {
                    CbResourceInstaller installer = new(datas);
                    installer.Show();
                }
                catch { }
            });
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

        #endregion ---私有方法---
    }
}