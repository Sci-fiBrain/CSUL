/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: GameDataManager.cs
 *  创建时间: 2024年4月17日 0:00
 *  创建开发: ScifiBrain
 *  文件介绍: 游戏数据管理方法
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models.Local.GameEx
{
    internal static class GameDataManager
    {
        private readonly static ComParameters CP = ComParameters.Instance;

        /// <summary>
        /// 退出P社账号
        /// </summary>
        public static void LogoutParadox()
        {
            string cachePath = Path.Combine(CP.GameData.FullName, ".cache");
            string pdxSdk = Path.Combine(CP.GameData.FullName, ".pdxsdk");
            if (Directory.Exists(cachePath)) Directory.Delete(cachePath, true);
            if (Directory.Exists(pdxSdk)) Directory.Delete(pdxSdk, true);
        }

        /// <summary>
        /// 刷新Pmod文件数据
        /// </summary>
        public static async Task ReloadPmodData()
        {
            await Task.Run(() =>
            {
                string modPath = CP.Pmod.FullName;
                string cachePath = Path.Combine(CP.GameData.FullName, ".cache");
                using TempDirectory temp = new();
                if (Directory.Exists(modPath))
                {
                    DirectoryEx.CopyTo(modPath, Path.Combine(temp.FullName, "Mods"));
                    Directory.Delete(modPath, true);
                }
                if (Directory.Exists(cachePath))
                {
                    DirectoryEx.CopyTo(cachePath, Path.Combine(temp.FullName, ".cache"));
                    Directory.Delete(cachePath, true);
                }
                temp.DirectoryInfo.CopyTo(CP.GameData.FullName);
            });
        }
    }
}
