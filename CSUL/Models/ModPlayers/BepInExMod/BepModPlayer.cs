using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSUL.Models.ModPlayers.BepInExMod
{
    /// <summary>
    /// BepInEx播放集
    /// </summary>
    public class BepModPlayer : BaseModPlayer<BepModData>
    {
        #region ---构造函数---
        /// <summary>
        /// 初始化<see cref="BepModPlayer"/>实例
        /// </summary>
        /// <param name="sourcePath">存储该播放集数据的文件夹目录</param>
        public BepModPlayer(string sourcePath)
        {
            this.sourcePath = sourcePath;
            RefreshData();
        }
        #endregion

        private readonly string sourcePath;
        private List<BepModData> modDatas = new();

        /// <summary>
        /// BepInEx文件夹路径
        /// </summary>
        public string BepInExPath { get; set; } = default!;
        public string PluginPath { get => Path.Combine(BepInExPath, "plugins"); }
        public List<BepModData> ModDatas { get { RefreshData(); return modDatas; } }

        public override void AddMod(BepModData mod)
        {
            if (Path.GetDirectoryName(mod.ModPath) == sourcePath) throw new Exception("不可循环添加模组");
            string targetPath = Path.Combine(sourcePath, mod.Name);
            if (mod.IsFile ? File.Exists(targetPath) : Directory.Exists(targetPath)) throw new Exception("该模组已存在");
            if (mod.IsFile)
            {
                FileInfo file = new(mod.ModPath);
                if (file.Exists) file.CopyTo(targetPath, true);
            }
            else
            {
                DirectoryInfo dir = new(mod.ModPath);
                if(dir.Exists) dir.CopyTo(targetPath, true);
            }
            RefreshData();
        }

        public override IEnumerable<BepModData> GetMods()
        {
            return ModDatas.ToArray();
        }

        public override void RemoveMod(BepModData mod)
        {
            ModDatas.Find(x => x.Equals(mod));
            mod.Delete();
            RefreshData();
        }

        public override void DisablePlayer()
        {
            DirectoryInfo plugin = new(PluginPath);
            if (!plugin.Exists) return;
            string hash = GetHashCode().ToString();
            foreach (FileInfo file in plugin.GetFiles())
            {
                if (file.Name.StartsWith(hash)) file.Delete();
            }
            foreach (DirectoryInfo dir in plugin.GetDirectories())
            {
                if (dir.Name.StartsWith(hash)) dir.Delete(true);
            }
        }

        public override void EnablePlayer()
        {
            DirectoryInfo plugin = new(PluginPath);
            if (!plugin.Exists) plugin.Create();
            string hash = GetHashCode().ToString();
            foreach (BepModData mod in ModDatas)
            {
                if (mod.Disabled) continue;
                string targetPath = Path.Combine(plugin.FullName, hash + mod.Name);
                if (mod.IsFile)
                {
                    FileInfo file = new(targetPath);
                    if (file.Exists) file.CopyTo(targetPath, true);
                }
                else
                {
                    DirectoryInfo dir = new(targetPath);
                    if(dir.Exists) dir.CopyTo(targetPath, true);
                }
            }
        }

        public override void RefreshData()
        {
            if (Directory.Exists(sourcePath))
            {
                modDatas = new();
                IEnumerable<string> paths = Directory.GetDirectories(sourcePath);
                foreach (string dir in paths)
                {   //文件夹型
                    modDatas.Add(new BepModData(dir, false));
                }
                paths = Directory.GetFiles(sourcePath).Where(x => x.EndsWith(".dll"));
                foreach (string file in paths)
                {   //dll型
                    modDatas.Add(new BepModData(file, true));
                }
            }
        }

        #region ---比较方法---
        public override bool Equals(object? obj)
        {
            if (obj is BepModPlayer player)
            {
                return player.sourcePath.Equals(sourcePath);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return sourcePath.GetHashCode();
        }
        #endregion
    }
}
