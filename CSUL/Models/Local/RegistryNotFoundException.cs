/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: RegistryNotFoundException.cs
 *  创建时间: 2023年12月14日 12:20
 *  创建开发: ScifiBrain
 *  文件介绍: 注册表未找到的异常
 *  --------------------------------------
 */

using System;

namespace CSUL.Models.Local
{
    /// <summary>
    /// 注册表未找到的异常
    /// </summary>
    internal class RegistryNotFoundException : Exception
    {
        /// <summary>
        /// 实例化<see cref="RegistryNotFoundException"/>对象
        /// </summary>
        /// <param name="name">未找到的注册表名称</param>
        public RegistryNotFoundException(string? name) : base($"Couldn't fint {name}") { }
    }
}