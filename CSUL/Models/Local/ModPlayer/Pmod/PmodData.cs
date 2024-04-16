/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: PmodData.cs
 *  创建时间: 2024年3月28日 0:18
 *  创建开发: ScifiBrain
 *  文件介绍: Pmod模组信息
 *  --------------------------------------
 */

using CSUL.Models.Local.ModPlayer.BepInEx;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace CSUL.Models.Local.ModPlayer.Pmod
{
    /// <summary>
    /// Pmod模组信息
    /// </summary>
    internal class PmodData : IModData
    {
        /// <summary>
        /// 实例化<see cref="PmodData"/>对象
        /// </summary>
        /// <param name="path">模组文件夹路径</param>
        public PmodData(string path)
        {
            ModPath = path;
            LoadModInfo();
        }

        [Config]
        public string Name { get; set; } = default!;

        [Config]
        public int? Id { get; set; }

        [Config]
        public string ModPath { get; set; } = default!;

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

        [Config]
        public string? ModVersion { get; set; }

        public string? LatestVersion => Id is int id ? Network.NetworkData.GetCbResourceData(id).Result?.ResourceVersion : null;

        [Config]
        public string? Description { get; set; }

        [Config]
        public string? ModUrl { get; set; }

        /// <summary>
        /// 保存模组数据
        /// </summary>
        public void SaveData() => this.SaveConfig(Path.Combine(ModPath, "pmod.data"));

        #region ---静态方法---

        /// <summary>
        /// 获取Pmod程序集
        /// </summary>
        public static Assembly? GetPmodAssembly(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("无效的Pmod路径: " + path);
            string[] dlls = Directory.GetFiles(path);
            foreach (string dll in dlls)
            {
                AssemblyLoadContext custom = new(null, true);
                try
                {
                    using Stream stream = File.OpenRead(dll);
                    Assembly assembly = custom.LoadFromStream(stream);
                    if (assembly.GetReferencedAssemblies().Any(x => x.Name == "Game"))
                    {
                        return assembly;
                    }
                }
                catch { }
                finally
                {
                    custom.Unload();
                }
            }
            return null;
        }

        #endregion ---静态方法---

        #region ---私有方法---

        /// <summary>
        /// 删除模组
        /// </summary>
        public void Delete()
        {
            if (Directory.Exists(ModPath)) Directory.Delete(ModPath, true);
            if (File.Exists(ModPath + ".disabled")) File.Delete(ModPath + ".disabled");
        }

        /// <summary>
        /// 加载模组信息
        /// </summary>
        private void LoadModInfo()
        {
            string path = Path.Combine(ModPath, "pmod.data");
            try
            {
                if (File.Exists(path))
                {
                    this.LoadConfig(path);
                }
                else throw new System.Exception();
            }
            catch
            {
                Name = Path.GetFileName(ModPath);
                if (GetPmodAssembly(ModPath) is Assembly assembly)
                {
                    string? assName = assembly.GetName().Name;
                    Description = assName ?? "暂无描述";
                    ModVersion = assembly.GetName().Version?.ToString();
                }
                else
                {
                    Description = "未获取到Pmod信息，该文件可能不是Pmod";
                }
            }
        }

        #endregion ---私有方法---

        #region ---比较方法---

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object? obj)
        {
            if (obj is not PmodData data) return false;
            else return data.GetHashCode() == GetHashCode();
        }

        #endregion ---比较方法---
    }
}