/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: PmodPlayer.cs
 *  创建时间: 2024年3月28日 0:19
 *  创建开发: 
 *  文件介绍: 
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models.Local.ModPlayer.Pmod
{
    internal class PmodPlayer : BaseModPlayer
    {
        public override ModPlayerType PlayerType => ModPlayerType.Pmod;

        public override IModData[] ModDatas => throw new NotImplementedException();

        public override Version? PlayerVersion => null;

        protected override void Initialized()
        {

        }

        public override Task AddMod(string path, IModData? data = null)
        {
            throw new NotImplementedException();
        }

        public override IModData? FirstOrDefault(Func<IModData?, bool> precidate)
        {
            throw new NotImplementedException();
        }

        public override Task<ModPlayerData> Install(string rootPath, string dataPath)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveMod(IModData modData)
        {
            throw new NotImplementedException();
        }

        public override Task UpgradeMod(IModData modData, string path, IModData? newData = null)
        {
            throw new NotImplementedException();
        }
    }
}
