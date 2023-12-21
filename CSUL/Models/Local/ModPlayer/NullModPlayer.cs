/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: NullModPlayer.cs
 *  创建时间: 2023年12月22日 2:29
 *  创建开发: ScifiBrain
 *  文件介绍: 空白播放集类
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models.Local.ModPlayer
{
    /// <summary>
    /// 空白播放集类
    /// </summary>
    internal class NullModPlayer : BaseModPlayer
    {
        public override ModPlayerType PlayerType => ModPlayerType.None;

        public override IModData[] ModDatas => Array.Empty<IModData>();

        public override void AddMod(string path) { }

        public override void Disable(string gameRoot, string dataRoot) { }

        public override void DisableMod(IModData modData) { }

        public override void Enable(string gameRoot, string dataRoot) { }

        public override void EnableMod(IModData modData) { }

        public override bool GetPlayerState(string gameRoot, string dataRoot) => false;

        public override void RemoveMod(IModData modData) { }

        public override void UpgradeMod(IModData modData, string path) { }
    }
}
