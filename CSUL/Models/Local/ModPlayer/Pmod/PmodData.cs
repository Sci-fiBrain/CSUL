/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: PmodData.cs
 *  创建时间: 2024年3月28日 0:18
 *  创建开发: ScifiBrain
 *  文件介绍: Pmod模组信息
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models.Local.ModPlayer.Pmod
{
    internal class PmodData : IModData
    {
        public string Name => throw new NotImplementedException();

        public string ModPath => throw new NotImplementedException();

        public bool IsEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string? ModVersion => throw new NotImplementedException();

        public string? LatestVersion => throw new NotImplementedException();

        public string? Description => throw new NotImplementedException();

        public string? ModUrl => throw new NotImplementedException();
    }
}
