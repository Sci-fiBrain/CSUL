/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: BepModPlayer.cs
 *  创建时间: 2023年12月16日 18:57
 *  创建开发: ScifiBrain
 *  文件介绍: BepInEx的模组播放集
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models.Local.ModPlayer.BepInEx
{
    /// <summary>
    /// BepInEx的模组播放集
    /// </summary>
    internal class BepModPlayer : BaseModPlayer
    {
        #region ---构造函数---
        public BepModPlayer() { }

        protected override void Initialized()
        {
            bepPath = Path.Combine(PlayerPath, "BepInEx");
            modPath = Path.Combine(PlayerPath, "Mods");
            if (!Directory.Exists(bepPath)) throw new DirectoryNotFoundException("BepInEx文件未找到");
            if (!Directory.Exists(modPath)) throw new DirectoryNotFoundException("Mods文件未找到");

        }
        #endregion

        #region ---私有字段---
        private readonly List<BepModData> mods = new();
        private string bepPath = default!;
        private string modPath = default!;
        #endregion

        #region ---公共属性---

        public override ModPlayerType PlayerType => ModPlayerType.BepInEx;

        public override IModData[] ModDatas => mods.ToArray();
        #endregion

        #region ---公共方法---
        public override void AddMod(string path)
        {
            throw new NotImplementedException();
        }

        public override void Disable(string gameRoot, string dataRoot)
        {
            throw new NotImplementedException();
        }

        public override void Enable(string gameRoot, string dataRoot)
        {
            throw new NotImplementedException();
        }

        public override bool GetPlayerState(string gameRoot, string dataRoot)
        {
            throw new NotImplementedException();
        }

        public override void EnableMod(IModData modData)
        {
            if (modData is not BepModData mod) return;
        }

        public override void DisableMod(IModData modData)
        {
            if (modData is not BepModData mod) return;
        }

        public override void RemoveMod(IModData modData)
        {
            if (modData is not BepModData mod) return;
        }

        public override void UpgradeMod(IModData modData, string path)
        {
            if (modData is not BepModData mod) return;
        }
        #endregion
    }
}
