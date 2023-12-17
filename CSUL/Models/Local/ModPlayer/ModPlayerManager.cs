/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ModPlayerManager.cs
 *  创建时间: 2023年12月16日 19:27
 *  创建开发: ScifiBrain
 *  文件介绍: 模组播放集管理器
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace CSUL.Models.Local.ModPlayer
{
    /// <summary>
    /// 模组播放集管理器
    /// </summary>
    internal class ModPlayerManager
    {
        /// <summary>
        /// 数据改变事件
        /// </summary>
        public event EventHandler? OnDataChanged;

        #region ---构造函数---
        /// <summary>
        /// 实例化<see cref="ModPlayerManager"/>对象
        /// </summary>
        /// <param name="playerRootPath">播放集根目录</param>
        public ModPlayerManager(string playerRootPath)
        {
            this.playerRootPath = playerRootPath;
            string[] pathes = Directory.GetDirectories(playerRootPath);
            foreach (string playerPath in pathes)
            {
                try
                {
                    LoadModPlayer(playerPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToFormative($"路径 {playerPath}"), "播放集加载出错", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        #endregion

        #region ---私有字段---
        private readonly string playerRootPath;
        private readonly Dictionary<int, BaseModPlayer> players = new();
        #endregion

        /// <summary>
        /// 获取指定HashCode的播放集
        /// </summary>
        public BaseModPlayer? this[int hashCode]
        {
            get
            {
                players.TryGetValue(hashCode, out BaseModPlayer? modPlayer);
                return modPlayer;
            }
        }

        #region ---公共方法---

        /// <summary>
        /// 获取所有的加载器
        /// </summary>
        /// <returns></returns>
        public List<BaseModPlayer> GetModPlayers() => players.Select(x => x.Value).ToList();

        /// <summary>
        /// 加载播放集
        /// </summary>
        /// <param name="playerPath">播放集目录</param>
        /// <returns></returns>
        public void LoadModPlayer(string playerPath)
        {
            string config = Path.Combine(playerPath, "modPlayer.config");
            if (!File.Exists(config)) throw new FileNotFoundException(config);
            using Stream stream = File.OpenRead(config);
            using JsonDocument json = JsonDocument.Parse(stream);
            JsonElement root = json.RootElement;
            ModPlayerType playerType = Enum.Parse<ModPlayerType>(root.GetProperty(typeof(ModPlayerType).Name).GetString()!);
            BaseModPlayer player = playerType switch
            {
                ModPlayerType.BepInEx => new BepInEx.BepModPlayer(),
                _ => throw new Exception("未知的播放集类型")
            };
            player.Initialize(playerPath);
            int hashCode = player.GetHashCode();
            if (players.ContainsKey(hashCode)) throw new Exception("该播放集已加载");
            players.Add(hashCode, player);
        }
        #endregion
    }
}
