using System;
using System.Collections.Generic;

namespace CSUL.Models.ModPlayers.BepInExMod
{
    /// <summary>
    /// BepInEx播放集
    /// </summary>
    public class BepModPlayer : BaseModPlayer
    {
        /// <summary>
        /// 初始化<see cref="BepModPlayer"/>实例
        /// </summary>
        /// <param name="sourcePath">存储该播放集数据的文件夹目录</param>
        public BepModPlayer(string sourcePath) { }

        public override void AddMod(IModData mod)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IModData> GetMods()
        {
            throw new NotImplementedException();
        }

        public override void RemoveMod(IModData mod)
        {
            throw new NotImplementedException();
        }

        public override void DisablePlayer()
        {
            throw new NotImplementedException();
        }

        public override void EnablePlayer()
        {
            throw new NotImplementedException();
        }

    }
}
