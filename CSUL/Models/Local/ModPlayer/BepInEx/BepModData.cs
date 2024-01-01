/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: BepModData.cs
 *  创建时间: 2023年12月16日 18:57
 *  创建开发: ScifiBrain
 *  文件介绍: BepInEx模组信息
 *  --------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace CSUL.Models.Local.ModPlayer.BepInEx
{
    /// <summary>
    /// BepInEx模组信息
    /// </summary>
    internal class BepModData : IModData
    {
        #region ---构造函数---

        /// <summary>
        /// 实例化<see cref="BepModData"/>对象
        /// </summary>
        public BepModData(string path)
        {
            IsFile = !DirectoryEx.IsDirectory(path);
            ModPath = path;
            Name = Path.GetFileName(path).Replace(".dll", null);

            if (File.Exists(path + ".data"))
            {
                try
                {
                    this.LoadConfig(path + ".data");
                }
                catch { }
            }
        }

        #endregion ---构造函数---

        #region ---公共属性---

        public string Name { get; set; }

        [Config]
        public int? Id { get; set; }

        public string ModPath { get; set; }

        [Config]
        public string? ModVersion { get; set; }

        public string? LatestVersion { get; set; }

        [Config]
        public string? Description { get; set; }

        [Config]
        public string? ModUrl { get; set; }

        public bool IsEnabled
        {
            get => !File.Exists(Path.Combine(ModPath + ".disabled"));
            set
            {
                string path = Path.Combine(ModPath + ".disabled");
                if (value)
                {
                    if (File.Exists(path)) File.Delete(path);
                }
                else
                {
                    File.Create(path).Dispose();
                    File.SetAttributes(path, FileAttributes.Hidden);
                }
            }
        }

        /// <summary>
        /// 是否为单文件模组
        /// </summary>
        public bool IsFile { get; set; }

        /// <summary>
        /// 当前模组的BepInEx版本
        /// </summary>
        public Version? BepVersion => GetBepVersion(ModPath, IsFile);

        #endregion ---公共属性---

        #region ---公共方法---

        /// <summary>
        /// 删除模组
        /// </summary>
        public void Delete()
        {
            if (IsFile)
            {
                if (File.Exists(ModPath)) File.Delete(ModPath);
            }
            else
            {
                if (Directory.Exists(ModPath)) Directory.Delete(ModPath, true);
            }
            if (File.Exists(ModPath + ".disabled")) File.Delete(ModPath + ".disabled");
            if (File.Exists(ModPath + ".data")) File.Delete(ModPath + ".data");
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveData() => this.SaveConfig(ModPath + ".data");

        #endregion ---公共方法---

        #region ---静态方法---

        /// <summary>
        /// 获取当前模组的BepInEx版本
        /// </summary>
        /// <returns></returns>
        private static Version? GetBepVersion(string path, bool isFile)
        {
            try
            {
                Version? version = null;
                if (isFile) TryGetBepVersionFromFile(path, out version);
                else
                {
                    if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);
                    IEnumerable<FileInfo> dlls = DirectoryEx.GetAllFiles(path).Where(x => x.Name.EndsWith(".dll"));
                    foreach (FileInfo dll in dlls) if (TryGetBepVersionFromFile(dll.FullName, out version)) break;
                }
                return version;
            }
            catch { return null; }
        }

        /// <summary>
        /// 从单个文件获取BepInEx版本
        /// </summary>
        private static bool TryGetBepVersionFromFile(string path, [NotNullWhen(true)] out Version? version)
        {
            try
            {
                if (!File.Exists(path)) throw new FileNotFoundException(path);
                AssemblyLoadContext custom = new(null, true);
                using Stream stream = File.OpenRead(path);
                Assembly assembly = custom.LoadFromStream(stream);
                AssemblyName? bep = assembly.GetReferencedAssemblies().FirstOrDefault(x =>
                    x.Name?.Contains("BepInEx") is true) ?? throw new Exception("模组没有引用BepInEx");
                version = bep.Version ?? throw new Exception("没有得到模组BepInEx的版本信息");
                custom.Unload();
                return true;
            }
            catch
            {
                version = null;
                return false;
            }
        }

        #endregion ---静态方法---

        #region ---比较方法---

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object? obj)
        {
            if (obj is not BepModData data) return false;
            else return data.GetHashCode() == GetHashCode();
        }

        #endregion ---比较方法---
    }
}