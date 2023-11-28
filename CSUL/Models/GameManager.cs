using System;
using System.Diagnostics;
using System.IO;

namespace CSUL.Models
{
    /// <summary>
    /// 游戏管理类
    /// </summary>
    public class GameManager
    {
        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="gamePath">游戏路径</param>
        public static void StartGame(string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath)) throw new ArgumentNullException(nameof(gamePath));
            if (!File.Exists(gamePath))
            {
                throw new FileNotFoundException($"游戏路径{gamePath}不存在，请检查路径设置", gamePath);
            }
            Process.Start(gamePath);
        }
    }
}