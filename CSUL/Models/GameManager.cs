using System;
using System.Diagnostics;
using System.IO;

namespace CSUL.Models
{
    /// <summary>
    /// 游戏管理类
    /// </summary>
    public class GameManager : IDisposable
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSUL_Game.config");

        public static GameManager Instance { get; } = new();

        #region ---构造函数---
        private GameManager()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    this.LoadConfig(ConfigPath);
                }
                catch { }
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.SaveConfig(ConfigPath);
        }
        #endregion

        #region ---公共属性---
        /// <summary>
        /// 启动参数
        /// </summary>
        [Config]
        public string StartArguemnt { get; set; }

        /// <summary>
        /// 是否以开发模式启动
        /// </summary>
        [Config]
        public bool OpenDeveloper { get; set; }
        #endregion

        #region ---静态方法---
        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="gamePath">游戏路径</param>
        public static void StartGame(string gamePath, string? arguments = null)
        {
            if (string.IsNullOrEmpty(gamePath)) throw new ArgumentNullException(nameof(gamePath));
            if (!File.Exists(gamePath))
            {
                throw new FileNotFoundException($"游戏路径{gamePath}不存在，请检查路径设置", gamePath);
            }
            if (arguments is null)
            {
                Process.Start(gamePath);
            }
            else
            {
                Process.Start(gamePath, arguments);
            }
        }
        #endregion
    }
}