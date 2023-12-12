using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models.ModPlayers.BepInExMod
{
    /// <summary>
    /// BepInEx模组数据
    /// </summary>
    public class BepModData : IModData
    {
        /// <summary>
        /// 实例化<see cref="BepModData"/>对象
        /// </summary>
        /// <param name="path">模组路径</param>
        /// <param name="isFile">是否为单文件</param>
        public BepModData(string path, bool isFile) => (modPath, this.isFile) = (path, isFile);

        #region ---私有字段---
        /// <summary>
        /// 禁用标签
        /// </summary>
        private const string DisTag = "[Disabled]";
        private readonly bool isFile;
        private string modPath;
        #endregion

        #region ---公共属性---
        public string Name
        {
            get => Path.GetFileName(ModPath);
            private set
            {
                string sourcePath = Path.GetDirectoryName(ModPath)!;
                ModPath = Path.Combine(sourcePath, value);
            }
        }

        public string ModPath
        {
            get => modPath;
            private set
            {
                if (isFile)
                {
                    File.Move(ModPath, value);
                }
                else
                {
                    Directory.Move(ModPath, value);
                }
                modPath = value;
            }
        }

        public string LastWriteTime
        {
            get
            {
                if (isFile)
                {
                    return File.GetLastWriteTime(ModPath).ToString("yyyy-MM-dd-HH:mm:ss"); ;
                }
                else
                {
                    return Directory.GetLastWriteTime(ModPath).ToString("yyyy-MM-dd-HH:mm:ss"); ;
                }
            }
        }

        public bool Disabled
        {
            get => Name.StartsWith(DisTag);
            set
            {
                if (Disabled == value) return;
                if (Disabled)
                {   //禁用 -> 启用
                    Name = Name.Replace(DisTag, "");
                }
                else
                {   //启用 -> 禁用
                    Name = DisTag + Name;
                }
            }
        }

        /// <summary>
        /// 是否为单文件mod
        /// </summary>
        public bool IsFile { get => isFile; }
        #endregion

        /// <summary>
        /// 删除模组
        /// </summary>
        public void Delete()
        {
            if (isFile)
            {
                if (File.Exists(ModPath)) File.Delete(ModPath);
            }
            else
            {
                if (Directory.Exists(ModPath)) Directory.Delete(ModPath, true);
            }
        }

        #region ---比较方法---
        public override bool Equals(object? obj)
        {
            if(obj is BepModData mod)
            {
                return mod.ModPath == ModPath;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ModPath.GetHashCode();
        }
        #endregion
    }
}
