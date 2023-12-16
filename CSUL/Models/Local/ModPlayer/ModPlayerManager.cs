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
        /// <param name="rootPath">游戏根目录</param>
        /// <param name="dataPath">游戏数据目录</param>
        /// <param name="playerRootPath">播放集根目录</param>
        public ModPlayerManager(string rootPath, string dataPath, string playerRootPath)
        {
            (this.rootPath, this.dataPath, this.playerRootPath) = (rootPath, dataPath, playerRootPath);
            string[] pathes = Directory.GetDirectories(playerRootPath);
            foreach (string playerPath in pathes)
            {
                try
                {
                    BaseModPlayer modPlayer = LoadModPlayer(playerPath);
                    players.Add(modPlayer.GetHashCode(), modPlayer);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToFormative($"路径 {playerPath}"), "播放集加载出错", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        #endregion

        #region ---私有字段---
        private readonly string rootPath;
        private readonly string dataPath;
        private readonly string playerRootPath;
        private readonly Dictionary<int, BaseModPlayer> players = new();
        #endregion

        #region ---公共方法---
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
        
        /// <summary>
        /// 获取所有的加载器
        /// </summary>
        /// <returns></returns>
        public List<BaseModPlayer> GetModPlayers() => players.Select(x => x.Value).ToList();

        /// <summary>
        /// 创建新的模组播放集
        /// </summary>
        /// <param name="name">播放集名称</param>
        /// <param name="playerType">播放集类型</param>
        /// <returns>该播放集的HashCode</returns>
        public int CreatNewModPlayer(string name, ModPlayerType playerType)
        {
            string path = Path.Combine(playerRootPath, name);
            if (Directory.Exists(path)) throw new Exception("该名称的播放集已存在");
            else Directory.CreateDirectory(path);
            string config = Path.Combine(path, "modPlayer.config");
            using Stream stream = File.Create(config);
            using Utf8JsonWriter json = new(stream);
            json.WriteStartObject();
            json.WriteString(typeof(ModPlayerType).Name, playerType.ToString());
            json.WriteEndObject();
            BaseModPlayer player = playerType switch
            {
                ModPlayerType.BepInEx => new BepInEx.BepModPlayer(),
                _ => throw new Exception("未知的播放集类型")
            };
            player.Initialize(rootPath, dataPath, path);
            int hashCode = player.GetHashCode();
            players.Add(hashCode, player);
            Task.Run(() => OnDataChanged?.Invoke(this, EventArgs.Empty));
            return hashCode;
        }
        #endregion

        #region ---私有方法---
        /// <summary>
        /// 加载播放集
        /// </summary>
        /// <param name="rootPath">游戏根目录</param>
        /// <param name="dataPath">游戏数据目录</param>
        /// <param name="playerPath">播放集目录</param>
        /// <returns></returns>
        private BaseModPlayer LoadModPlayer(string playerPath)
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
            player.Initialize(rootPath, dataPath, playerPath);
            return player;
        }
        #endregion
    }
}
