namespace CSUL.Models
{
    /// <summary>
    /// BepInEx版本匹配检查结果
    /// </summary>
    public enum BepInExCheckResult
    {
        /// <summary>
        /// 没有获取到BepInEx的版本
        /// </summary>
        UnknowBepInEx,

        /// <summary>
        /// 没有获取到Mod的BepInEx版本
        /// </summary>
        UnkownMod,

        /// <summary>
        /// 错误的版本
        /// </summary>
        WrongVersion,

        /// <summary>
        /// 检查通过
        /// </summary>
        Passed,
    }
}