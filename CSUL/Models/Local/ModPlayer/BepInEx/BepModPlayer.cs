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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSUL.Models.Local.ModPlayer.BepInEx
{
    /// <summary>
    /// BepInEx的模组播放集
    /// </summary>
    internal class BepModPlayer : BaseModPlayer
    {
        #region ---构造函数---

        public BepModPlayer()
        { }

        protected override void Initialized()
        {
            bepPath = Path.Combine(PlayerPath, "BepInEx");
            modPath = Path.Combine(PlayerPath, "Mods");
            if (!Directory.Exists(bepPath)) throw new DirectoryNotFoundException("BepInEx文件未找到");
            if (!Directory.Exists(modPath)) throw new DirectoryNotFoundException("Mods文件未找到");
            mods.AddRange(Directory.GetFiles(modPath).Where(x => x.EndsWith(".dll")).Select(x => new BepModData(x)));
            mods.AddRange(Directory.GetDirectories(modPath).Select(x => new BepModData(x)));
        }

        #endregion ---构造函数---

        #region ---私有字段---

        /// <summary>
        /// 一个包含所有mod的集合
        /// </summary>
        private readonly List<BepModData> mods = new();

        /// <summary>
        /// BepInEx文件夹路径
        /// </summary>
        private string bepPath = default!;

        /// <summary>
        /// Mods文件夹路径
        /// </summary>
        private string modPath = default!;

        #endregion ---私有字段---

        #region ---公共属性---

        public override ModPlayerType PlayerType => ModPlayerType.BepInEx;

        public override IModData[] ModDatas => mods.ToArray();

        public override Version? PlayerVersion => GetBepVersion();

        #endregion ---公共属性---

        #region ---公共方法---

        public override async Task AddMod(string path)
        {
            try
            {
                using TempDirectory package = new();
                if (path.EndsWith(".dll")) await package.AddFile(path);
                else await package.Decompress(path);
                if (package.IsEmpty) throw new Exception("该包不含任何文件");
                string name = Path.GetFileName(path);
                name = name[..name.LastIndexOf('.')];
                BepModData originData = new(package.FullName);
                Version? bepVersion = PlayerVersion, modVersion = originData.BepVersion;
                if (bepVersion is null || modVersion is null)
                {
                    if (MessageBox.Show($"文件 {name} 安装警告\n" +
                            $"BepInEx版本: {(bepVersion is null ? "未能获取已安装BepInEx的版本信息" : bepVersion)}\n" +
                            $"模组版本: {(modVersion is null ? "未能成功获取模组BepInEx版本" : modVersion)}\n" +
                            "是否继续？", "警告",
                            MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) return;
                }
                else if (bepVersion.Major != modVersion.Major)
                {
                    if (MessageBox.Show($"模组 {name} 安装警告" +
                            $"但模组版本与BepInEx不符\n" +
                            $"BepInEx版本: {bepVersion}\n" +
                            $"插件版本: {modVersion}\n" +
                            $"该情况可能会引发兼容性问题\n" +
                            $"是否继续？", "警告",
                            MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) return;
                }
                string targetDir = modPath;
                string targetPath;
                if (package.OnlySingleFile)
                {
                    targetPath = Path.Combine(targetDir, $"{name}.dll");
                    if (File.Exists(targetPath))
                    {
                        if (MessageBox.Show($"模组{name}.dll已存在\n" +
                            $"是否覆盖安装？", "提示",
                            MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel) return;
                    }
                    File.Copy(path, targetPath, true);
                }
                else
                {
                    targetPath = Path.Combine(targetDir, name);
                    if (Directory.Exists(targetPath))
                    {
                        if (MessageBox.Show($"模组{name}已存在\n" +
                            $"是否覆盖安装？", "提示",
                            MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel) return;
                        Directory.Delete(targetPath);
                    }
                    Directory.CreateDirectory(targetPath);
                    package.DirectoryInfo.CopyTo(targetPath, true);
                }
                BepModData modData = new(targetPath);
                if (mods.Contains(modData)) mods.Remove(modData);
                mods.Add(modData);
                MessageBox.Show($"模组 {name} 安装完成\n兼容性检查已完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show($"插件{path}安装失败，原因: \n{e.ToFormative()}", "安装出错",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public override async Task RemoveMod(IModData modData)
        {
            if (modData is not BepModData mod) return;
            await Task.Run(() =>
            {
                if (mods.Remove(mod))
                {
                    mod.IsEnabled = true;
                    if (mod.IsFile) File.Delete(mod.ModPath);
                    else Directory.Delete(mod.ModPath, true);
                }
            });
        }

        public override async Task<ModPlayerData> Install(string rootPath, string dataPath)
        {
            return await Task.Run(() =>
            {
                DirectoryEx.CopyTo(bepPath, rootPath, true);
                string plugins = Path.Combine(rootPath, "BepInEx", "plugins");
                foreach (BepModData mod in mods)
                {
                    if (!mod.IsEnabled) continue;
                    if (mod.IsFile) File.Copy(mod.ModPath, Path.Combine(plugins, mod.Name + ".dll"));
                    else DirectoryEx.CopyTo(mod.ModPath, Path.Combine(plugins, mod.Name));
                }
                return new ModPlayerData()
                {
                    Files = Directory.GetFiles(bepPath).Select(x => Path.Combine(rootPath, Path.GetFileName(x))),
                    Directories = new string[]
                    {
                        plugins,
                        Path.Combine(rootPath, "BepInEx", "core"),
                    }
                };
            });
        }

        public override async Task UpgradeMod(IModData modData, string path)
        {
            if (modData is not BepModData mod) return;
        }

        #endregion ---公共方法---

        #region ---私有方法---

        /// <summary>
        /// 获取当前播放集的BepInEx版本
        /// </summary>
        /// <returns></returns>
        private Version? GetBepVersion()
        {
            try
            {
                string corePath = Path.Combine(PlayerPath, "BepInEx", "BepInEx", "core");
                if (!Directory.Exists(corePath)) return null;
                string? bepCore = Directory.GetFiles(corePath).Select(Path.GetFileName)
                    .FirstOrDefault(x => x!.StartsWith("BepInEx") && x!.EndsWith(".dll"));
                if (bepCore is null) return null;
                FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(Path.Combine(corePath, bepCore));
                string? ver = fileVersion.FileVersion;
                if (ver is null) return null;
                return Version.Parse(ver);
            }
            catch { return null; }
        }

        #endregion ---私有方法---
    }
}