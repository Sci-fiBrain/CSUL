/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ExceptionEx.cs
 *  创建时间: 2023年11月26日 12:47
 *  创建开发: ScifiBrain
 *  文件介绍: Exception扩展类 提供Exception的扩展方法
 *  --------------------------------------
 */

using System;
using System.Text;

namespace CSUL.Models
{
    /// <summary>
    /// Exception扩展类 提供Exception的扩展方法
    /// </summary>
    public static class ExceptionEx
    {
        /// <summary>
        /// 格式化异常信息
        /// </summary>
        public static string ToFormative(this Exception? ex, string? meg = null)
        {
            StringBuilder builder = new();
            builder.AppendLine($"发生时间: {DateTime.Now:yyyy_MM_dd HH:mm:ss:ffff}");
            if (ex is not null)
            {
                builder.AppendLine($"异常类型: {ex.GetType().Name}");
                builder.AppendLine($"异常对象: {ex.Source}");
                builder.AppendLine($"异常方法: {ex.TargetSite?.Name}");
                builder.AppendLine($"堆栈信息: \n{ex.StackTrace}");
                builder.AppendLine($"异常内容: {ex.Message}");
            }
            if (meg is not null)
            {
                builder.AppendLine($"附加信息: \n{meg}");
            }
            return builder.ToString();
        }
    }
}