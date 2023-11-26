using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Models
{
    public class ExceptionManager
    {
        /// <summary>
        /// 得到异常信息
        /// </summary>
        public static string GetExMeg(Exception? ex, string? meg = null)
        {
            StringBuilder builder = new();
            builder.AppendLine($"发生时间: {DateTime.Now:yyyy_MM_dd HH:mm:ss:ffff}");
            if(ex is not null)
            {
                builder.AppendLine($"异常对象: {ex.Source}");
                builder.AppendLine($"异常方法: {ex.TargetSite?.Name}");
                builder.AppendLine($"堆栈信息: \n{ex.StackTrace}");
                builder.AppendLine($"异常内容: {ex.Message}");
            }
            if(meg is not null)
            {
                builder.AppendLine($"附加信息: \n{meg}");
            }
            return builder.ToString();
        }
    }
}
