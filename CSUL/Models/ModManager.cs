using CSUL.Models.ModPlayers;
using CSUL.Models.ModPlayers.BepInExMod;
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
        #region ---静态变量---
        private readonly static string modPlayersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modPlayers");
        /// <summary>
        /// 获取<see cref="ModManager"/>实例
        /// </summary>
        public static ModManager Instance { get; } = new();
        #endregion

        #region ---私有字段---
        private List<IModPlayer> modPlayers = default!;
        #endregion

        #region ---构造函数---
        private ModManager()
        {
            LoadPlayers();
        }
        #endregion

        #region ---公共方法---
        public IModPlayer[] GetBaseModPlayers() => modPlayers.ToArray();
        #endregion

        #region ---私有方法---
        private void LoadPlayers()
        {
            modPlayers = new();
            if (!Directory.Exists(modPlayersPath)) return;
            string[] dirs = Directory.GetDirectories(modPlayersPath);
            foreach (string dir in dirs)
            {
                IModPlayer? player = null;
                try
                {
                    player = new BepModPlayer(dir);
                }
                catch { }
                if(player is not null) modPlayers.Add(player);
            }
        }
        #endregion
    }
}
