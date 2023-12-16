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
            name = Path.GetFileName(PlayerPath);
        }
        #endregion

        #region ---私有字段---
        private string name = default!;
        private bool isEnabled = default!;
        #endregion

        #region ---公共属性---
        public override bool IsEnabled => isEnabled;

        public override string Name => name;

        public override ModPlayerType PlayerType => ModPlayerType.BepInEx;

        public override IModData[] ModDatas => new BepModData[]
        {
            new BepModData(){Name = "模组A"},
            new BepModData(){Name = "模组B"},
            new BepModData(){Name = "模组C"},
        };
        #endregion

        #region ---公共方法
        public override void AddMod(string path)
        {
            throw new NotImplementedException();
        }

        public override void Disable()
        {
            throw new NotImplementedException();
        }

        public override void Enable()
        {
            throw new NotImplementedException();
        }

        public override void EnableMod(IModData modData)
        {
            throw new NotImplementedException();
        }

        public override void DisableMod(IModData modData)
        {
            throw new NotImplementedException();
        }

        public override void RemoveMod(IModData modData)
        {
            throw new NotImplementedException();
        }

        public override void UpgradeMod(IModData modData, string path)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
