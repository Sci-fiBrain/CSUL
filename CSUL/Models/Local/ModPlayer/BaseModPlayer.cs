/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: BaseModPlayer.cs
 *  创建时间: 2023年12月16日 17:57
 *  创建开发: ScifiBrain
 *  文件介绍: 模组播放集基类
 *  --------------------------------------
 */

using System;
using System.Threading.Tasks;

namespace CSUL.Models.Local.ModPlayer
{
    /// <summary>
    /// 模组播放集基类
    /// </summary>
    internal abstract class BaseModPlayer
    {
        #region ---初始化方法---

        /// <summary>
        /// 初始化播放集
        /// </summary>
        /// <param name="playerPath">播放集目录</param>
        public void Initialize(string playerPath)
        {
            PlayerPath = playerPath;
            Initialized();
        }

        #endregion ---初始化方法---

        #region ---虚拟方法---

        /// <summary>
        /// 播放集初始化完成后调用
        /// </summary>
        protected virtual void Initialized()
        { }

        #endregion ---虚拟方法---

        #region ---抽象方法---

        /// <summary>
        /// 安装该播放集
        /// </summary>
        /// <param name="rootPath">游戏安装目录</param>
        /// <param name="dataPath">游戏数据目录</param>
        /// <returns></returns>
        public abstract Task<ModPlayerData> Install(string rootPath, string dataPath);

        /// <summary>
        /// 添加模组
        /// </summary>
        /// <param name="path">模组文件路径</param>
        /// <param name="data">模组信息数据</param>
        public abstract Task AddMod(string path, IModData? data = null);

        /// <summary>
        /// 移除模组
        /// </summary>
        /// <param name="modData"></param>
        public abstract Task RemoveMod(IModData modData);

        /// <summary>
        /// 更新模组
        /// </summary>
        /// <param name="modData">要更新的模组</param>
        /// <param name="path">新版本模组文件路径</param>
        public abstract Task UpgradeMod(IModData modData, string path);

        #endregion ---抽象方法---

        #region ---抽象属性---

        /// <summary>
        /// 播放集类型
        /// </summary>
        public abstract ModPlayerType PlayerType { get; }

        /// <summary>
        /// 获取该播放集所有模组
        /// </summary>
        /// <returns>一个包含所有模组的数组</returns>
        public abstract IModData[] ModDatas { get; }

        /// <summary>
        /// 播放集版本
        /// </summary>
        public abstract Version? PlayerVersion { get; }

        #endregion ---抽象属性---

        #region ---公共属性---

        /// <summary>
        /// 播放集根目录
        /// </summary>
        public string PlayerPath { get; private set; } = default!;

        /// <summary>
        /// 播放集名称
        /// </summary>
        public string PlayerName => System.IO.Path.GetFileName(PlayerPath);

        #endregion ---公共属性---

        #region ---比较方法---

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            else return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() => PlayerName.GetHashCode();

        #endregion ---比较方法---
    }
}