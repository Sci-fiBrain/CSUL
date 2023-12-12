using System;
using System.Collections.Generic;

namespace CSUL.Models.ModPlayers.BepInExMod
{
    /// <summary>
    /// BepInEx播放集
    /// </summary>
    public class BepModPlayer : BaseModPlayer<BepModData>
    {
        public override void AddMod(BepModData mod)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<BepModData> GetMods()
        {
            throw new NotImplementedException();
        }

        public override void RemoveMod(BepModData mod)
        {
            throw new NotImplementedException();
        }
    }
}
