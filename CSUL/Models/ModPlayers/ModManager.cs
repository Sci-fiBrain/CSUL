using CSUL.Models.ModPlayers.BepInExMod;
using System;
using System.Collections.Generic;
using System.IO;

namespace CSUL.Models.ModPlayers
{
    /// <summary>
    /// 模组管理器
    /// </summary>
    public class ModManager
    {
        #region ---静态变量---

        private static readonly string modPlayersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modPlayers");

        /// <summary>
        /// 获取<see cref="ModManager"/>实例
        /// </summary>
        public static ModManager Instance { get; } = new();

        #endregion ---静态变量---

        #region ---私有字段---

        private List<IModPlayer> modPlayers = default!;

        #endregion ---私有字段---

        #region ---构造函数---

        private ModManager()
        {
            LoadPlayers();
        }

        #endregion ---构造函数---

        #region ---公共方法---

        public IModPlayer[] GetBaseModPlayers() => modPlayers.ToArray();

        #endregion ---公共方法---

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
                if (player is not null) modPlayers.Add(player);
            }
        }

        #endregion ---私有方法---
    }
}