using CSUL.Models.ModPlayers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models
{
    /// <summary>
    /// 模组管理器
    /// </summary>
    public class ModManager
    {
        private readonly static string modPlayersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modPlayers");

        /// <summary>
        /// 获取<see cref="ModManager"/>实例
        /// </summary>
        public static ModManager Instance { get; } = new();

        private ModManager() { }

        public BaseModPlayer[] GetBaseModPlayers()
        {

        }
    }
}
