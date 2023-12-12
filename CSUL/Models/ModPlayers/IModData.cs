
namespace CSUL.Models.ModPlayers
{
    /// <summary>
    /// 模组基本信息接口
    /// </summary>
    public interface IModData
    {
        /// <summary>
        /// 模组名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 模组路径
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public string LastWriteTime { get; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disabled { get; }

        /// <summary>
        /// 删除模组
        /// </summary>
        public void Delete();
    }
}
