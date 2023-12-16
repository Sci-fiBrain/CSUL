/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: BaseModPlayer.cs
 *  创建时间: 2023年12月16日 17:57
 *  创建开发: ScifiBrain
 *  文件介绍: 模组播放集基类
 *  --------------------------------------
 */

namespace CSUL.Models.Local.ModPlayer
{
    /// <summary>
    /// 模组播放集基类
    /// </summary>
    internal abstract class BaseModPlayer
    {
        #region ---加载方法---

        /// <summary>
        /// 加载播放集
        /// </summary>
        /// <param name="rootPath">游戏根目录</param>
        /// <param name="dataPath">游戏数据目录</param>
        /// <param name="playerPath">播放集目录</param>
        public void Initialize(string rootPath, string dataPath, string playerPath)
        {
            RootPath = rootPath;
            DataPath = dataPath;
            PlayerPath = playerPath;
            Initialized();
        }

        #endregion ---加载方法---

        #region ---虚拟方法---

        /// <summary>
        /// 播放集加载完成后调用
        /// </summary>
        protected virtual void Initialized()
        { }

        #endregion ---虚拟方法---

        #region ---抽象方法---

        /// <summary>
        /// 启用播放集
        /// </summary>
        public abstract void Enable();

        /// <summary>
        /// 禁用播放集
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// 添加模组
        /// </summary>
        /// <param name="path">模组文件路径</param>
        public abstract void AddMod(string path);

        /// <summary>
        /// 移除模组
        /// </summary>
        /// <param name="modData"></param>
        public abstract void RemoveMod(IModData modData);

        /// <summary>
        /// 更新模组
        /// </summary>
        /// <param name="modData">要更新的模组</param>
        /// <param name="path">新版本模组文件路径</param>
        public abstract void UpgradeMod(IModData modData, string path);

        /// <summary>
        /// 禁用模组
        /// </summary>
        /// <param name="modData">要禁用的模组</param>
        public abstract void DisableMod(IModData modData);

        /// <summary>
        /// 启用模组
        /// </summary>
        /// <param name="modData">要启用的模组</param>
        public abstract void EnableMod(IModData modData);


        #endregion ---抽象方法---

        #region ---抽象属性---

        /// <summary>
        /// 当前播放集是否已启用
        /// </summary>
        public abstract bool IsEnabled { get; }

        /// <summary>
        /// 播放集类型
        /// </summary>
        public abstract ModPlayerType PlayerType { get; }

        /// <summary>
        /// 获取该播放集所有模组
        /// </summary>
        /// <returns>一个包含所有模组的数组</returns>
        public abstract IModData[] ModDatas { get; }

        #endregion ---抽象属性---

        #region ---公共属性---

        /// <summary>
        /// 游戏数据根目录
        /// </summary>
        protected string DataPath { get; private set; } = default!;

        /// <summary>
        /// 游戏安装根目录
        /// </summary>
        protected string RootPath { get; private set; } = default!;

        /// <summary>
        /// 播放集根目录
        /// </summary>
        public string PlayerPath { get; private set; } = default!;

        /// <summary>
        /// 播放集名称
        /// </summary>
        public string PlayerName => System.IO.Path.GetFileName(PlayerPath);

        #endregion ---公共属性---

        #region ---比较方法---
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            else return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() => PlayerName.GetHashCode();
        #endregion
    }
}