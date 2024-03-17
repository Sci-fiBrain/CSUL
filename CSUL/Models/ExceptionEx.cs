/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ExceptionEx.cs
 *  创建时间: 2023年11月26日 12:47
 *  创建开发: ScifiBrain
 *  文件介绍: Exception扩展类 提供Exception的扩展方法
 *  --------------------------------------
 */

using System;
using System.Reflection;
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
            if (meg is not null)
            {
                builder.AppendLine($"附加信息: \n{meg}");
            }
            if (ex is UnauthorizedAccessException)
            {
                builder.AppendLine($"异常信息: 该操作缺少权限，可能被安全软件拦截或是需要管理员权限");
            }
            else if (ex is System.Net.Http.HttpRequestException httpEx)
            {
                builder.AppendLine($"异常信息: 网站访问失败 错误码{(int?)httpEx.StatusCode}({httpEx.StatusCode})");
            }
            if (ex is not null)
            {
                builder.AppendLine($"异常内容: {ex.Message}");
                if (ex.InnerException is Exception inner)
                {
                    builder.AppendLine($"内敛信息: {inner.Message}");
                    builder.AppendLine($"内敛类型: {inner.GetType().Name}");
                }
                builder.AppendLine();
                builder.AppendLine("如遇无法解决的问题，请将该报错完整截图后再咨询");
                builder.AppendLine("以下为开发者所需信息，无需用户处理");
                builder.Append('-', 60).AppendLine();
                builder.AppendLine($"软件版本: {Assembly.GetExecutingAssembly().GetName().Version}");
                builder.AppendLine($"异常类型: {ex.GetType().Name}");
                builder.AppendLine($"异常对象: {ex.Source}");
                builder.AppendLine($"异常方法: {ex.TargetSite?.Name}");
                builder.AppendLine($"堆栈信息: \n{ex.StackTrace}");
            }
            return builder.ToString();
        }
    }
}