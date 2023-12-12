using System.Collections.Generic;

namespace CSUL.Models.ModPlayers
{
    /// <summary>
    /// 模组播放集基类
    /// </summary>
    public abstract class BaseModPlayer
    {
        /// <summary>
        /// 得到mod列表
        /// </summary>
        public abstract IEnumerable<IModData> GetMods();

        /// <summary>
        /// 添加mod
        /// </summary>
        public abstract void AddMod(IModData mod);

        /// <summary>
        /// 移除mod
        /// </summary>
        public abstract void RemoveMod(IModData mod);

        /// <summary>
        /// 启用播放集
        /// </summary>
        public abstract void EnablePlayer();

        /// <summary>
        /// 禁用播放集
        /// </summary>
        public abstract void DisablePlayer();
    }
}
