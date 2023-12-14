using System.Collections.Generic;

namespace CSUL.Models.ModPlayers
{
    /// <summary>
    /// 模组播放集接口
    /// </summary>
    public interface IModPlayer
    {
        /// <summary>
        /// 得到mod列表
        /// </summary>
        public IEnumerable<IModData> GetMods();

        /// <summary>
        /// 添加mod
        /// </summary>
        public void AddMod(IModData mod);

        /// <summary>
        /// 移除mod
        /// </summary>
        public void RemoveMod(IModData mod);

        /// <summary>
        /// 启用播放集
        /// </summary>
        public void EnablePlayer();

        /// <summary>
        /// 禁用播放集
        /// </summary>
        public void DisablePlayer();

        /// <summary>
        ///刷新数据
        /// </summary>
        public void RefreshData();
    }
}