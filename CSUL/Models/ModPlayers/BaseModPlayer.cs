using System.Collections.Generic;

namespace CSUL.Models.ModPlayers
{
    /// <summary>
    /// 模组播放集基类
    /// </summary>
    public abstract class BaseModPlayer<T> where T : IModData
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
    }
}
