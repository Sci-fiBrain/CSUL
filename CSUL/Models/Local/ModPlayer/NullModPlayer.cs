﻿/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: NullModPlayer.cs
 *  创建时间: 2023年12月22日 2:29
 *  创建开发: ScifiBrain
 *  文件介绍: 空白播放集类
 *  --------------------------------------
 */

using System;
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

        public override Version? PlayerVersion => null;

        public override async Task AddMod(string path, IModData? data = null) => await Task.Delay(0);

        public override async Task<ModPlayerData> Install(string rootPath, string dataPath) =>
            await Task.Run(() => new ModPlayerData { Directories = default!, Files = default! });

        public override async Task RemoveMod(IModData modData) => await Task.Delay(0);

        public override async Task UpgradeMod(IModData modData, string path, IModData? newData = null) => await Task.Delay(0);

        public override IModData? FirstOrDefault(Func<IModData?, bool> precidate) => null;
    }
}