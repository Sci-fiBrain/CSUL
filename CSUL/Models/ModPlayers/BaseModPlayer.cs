using System.Collections.Generic;

namespace CSUL.Models.ModPlayers
{
    /// <summary>
    /// 模组播放集基类
    /// </summary>
    public abstract class BaseModPlayer<T> : IModPlayer where T : IModData
    {
        /// <summary>
        /// 得到mod列表
        /// </summary>
        public abstract IEnumerable<T> GetMods();

        /// <summary>
        /// 添加mod
        /// </summary>
        public abstract void AddMod(T mod);

        /// <summary>
        /// 移除mod
        /// </summary>
        public abstract void RemoveMod(T mod);

        /// <summary>
        /// 启用播放集
        /// </summary>
        public abstract void EnablePlayer();

        /// <summary>
        /// 禁用播放集
        /// </summary>
        public abstract void DisablePlayer();

        /// <summary>
        ///刷新数据
        /// </summary>
        public abstract void RefreshData();

        #region ---接口方法---
        /// <summary>
        /// 启用播放集
        /// </summary>
        void IModPlayer.EnablePlayer() => EnablePlayer();

        /// <summary>
        /// 禁用播放集
        /// </summary>
        void IModPlayer.DisablePlayer() => DisablePlayer();

        /// <summary>
        ///刷新数据
        /// </summary>
        void IModPlayer.RefreshData() => RefreshData();

        /// <summary>
        /// 得到mod列表
        /// </summary>
        IEnumerable<IModData> IModPlayer.GetMods() => (IEnumerable<IModData>)GetMods();

        /// <summary>
        /// 添加mod
        /// </summary>
        void IModPlayer.AddMod(IModData mod) => AddMod((T)mod);

        /// <summary>
        /// 移除mod
        /// </summary>
        void IModPlayer.RemoveMod(IModData mod) => RemoveMod((T)mod);
        #endregion
    }
}
