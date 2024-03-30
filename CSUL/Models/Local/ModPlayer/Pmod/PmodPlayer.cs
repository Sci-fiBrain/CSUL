/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: PmodPlayer.cs
 *  创建时间: 2024年3月28日 0:19
 *  创建开发: ScifiBrain
 *  文件介绍: Pmod播放集
 *  --------------------------------------
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSUL.Models.Local.ModPlayer.Pmod
{
    /// <summary>
    /// Pmod播放集
    /// </summary>
    internal class PmodPlayer : BaseModPlayer
    {
        #region ---私有字段---

        private List<PmodData> mods = default!;
        private string modPath = default!;

        #endregion ---私有字段---

        #region ---构造方法---

        protected override void Initialized()
        {
            modPath = Path.Combine(PlayerPath, "Mods");
            if (!Directory.Exists(modPath)) Directory.CreateDirectory(modPath);
            mods = Directory.GetDirectories(modPath).Select(x => new PmodData(x)).ToList();
        }

        #endregion ---构造方法---

        #region ---公共属性---

        public override ModPlayerType PlayerType => ModPlayerType.Pmod;

        public override IModData[] ModDatas => mods.Order(Comparer<IModData>.Create((x, y) => string.Compare(x.Name, y.Name))).ToArray();

        public override Version? PlayerVersion => null;

        #endregion ---公共属性---

        #region ---公共方法---

        public override async Task AddMod(string path, IModData? data = null)
        {
            try
            {
                using TempDirectory package = new();
                if (path.EndsWith(".dll")) await package.AddFile(path);
                else await package.Decompress(path);
                if (package.IsEmpty) throw new Exception("该包不包含任何文件");
                string name = Path.GetFileName(path);
                name = name[..name.LastIndexOf('.')];
                if (PmodData.GetPmodAssembly(package.FullName) is null)
                {
                    if (MessageBox.Show($"文件 {name} 安装警告\n" +
                        $"未获取到Pmod信息，该文件可能不是Pmod\n" +
                        $"是否继续？", "警告",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) return;
                }
                string targetPath = Path.Combine(modPath, name);
                if (Directory.Exists(targetPath))
                {
                    if (MessageBox.Show($"模组 {name} 已存在\n" +
                        $"是否覆盖安装？", "提示",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) return;
                    Directory.Delete(targetPath, true);
                }
                package.DirectoryInfo.CopyTo(targetPath, true);
                PmodData pmodData = new(targetPath);
                mods.Add(pmodData);
                MessageBox.Show($"模组 {name} 安装完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                OnDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"模组 {path} 安装失败，原因: \n{ex.ToFormative()}", "安装出错",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public override IModData? FirstOrDefault(Func<IModData?, bool> precidate) => mods.FirstOrDefault(precidate);

        public override async Task<ModPlayerData> Install(string rootPath, string dataPath)
        {
            string pmodPath = ComParameters.Instance.Pmod.FullName;
            return await Task.Run(() =>
            {
                List<string> paths = new();
                foreach (PmodData mod in mods)
                {
                    if (!mod.IsEnabled) continue;
                    string path = Path.Combine(pmodPath, mod.Name);
                    DirectoryEx.CopyTo(mod.ModPath, path, true);
                    paths.Add(path);
                }
                return new ModPlayerData()
                {
                    Files = Array.Empty<string>(),
                    Directories = paths
                };
            });
        }

        public override async Task RemoveMod(IModData modData)
        {
            if (modData is not PmodData mod) return;
            await Task.Run(() =>
            {
                if (mods.Remove(mod))
                {
                    mods.Remove(mod);
                    OnDataChanged?.Invoke();
                }
            });
        }

        public override async Task UpgradeMod(IModData modData, string path, IModData? newData = null)
        {
            await RemoveMod(modData);
            await AddMod(path, newData);
        }

        #endregion ---公共方法---
    }
}